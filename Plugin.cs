using BepInEx;
using Nothing.Notifications;
using Nothing.Template;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace Nothing
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
        public static bool isInitialized = false;
        private static GameObject uiObject;

        public static void OnPlayerSpawned()
        {
            if (isInitialized) return;

            Nothing.Template.Buttons.Init();
            Patches.PatchHandler.PatchAll();
			Main.CreateMenu();

			Debug.Log(PluginInfo.Name + "loaded");

			NotifiLib.SendNotification("Test notification");

            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized && GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
            {
                OnPlayerSpawned();
            }
        }
    }
}