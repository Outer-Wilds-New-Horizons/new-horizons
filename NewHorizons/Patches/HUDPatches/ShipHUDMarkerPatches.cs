using HarmonyLib;
using NewHorizons.Components.Sectored;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.HUDPatches
{
    [HarmonyPatch(typeof(ShipHUDMarker))]
    public static class ShipHUDMarkerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipHUDMarker.RefreshOwnVisibility))]
        public static bool ShipHUDMarker_RefreshOwnVisibility(ShipHUDMarker __instance)
        {
            bool insideEYE = Locator.GetEyeStateManager() != null && Locator.GetEyeStateManager().IsInsideTheEye();
            bool insideQM = __instance._quantumMoon != null && (__instance._quantumMoon.IsPlayerInside() || __instance._quantumMoon.IsShipInside());
            bool insideRW = Locator.GetRingWorldController() != null && Locator.GetRingWorldController().isPlayerInside;
            bool insideIPMatches = Locator.GetCloakFieldController() == null || Locator.GetCloakFieldController().isPlayerInsideCloak == Locator.GetCloakFieldController().isShipInsideCloak;
            bool insideCloakMatches = CloakSectorController.isPlayerInside == CloakSectorController.isShipInside;
            bool sameInterference = InterferenceHandler.IsPlayerSameAsShip();

            __instance._isVisible = !insideEYE && !insideQM && !insideRW && !__instance._translatorEquipped && !__instance._inConversation && !__instance._shipDestroyed && !__instance._playerInShip && PlayerState.HasPlayerEnteredShip() && __instance._isWearingHelmet && insideIPMatches && insideCloakMatches && sameInterference;

            if (__instance._canvasMarker != null) __instance._canvasMarker.SetVisibility(__instance._isVisible);

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ShipHUDMarker.OnDestroy))]
        public static void ShipHUDMarker_OnDestroy(ShipHUDMarker __instance)
        {
            GlobalMessenger.RemoveListener("ShipEnterCloakField", __instance.RefreshOwnVisibility);
            GlobalMessenger.RemoveListener("ShipExitCloakField", __instance.RefreshOwnVisibility);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ShipHUDMarker.Awake))]
        public static void ShipHUDMarker_Awake(ShipHUDMarker __instance)
        {
            GlobalMessenger.AddListener("ShipEnterCloakField", __instance.RefreshOwnVisibility);
            GlobalMessenger.AddListener("ShipExitCloakField", __instance.RefreshOwnVisibility);
        }
    }
}
