using NewHorizons.Builder.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Tools
{
    public static class WarpDrivePatches
    {
        public static void Apply()
        {
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipCockpitController>("Update", typeof(WarpDrivePatches), nameof(WarpDrivePatches.OnShipCockpitControllerUpdate));

            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<ShipLogMapMode>("EnterMode", typeof(WarpDrivePatches), nameof(WarpDrivePatches.OnShipLogMapModeEnterMode));

            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ShipLogController>("Update", typeof(WarpDrivePatches), nameof(WarpDrivePatches.OnShipLogControllerUpdate));
        }

        public static void OnShipLogMapModeEnterMode(ShipLogMapMode __instance)
        {
            if (!Main.HasWarpDrive) return;

            var newPrompt = "Interstellar Mode";
            __instance._detectiveModePrompt.SetText(newPrompt);
            var text = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/ScreenPromptListScaleRoot/ScreenPromptList_UpperRight/ScreenPrompt/Text").GetComponent<UnityEngine.UI.Text>();
            text.text = newPrompt;
        }

        public static bool OnShipCockpitControllerUpdate(ShipCockpitController __instance)
        {
            if (!Main.HasWarpDrive) return true;

            if (__instance._playerAtFlightConsole && OWInput.IsNewlyPressed(InputLibrary.autopilot, InputMode.ShipCockpit))
            {
                var targetSystem = ShipLogBuilder.ShipLogStarChartMode.GetTargetStarSystem();
                if (targetSystem != null)
                {
                    Main.Instance.ChangeCurrentStarSystem(targetSystem, true);
                    return false;
                }
            }
            return true;
        }

        public static bool OnShipLogControllerUpdate(ShipLogController __instance)
        {
            if (!Main.HasWarpDrive) return true;

            if (__instance._exiting
                || OWInput.GetInputMode() != InputMode.ShipComputer
                || __instance._currentMode.AllowCancelInput() && OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.All)
                || ShipLogBuilder.ShipLogStarChartMode == null)
                return true;

            __instance._exitPrompt.SetVisibility(__instance._currentMode.AllowCancelInput());
            __instance._currentMode.UpdateMode();
            if (__instance._currentMode.AllowModeSwap() && OWInput.IsNewlyPressed(InputLibrary.swapShipLogMode, InputMode.All))
            {
                ShipLogMode currentMode = __instance._currentMode;
                string focusedEntryID = currentMode.GetFocusedEntryID();
                if (!focusedEntryID.Equals("")) return true;
                bool flag = currentMode.Equals(__instance._mapMode);
                __instance._currentMode = (flag ? __instance._detectiveMode : __instance._mapMode);

                if (currentMode.Equals(__instance._mapMode))
                    __instance._currentMode = ShipLogBuilder.ShipLogStarChartMode;
                else if (currentMode.Equals(ShipLogBuilder.ShipLogStarChartMode))
                    __instance._currentMode = __instance._detectiveMode;
                else
                    __instance._currentMode = __instance._mapMode;

                currentMode.ExitMode();
                __instance._currentMode.EnterMode(focusedEntryID, null);
                __instance._oneShotSource.PlayOneShot(flag ? global::AudioType.ShipLogEnterDetectiveMode : global::AudioType.ShipLogEnterMapMode, 1f);
            }
            return false;
        }
    }
}
