using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches;

[HarmonyPatch(typeof(GlobalMusicController))]
public class GlobalMusicControllerPatches
{
	private static AudioDetector _audioDetector;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(GlobalMusicController.UpdateBrambleMusic))]
	public static bool GlobalMusicController_UpdateBrambleMusic(GlobalMusicController __instance)
	{
		// is this too hacky?
		if (_audioDetector == null) _audioDetector = Object.FindObjectOfType<AudioDetector>();


		var shouldBePlaying = Locator.GetPlayerSectorDetector().InBrambleDimension() &&
			!Locator.GetPlayerSectorDetector().InVesselDimension() &&
			PlayerState.AtFlightConsole() &&
			!PlayerState.IsHullBreached() &&
			!__instance._playingFinalEndTimes &&
			_audioDetector._activeVolumes.Count <= 1; // change - don't play if in another audio volume other than ambient
		var playing = __instance._darkBrambleSource.isPlaying &&
			!__instance._darkBrambleSource.IsFadingOut();
		if (shouldBePlaying && !playing)
		{
			__instance._darkBrambleSource.FadeIn(5f);
		}
		else if (!shouldBePlaying && playing)
		{
			__instance._darkBrambleSource.FadeOut(5f);
		}

		return false;
	}
}
