using HarmonyLib;
using NewHorizons.Components.Sectored;

namespace NewHorizons.Patches.HUDPatches
{
    [HarmonyPatch(typeof(ShipLogEntryHUDMarker))]
    public static class ShipLogEntryHUDMarkerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogEntryHUDMarker.RefreshOwnVisibility))]
        public static bool ShipLogEntryHUDMarker_RefreshOwnVisibility(ShipLogEntryHUDMarker __instance)
        {
            bool hasEntryLocation = ShipLogEntryHUDMarker.s_entryLocation != null;
            bool insideEYE = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
            bool insideQM = __instance._quantumMoon != null && __instance._quantumMoon.IsPlayerInside();
            bool insideRW = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside && ShipLogEntryHUDMarker.s_entryLocationID == "IP_RING_WORLD";
            bool insideIP = hasEntryLocation && ShipLogEntryHUDMarker.s_entryLocation.IsWithinCloakField() || !(Locator.GetCloakFieldController() != null && Locator.GetCloakFieldController().isPlayerInsideCloak);
            bool insideCloak = hasEntryLocation && ShipLogEntryHUDMarker.s_entryLocation.IsWithinCloakField() || !CloakSectorController.isPlayerInside;

            __instance._isVisible = !insideEYE && !insideQM && !insideRW && !__instance._translatorEquipped && !__instance._inConversation && hasEntryLocation && (__instance._isWearingHelmet || __instance._atFlightConsole) && insideIP && insideCloak;

            if (__instance._canvasMarker != null) __instance._canvasMarker.SetVisibility(__instance._isVisible);

            return false;
        }
    }
}
