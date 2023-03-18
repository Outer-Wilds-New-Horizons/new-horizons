using HarmonyLib;

namespace NewHorizons.Patches.HUDPatches
{
    [HarmonyPatch(typeof(HUDMarker))]
    public static class HUDMarkerPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HUDMarker.Awake))]
        public static void HUDMarker_Awake(HUDMarker __instance)
        {
            GlobalMessenger.AddListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
            GlobalMessenger.AddListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
            GlobalMessenger.AddListener("PlayerEnterCloakField", __instance.OnPlayerEnterCloakField);
            GlobalMessenger.AddListener("PlayerExitCloakField", __instance.OnPlayerExitCloakField);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HUDMarker.OnDestroy))]
        public static void HUDMarker_OnDestroy(HUDMarker __instance)
        {
            GlobalMessenger.RemoveListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
            GlobalMessenger.RemoveListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
            GlobalMessenger.RemoveListener("PlayerEnterCloakField", __instance.OnPlayerEnterCloakField);
            GlobalMessenger.RemoveListener("PlayerExitCloakField", __instance.OnPlayerExitCloakField);
        }
    }
}
