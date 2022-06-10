using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class HUDPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDMarker), nameof(HUDMarker.Awake))]
        public static void HUDMarker_Awake(HUDMarker __instance)
        {
            GlobalMessenger.AddListener("PlayerEnterCloakField", __instance.OnPlayerEnterCloakField);
            GlobalMessenger.AddListener("PlayerExitCloakField", __instance.OnPlayerExitCloakField);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDMarker), nameof(HUDMarker.OnDestroy))]
        public static void HUDMarker_OnDestroy(HUDMarker __instance)
        {
            GlobalMessenger.RemoveListener("PlayerEnterCloakField", __instance.OnPlayerEnterCloakField);
            GlobalMessenger.RemoveListener("PlayerExitCloakField", __instance.OnPlayerExitCloakField);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProbeHUDMarker), nameof(ProbeHUDMarker.Awake))]
        public static void ProbeHUDMarker_Awake(ProbeHUDMarker __instance)
        {
            GlobalMessenger.AddListener("ProbeEnterCloakField", __instance.RefreshOwnVisibility);
            GlobalMessenger.AddListener("ProbeExitCloakField", __instance.RefreshOwnVisibility);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProbeHUDMarker), nameof(ProbeHUDMarker.OnDestroy))]
        public static void ProbeHUDMarker_OnDestroy(ProbeHUDMarker __instance)
        {
            GlobalMessenger.RemoveListener("ProbeEnterCloakField", __instance.RefreshOwnVisibility);
            GlobalMessenger.RemoveListener("ProbeExitCloakField", __instance.RefreshOwnVisibility);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipHUDMarker), nameof(ShipHUDMarker.Awake))]
        public static void ShipHUDMarker_Awake(ShipHUDMarker __instance)
        {
            GlobalMessenger.AddListener("ShipEnterCloakField", __instance.RefreshOwnVisibility);
            GlobalMessenger.AddListener("ShipExitCloakField", __instance.RefreshOwnVisibility);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipHUDMarker), nameof(ShipHUDMarker.OnDestroy))]
        public static void ShipHUDMarker_OnDestroy(ShipHUDMarker __instance)
        {
            GlobalMessenger.RemoveListener("ShipEnterCloakField", __instance.RefreshOwnVisibility);
            GlobalMessenger.RemoveListener("ShipExitCloakField", __instance.RefreshOwnVisibility);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.HasInterference))]
        public static void ProbeCamera_HasInterference(ProbeCamera __instance, ref bool __result)
        {
            __result = __result || Components.CloakSectorController.isPlayerInside != Components.CloakSectorController.isProbeInside;
        }
    }
}
