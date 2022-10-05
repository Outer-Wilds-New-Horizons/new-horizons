using HarmonyLib;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using UnityEngine;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class WarpDrivePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.Update))]
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
