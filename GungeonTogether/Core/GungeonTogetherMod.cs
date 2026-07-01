using BepInEx;
using GungeonTogether.Networking;
using GungeonTogether.Networking.Proxies;
using GungeonTogether.UI;
using HarmonyLib;

namespace GungeonTogether.Core
{
    [BepInPlugin("com.ismasel.gungeontogether", "Gungeon Together", "1.0.0")]
    [BepInDependency("etgmodding.etg.mtgapi")]
    public class GungeonTogetherMod : BaseUnityPlugin
    {
        public static GungeonTogetherMod Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo("Gungeon Together started!");

            try
            {
                // Initialise Logging
                Systems.Logging.Logger.Initialise(base.Logger);
                Logger.LogInfo("Logging initialized.");

                new Harmony("com.ismasel.gungeontogether").PatchAll();
                InitProxies();

                // Initialise Networking
                Logger.LogInfo("Initializing NetworkManager...");
                NetworkManager.Instance.Initialise();
                Logger.LogInfo("NetworkManager initialized.");

                // Initialise UI
                Logger.LogInfo("Initializing UIManager...");
                UIManager.Initialise();
                Logger.LogInfo("UIManager initialized.");

                Logger.LogInfo("Gungeon Together fully initialized!");
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
        private void InitProxies() { }
    }
}
