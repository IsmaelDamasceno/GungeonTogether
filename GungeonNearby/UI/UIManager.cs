using System.Collections;
using GungeonNearby.Core;
using GungeonNearby.Networking;
using GungeonNearby.Networking.Lan;
using GungeonNearby.Networking.Steam;
using GungeonNearby.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = GungeonNearby.Systems.Logging.Debug;

namespace GungeonNearby.UI
{
    public static class UIManager
    {
        private const int LanDefaultPort = 7777;

        private static GameObject _root;

        // Main panel
        private static GameObject _mainPanel;

        // Steam panel
        private static GameObject _steamPanel;
        private static Text _steamStatusText;

        // LAN panel
        private static GameObject _lanPanel;
        private static Text _lanStatusText;
        private static InputField _bindPortField;
        private static InputField _ipField;

        private static bool _built;

        public static void Initialise()
        {
            GungeonNearbyMod.Instance.StartCoroutine(ActivateOnFoyer());
        }

        private static IEnumerator ActivateOnFoyer()
        {
            Debug.Log("[UI] Waiting for Foyer...");
            yield return FoyerUtils.WaitForFoyer();
            Debug.Log("[UI] Foyer initialized, ensuring built...");
            EnsureBuilt();
            Debug.Log("[UI] Setting visibility");
            SetVisible(true);
        }

        public static void Update()
        {
            try
            {
                if (GameManager.Instance == null)
                {
                    return;
                }

                if (GameManager.Instance.IsFoyer)
                {
                    HandleVisibilityToggle();
                    UpdateStatus();
                }
                else
                {
                    SetVisible(false);
                }
            }
            catch { }
        }

        private static void HandleVisibilityToggle()
        {
            if (!_root)
            {
                return;
            }

            if (InputUtils.IsKeyCtrl() && Input.GetKeyDown(KeyCode.P))
            {
                SetVisible(!_root.activeSelf);
            }
        }

        private static void EnsureBuilt()
        {
            if (_built)
            {
                return;
            }

            var canvasGo = new GameObject("GungeonNearby_Canvas");
            Object.DontDestroyOnLoad(canvasGo);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGo.AddComponent<GraphicRaycaster>();
            _root = canvasGo;

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("GungeonNearby_EventSystem");
                Object.DontDestroyOnLoad(esGo);
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            BuildMainPanel();
            BuildSteamPanel();
            BuildLanPanel();

            ShowMain();
            _built = true;
        }

        // ── Main panel ──────────────────────────────────────────────

        private static void BuildMainPanel()
        {
            _mainPanel = CreatePanel("GN_MainPanel", 420, 120);
            CreateLabel(
                _mainPanel,
                "GUNGEON TOGETHER",
                0,
                35,
                400,
                30,
                18,
                TextAnchor.MiddleCenter
            );

            float bw = 190f;
            var steamBtn = CreateButton(_mainPanel, "STEAM", -105, -20, bw, 40);
            var lanBtn = CreateButton(_mainPanel, "LAN", 105, -20, bw, 40);

            steamBtn.onClick.AddListener(() =>
            {
                NetworkManager.Instance.InitialiseSteam();
                ShowSteam();
            });
            lanBtn.onClick.AddListener(() => ShowLan());
        }

        // ── Steam panel ─────────────────────────────────────────────

        private static void BuildSteamPanel()
        {
            _steamPanel = CreatePanel("GN_SteamPanel", 420, 220);
            _steamStatusText = CreateLabel(
                _steamPanel,
                "",
                0,
                65,
                400,
                50,
                13,
                TextAnchor.MiddleCenter
            );

            float bw = 190f;
            var hostBtn = CreateButton(_steamPanel, "HOST LOBBY", -105, 20, bw, 40);
            var inviteBtn = CreateButton(_steamPanel, "INVITE", 105, 20, bw, 40);
            var leaveBtn = CreateButton(_steamPanel, "LEAVE", -105, -30, bw, 40);
            var backBtn = CreateButton(_steamPanel, "BACK", 105, -30, bw, 40);

            hostBtn.onClick.AddListener(() => SteamLobbyManager.Instance.CreateLobby(4));
            inviteBtn.onClick.AddListener(() => SteamLobbyManager.Instance.OpenInviteDialog());
            leaveBtn.onClick.AddListener(() =>
            {
                SteamLobbyManager.Instance.LeaveLobby();
                NetworkManager.Instance.Shutdown();
            });
            backBtn.onClick.AddListener(() => ShowMain());
        }

        // ── LAN panel ───────────────────────────────────────────────

        private static void BuildLanPanel()
        {
            _lanPanel = CreatePanel("GN_LanPanel", 420, 300);
            _lanStatusText = CreateLabel(
                _lanPanel,
                "",
                0,
                120,
                400,
                40,
                13,
                TextAnchor.MiddleCenter
            );

            _bindPortField = CreateInputField(
                _lanPanel,
                GetLocalLanPort().ToString(),
                "Bind port",
                0,
                70,
                400,
                35
            );
            _ipField = CreateInputField(
                _lanPanel,
                "127.0.0.1:7777",
                "Host IP:PORT",
                0,
                25,
                400,
                35
            );

            float bw = 190f;
            var hostBtn = CreateButton(_lanPanel, "HOST", -105, -25, bw, 40);
            var joinBtn = CreateButton(_lanPanel, "JOIN", 105, -25, bw, 40);
            var leaveBtn = CreateButton(_lanPanel, "LEAVE", -105, -75, bw, 40);
            var backBtn = CreateButton(_lanPanel, "BACK", 105, -75, bw, 40);

            hostBtn.onClick.AddListener(OnLanHostClicked);
            joinBtn.onClick.AddListener(OnLanJoinClicked);
            leaveBtn.onClick.AddListener(() => NetworkManager.Instance.Shutdown());
            backBtn.onClick.AddListener(() => ShowMain());
        }

