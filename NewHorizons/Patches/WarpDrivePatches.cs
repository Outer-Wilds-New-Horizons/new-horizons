using HarmonyLib;
using NewHorizons.Handlers;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class WarpDrivePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogMapMode), nameof(ShipLogMapMode.EnterMode))]
        public static void ShipLogMapMode_EnterMode(ShipLogMapMode __instance)
        {
            if (!Main.HasWarpDrive) return;

            var newPrompt = TranslationHandler.GetTranslation("INTERSTELLAR_MODE", TranslationHandler.TextType.UI);
            __instance._detectiveModePrompt.SetText(newPrompt);
            var text = GameObject
                .Find(
                    "Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/ScreenPromptListScaleRoot/ScreenPromptList_UpperRight/ScreenPrompt/Text")
                .GetComponent<UnityEngine.UI.Text>();
            text.text = newPrompt;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.Update))]
        public static bool ShipCockpitController_Update(ShipCockpitController __instance)
        {
            if (!Main.HasWarpDrive) return true;

            if (__instance._playerAtFlightConsole &&
                OWInput.IsNewlyPressed(InputLibrary.autopilot, InputMode.ShipCockpit))
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogController), nameof(ShipLogController.Update))]
        public static bool ShipLogController_Update(ShipLogController __instance)
        {
            if (!Main.HasWarpDrive) return true;

            if (__instance._exiting
                || OWInput.GetInputMode() != InputMode.ShipComputer
                || (__instance._currentMode.AllowCancelInput() && OWInput.IsNewlyPressed(InputLibrary.cancel))
                || StarChartHandler.ShipLogStarChartMode == null)
                return true;

            // Mostly copied from the base method but we're trying to fit in our new mode
            __instance._exitPrompt.SetVisibility(__instance._currentMode.AllowCancelInput());
            __instance._currentMode.UpdateMode();
            if (__instance._currentMode.AllowModeSwap() && OWInput.IsNewlyPressed(InputLibrary.swapShipLogMode))
            {
                var currentMode = __instance._currentMode;
                var focusedEntryID = currentMode.GetFocusedEntryID();
                if (!focusedEntryID.Equals("")) return true;
                var flag = currentMode.Equals(__instance._mapMode);
                __instance._currentMode = flag ? __instance._detectiveMode : __instance._mapMode;

                if (currentMode.Equals(__instance._mapMode))
                    __instance._currentMode = StarChartHandler.ShipLogStarChartMode;
                else if (currentMode.Equals(StarChartHandler.ShipLogStarChartMode))
                    __instance._currentMode = __instance._detectiveMode;
                else
                    __instance._currentMode = __instance._mapMode;

                currentMode.ExitMode();
                __instance._currentMode.EnterMode(focusedEntryID);
                __instance._oneShotSource.PlayOneShot(flag
                    ? AudioType.ShipLogEnterDetectiveMode
                    : AudioType.ShipLogEnterMapMode);
            }

            return false;
        }
    }
}