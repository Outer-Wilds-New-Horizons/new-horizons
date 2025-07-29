
using HarmonyLib;
using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NewHorizons.Patches.WarpPatches
{
    [HarmonyPatch(typeof(ShipCockpitController))]
    public static class ShipCockpitControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipCockpitController.Update))]
        public static bool ShipCockpitController_Update(ShipCockpitController __instance)
        {
            if (!Main.HasWarpDriveFunctionality) return true;

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

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ShipCockpitController.FixedUpdate))]
        public static IEnumerable<CodeInstruction> ShipCockpitController_FixedUpdate(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Instead of targetting the Sun target the center of the universe
            return new CodeMatcher(instructions, generator)
            // Have to create a label that goes to the method return for the if statement logic
            .MatchForward(false,
                new CodeMatch(OpCodes.Ret)
            )
            .CreateLabel(out Label returnLabel)
            .Start()
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Locator), nameof(Locator.GetAstroObject), new Type[] {typeof(AstroObject.Name)})),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(AstroObject), nameof(AstroObject.GetOWRigidbody)))
            )
            .SetOpcodeAndAdvance(OpCodes.Nop) // Have to set to Nop since the Ldc_I4_2 operation is a label
            .RemoveInstructions(2)
            .Insert(
                // First do an if statement to see if the warp drive is locked on
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShipCockpitControllerPatches), nameof(ShipCockpitControllerPatches.ShouldReturn))),
                new CodeInstruction(OpCodes.Brfalse_S, returnLabel),

                // Then get the center of the universe and its reference frame
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Locator), nameof(Locator.GetCenterOfTheUniverse))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(CenterOfTheUniverse), nameof(CenterOfTheUniverse.GetStaticReferenceFrame)))
            )
            .InstructionEnumeration();
        }

        private static bool ShouldReturn()
        {
            return !StarChartHandler.IsWarpDriveLockedOn() && Main.GetCurrentSystemConfig.returnToSolarSystemWhenTooFar;
        }
    }
}
