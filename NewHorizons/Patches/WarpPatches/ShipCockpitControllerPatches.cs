using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.WarpPatches
{
    [HarmonyPatch(typeof(ShipCockpitController))]
    public static class ShipCockpitControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipCockpitController.Update))]
        public static bool ShipCockpitController_Update(ShipCockpitController __instance)
        {
            if (!Main.HasWarpDrive) return true;

            StarChartHandler.ShipLogStarChartMode.UpdateWarpPromptVisibility();
            if (__instance._playerAtFlightConsole && OWInput.IsNewlyPressed(InputLibrary.autopilot, InputMode.ShipCockpit))
            {
                var targetSystem = StarChartHandler.ShipLogStarChartMode.GetTargetStarSystem();
                if (targetSystem != null)
                {
                    Main.Instance.ChangeCurrentStarSystem(targetSystem, true);
                    return false;
                }
            }
            return true;
        }
    }
}
