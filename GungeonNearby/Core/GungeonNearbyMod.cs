using BepInEx;
using GungeonNearby.Networking;
using GungeonNearby.Networking.Proxies;
using GungeonNearby.UI;
using HarmonyLib;
using UnityEngine;

namespace GungeonNearby.Core
{
    [BepInPlugin("com.ismasel.GungeonNearby", "Gungeon Nearby", "1.0.0")]
    [BepInDependency("etgmodding.etg.mtgapi")]
    public class GungeonNearbyMod : BaseUnityPlugin
    {
        public static GungeonNearbyMod Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo("Gungeon Nearby started!");

            try
            {
                // Initialise Logging
                Systems.Logging.Logger.Initialise(Logger);
                Logger.LogInfo("Logging initialized.");

                new Harmony("com.ismasel.GungeonNearby").PatchAll();
                InitProxies();

                // Initialise Networking
                Logger.LogInfo("Initializing NetworkManager...");
                NetworkManager.Instance.Initialise();
                Logger.LogInfo("NetworkManager initialized.");

                // Initialise UI
                Logger.LogInfo("Initializing UIManager...");
                UIManager.Initialise();
                Logger.LogInfo("UIManager initialized.");

                // Initialise in-game log console overlay (F10 to toggle)
                var logConsoleGo = new GameObject("GungeonNearby_LogConsole");
                DontDestroyOnLoad(logConsoleGo);
                logConsoleGo.AddComponent<LogConsoleOverlay>();

                Logger.LogInfo("Gungeon Nearby fully initialized!");
            }
            catch (System.Exception ex)
            {
                Logger.LogError(
                    $"Exception during initialization: {ex.GetType().Name}: {ex.Message}"
                );
                Logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void Update()
        {
            try
            {
                NetworkManager.Instance.Update();
                UIManager.Update();
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error in Update: {ex.Message}");
            }
        }

        /// <summary>
        /// Statically Initialize Relevant Proxies
        /// </summary>
        private void InitProxies()
        {
            PlayerProxy.Initialize();
        }
    }
}
