using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class HUDPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDMarker), nameof(HUDMarker.Awake))]
        public static void HUDMarker_Awake(HUDMarker __instance)
        {
            GlobalMessenger.AddListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
            GlobalMessenger.AddListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
            GlobalMessenger.AddListener("PlayerEnterCloakField", __instance.OnPlayerEnterCloakField);
            GlobalMessenger.AddListener("PlayerExitCloakField", __instance.OnPlayerExitCloakField);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDMarker), nameof(HUDMarker.OnDestroy))]
        public static void HUDMarker_OnDestroy(HUDMarker __instance)
        {
            GlobalMessenger.RemoveListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
            GlobalMessenger.RemoveListener("RefreshHUDVisibility", __instance.RefreshOwnVisibility);
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProbeHUDMarker), nameof(ProbeHUDMarker.RefreshOwnVisibility))]
        public static bool ProbeHUDMarker_RefreshOwnVisibility(ProbeHUDMarker __instance)
        {
            bool insideEYE = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
            bool insideQM = __instance._quantumMoon != null && (__instance._quantumMoon.IsPlayerInside() || __instance._quantumMoon.IsProbeInside());
            bool insideRW = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside == Locator.GetRingWorldController().isProbeInside;
            bool insideIP = Locator.GetCloakFieldController() != null && Locator.GetCloakFieldController().isPlayerInsideCloak == Locator.GetCloakFieldController().isProbeInsideCloak;
            bool insideCloak = Components.CloakSectorController.isPlayerInside == Components.CloakSectorController.isProbeInside;
            bool sameInterference = InterferenceHandler.IsPlayerSameAsProbe();
            bool isActive = __instance.gameObject.activeInHierarchy || __instance._isTLCDuplicate;

            __instance._isVisible = isActive && !insideEYE && !insideQM && !__instance._translatorEquipped && !__instance._inConversation && __instance._launched && (__instance._isWearingHelmet || __instance._atFlightConsole) && insideRW && insideIP && insideCloak && sameInterference;

            if (__instance._canvasMarker != null) __instance._canvasMarker.SetVisibility(__instance._isVisible);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipHUDMarker), nameof(ShipHUDMarker.RefreshOwnVisibility))]
        public static bool ShipHUDMarker_RefreshOwnVisibility(ShipHUDMarker __instance)
        {
            bool insideEYE = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
            bool insideQM = __instance._quantumMoon != null && (__instance._quantumMoon.IsPlayerInside() || __instance._quantumMoon.IsShipInside());
            bool insideRW = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside;
            bool insideIP = Locator.GetCloakFieldController() != null ? true : Locator.GetCloakFieldController().isPlayerInsideCloak == Locator.GetCloakFieldController().isShipInsideCloak;
            bool insideCloak = Components.CloakSectorController.isPlayerInside == Components.CloakSectorController.isShipInside;
            bool sameInterference = InterferenceHandler.IsPlayerSameAsShip();

            __instance._isVisible = !insideEYE && !insideQM && !insideRW && !__instance._translatorEquipped && !__instance._inConversation && !__instance._shipDestroyed && !__instance._playerInShip && PlayerState.HasPlayerEnteredShip() && __instance._isWearingHelmet && insideIP && insideCloak && sameInterference;

            if (__instance._canvasMarker != null) __instance._canvasMarker.SetVisibility(__instance._isVisible);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogEntryHUDMarker), nameof(ShipLogEntryHUDMarker.RefreshOwnVisibility))]
        public static bool ShipLogEntryHUDMarker_RefreshOwnVisibility(ShipLogEntryHUDMarker __instance)
        {
            bool hasEntryLocation = ShipLogEntryHUDMarker.s_entryLocation != null;
            bool insideEYE = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
            bool insideQM = __instance._quantumMoon != null && __instance._quantumMoon.IsPlayerInside();
            bool insideRW = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside && ShipLogEntryHUDMarker.s_entryLocationID == "IP_RING_WORLD";
            bool insideIP = (hasEntryLocation && ShipLogEntryHUDMarker.s_entryLocation.IsWithinCloakField()) || !(Locator.GetCloakFieldController() != null && Locator.GetCloakFieldController().isPlayerInsideCloak);
            bool insideCloak = (hasEntryLocation && ShipLogEntryHUDMarker.s_entryLocation.IsWithinCloakField()) || !Components.CloakSectorController.isPlayerInside;

            __instance._isVisible = (!insideEYE && !insideQM && !insideRW && !__instance._translatorEquipped && !__instance._inConversation && hasEntryLocation && (__instance._isWearingHelmet || __instance._atFlightConsole) && insideIP && insideCloak);

            if (__instance._canvasMarker != null) __instance._canvasMarker.SetVisibility(__instance._isVisible);

            return false;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProbeCamera), nameof(ProbeCamera.HasInterference))]
        public static void ProbeCamera_HasInterference(ProbeCamera __instance, ref bool __result)
        {
            __result = __result || (__instance._id != ProbeCamera.ID.PreLaunch && (Components.CloakSectorController.isPlayerInside != Components.CloakSectorController.isProbeInside || !InterferenceHandler.IsPlayerSameAsProbe()));
        }
    }
}
