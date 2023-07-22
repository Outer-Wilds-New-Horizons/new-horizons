using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace NewHorizons.Patches;

[HarmonyPatch(typeof(GlobalMusicController))]
public class GlobalMusicControllerPatches
{
	private static AudioDetector _audioDetector;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(GlobalMusicController.UpdateBrambleMusic))]
	public static IEnumerable<CodeInstruction> GlobalMusicController_UpdateBrambleMusic(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        // This transpiler is to the check if dark bramble music should be playing #651
        // It essentially adds another boolean to a "should be playing" flag 
        return new CodeMatcher(instructions, generator)
        // All the other bools point to this so we make a label there
        .MatchForward(false,
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Stloc_0),
            new CodeMatch(OpCodes.Ldarg_0),
            new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GlobalMusicController), nameof(GlobalMusicController._darkBrambleSource))),
            new CodeMatch(OpCodes.Callvirt, AccessTools.Property(typeof(OWAudioSource), nameof(OWAudioSource.isPlaying)).GetGetMethod())
        )
        .CreateLabel(out Label label)
        // Find the first part of the boolean assignment
        .Start()
        .MatchForward(true,
            new CodeMatch(OpCodes.Call, typeof(Locator), nameof(Locator.GetPlayerSectorDetector)),
            new CodeMatch(OpCodes.Callvirt, typeof(PlayerSectorDetector), nameof(PlayerSectorDetector.InBrambleDimension)),
            new CodeMatch(OpCodes.Brfalse_S)
        )
        // Insert a new check to it pointing to the same label as the others
        .Insert(
            new CodeMatch(OpCodes.Call, typeof(GlobalMusicControllerPatches), nameof(GlobalMusicControllerPatches.IsPlayerInNoAudioVolumes)),
            new CodeMatch(OpCodes.Brfalse_S, label)
        )
        .InstructionEnumeration();
	}

    private static bool IsPlayerInNoAudioVolumes()
    {
        if (_audioDetector == null) _audioDetector = Object.FindObjectOfType<AudioDetector>();
        return _audioDetector._activeVolumes.Count == 0;
    }
}
