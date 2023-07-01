using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(DreamWorldController))]
    public static class DreamworldControllerPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DreamWorldController.FixedUpdate))]
        [HarmonyPatch(nameof(DreamWorldController.SpawnInDreamWorld))]
        public static IEnumerable<CodeInstruction> DreamWorldController_SpawnInDreamworld(IEnumerable<CodeInstruction> instructions)
        {
            // Thank you vio very cool!
            // For some reason in Patch 13 they made it so the planetary fog controller is disabled in the Dreamworld
            // This broke Hazy Dreams
            return new CodeMatcher(instructions).MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(DreamWorldController), nameof(DreamWorldController._playerCamera))),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Property(typeof(OWCamera), nameof(OWCamera.planetaryFog)).GetGetMethod()),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Property(typeof(Behaviour), nameof(Behaviour.enabled)).GetSetMethod())
                )
                .Repeat(matcher => matcher.RemoveInstructions(5))
                .InstructionEnumeration();
        }
    }
}