        // ── Button handlers ──────────────────────────────────────────

        private static void OnLanHostClicked()
        {
            int port = LanDefaultPort;
            if (int.TryParse(_bindPortField?.text?.Trim(), out int p))
                port = p;
            NetworkManager.Instance.InitialiseLan(port);
            NetworkManager.Instance.StartHosting();
        }

        private static void OnLanJoinClicked()
        {
            string input = _ipField?.text?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                Debug.LogWarning("[UI] LAN join: no IP entered.");
                return;
            }

            string ip;
            int hostPort = LanDefaultPort;
            int colon = input.LastIndexOf(':');
            if (colon >= 0 && int.TryParse(input.Substring(colon + 1), out int pp))
            {
                ip = input.Substring(0, colon);
                hostPort = pp;
            }
            else
            {
                ip = input;
            }

            int localPort = LanDefaultPort;
            if (int.TryParse(_bindPortField?.text?.Trim(), out int bp))
                localPort = bp;

            NetworkManager.Instance.InitialiseLan(localPort);
            ulong hostId = LanTransport.EncodeEndpoint(ip, hostPort);
            NetworkManager.Instance.ConnectTo(hostId);
        }

        // ── Panel visibility ─────────────────────────────────────────

        private static void ShowMain()
        {
            _mainPanel?.SetActive(true);
            _steamPanel?.SetActive(false);
            _lanPanel?.SetActive(false);
        }

        private static void ShowSteam()
        {
            _mainPanel?.SetActive(false);
            _steamPanel?.SetActive(true);
            _lanPanel?.SetActive(false);
        }

        private static void ShowLan()
        {
            _mainPanel?.SetActive(false);
            _steamPanel?.SetActive(false);
            _lanPanel?.SetActive(true);
        }

        private static void SetVisible(bool visible)
        {
            _root?.SetActive(visible);
        }

        // ── Status updates ───────────────────────────────────────────

        private static void UpdateStatus()
        {
            var nm = NetworkManager.Instance;
            string role = nm.IsHost ? "Host" : (nm.IsClient ? "Client" : "Idle");
            string conn = nm.IsConnected ? "Connected" : "Disconnected";

            if (_steamPanel != null && _steamPanel.activeSelf && _steamStatusText != null)
            {
                var lobby = SteamLobbyManager.Instance;
                string lobbyText = lobby.IsInLobby ? $"Lobby: {lobby.CurrentLobbyId}" : "No lobby";
                _steamStatusText.text = $"STEAM  |  {role}  |  {conn}\n{lobbyText}";
            }

            if (_lanPanel != null && _lanPanel.activeSelf && _lanStatusText != null)
            {
                _lanStatusText.text = $"LAN  |  {role}  |  {conn}";
            }
        }

        // ── Port helper ───────────────────────────────────────────────

        private static int GetLocalLanPort()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "--gt-lan-port" && int.TryParse(args[i + 1], out int p))
                    return p;
            return LanDefaultPort;
        }

        // ── Factory helpers ──────────────────────────────────────────

        private static Font _font;

        private static Font GetFont() => _font ??= Resources.GetBuiltinResource<Font>("Arial.ttf");

        private static GameObject CreatePanel(string name, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_root.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(25f, -25f);
            rt.sizeDelta = new Vector2(width, height);

            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
            go.SetActive(false);
            return go;
        }

        private static Text CreateLabel(
            GameObject parent,
            string text,
            float x,
            float y,
            float width,
            float height,
            int fontSize,
            TextAnchor anchor
        )
        {
            var go = new GameObject("GN_Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);

            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = GetFont();
            t.fontSize = fontSize;
            t.color = Color.white;
            t.alignment = anchor;
            return t;
        }

        private static Button CreateButton(
            GameObject parent,
            string label,
            float x,
            float y,
            float width,
            float height
        )
        {
            var go = new GameObject(
                "GN_Btn_" + label,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);

            go.GetComponent<Image>().color = new Color(0.22f, 0.22f, 0.22f, 1f);

            var btn = go.GetComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.38f, 0.38f, 0.38f);
            colors.pressedColor = new Color(0.10f, 0.10f, 0.10f);
            btn.colors = colors;

            CreateLabel(go, label, 0, 0, width, height, 13, TextAnchor.MiddleCenter);
            return btn;
        }

        private static InputField CreateInputField(
            GameObject parent,
            string defaultValue,
            string placeholder,
            float x,
            float y,
            float width,
            float height
        )
        {
            var go = new GameObject(
                "GN_Input_" + placeholder,
                typeof(RectTransform),
                typeof(Image)
            );
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(width, height);
            go.GetComponent<Image>().color = new Color(0.14f, 0.14f, 0.14f, 1f);

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8, 2);
            textRt.offsetMax = new Vector2(-8, -2);
            var inputText = textGo.GetComponent<Text>();
            inputText.font = GetFont();
            inputText.fontSize = 13;
            inputText.color = Color.white;

            var phGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
            phGo.transform.SetParent(go.transform, false);
            var phRt = phGo.GetComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(8, 2);
            phRt.offsetMax = new Vector2(-8, -2);
            var phText = phGo.GetComponent<Text>();
            phText.font = GetFont();
            phText.fontSize = 13;
            phText.color = new Color(0.5f, 0.5f, 0.5f);
            phText.fontStyle = FontStyle.Italic;
            phText.text = placeholder;

            var field = go.AddComponent<InputField>();
            field.textComponent = inputText;
            field.placeholder = phText;
            field.text = defaultValue;
            return field;
        }
    }
}
