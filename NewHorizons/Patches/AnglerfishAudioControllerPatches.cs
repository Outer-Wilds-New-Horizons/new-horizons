using HarmonyLib;
using UnityEngine;
using static AnglerfishController;

namespace NewHorizons.Patches
{
    [HarmonyPatch(typeof(AnglerfishAudioController))]
    public static class AnglerfishAudioControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AnglerfishAudioController.OnChangeAnglerState))]
        public static bool AnglerfishAudioController_OnChangeAnglerState(AnglerfishAudioController __instance, AnglerState anglerState)
        {
            var scale = __instance.transform.parent.localScale.x;

            if (scale == 1)
            {
                return true;
            }
            else
            {
                var modifier = 1f / Mathf.Clamp(scale, 0.5f, 2f);

                __instance.UpdateLoopingAudio(anglerState);
                if (anglerState == AnglerState.Investigating)
                {
                    __instance._longRangeOneShotSource.pitch = modifier * Random.Range(0.8f, 1f);
                    __instance._longRangeOneShotSource.PlayOneShot(AudioType.DBAnglerfishDetectDisturbance, 1f);
                    return false;
                }
                if (anglerState == AnglerState.Chasing)
                {
                    if (Time.time > AnglerfishAudioController.s_lastDetectTime + 2f)
                    {
                        AnglerfishAudioController.s_lastDetectTime = Time.time;
                        __instance._oneShotSource.pitch = modifier * Random.Range(0.8f, 1f);
                        __instance._oneShotSource.PlayOneShot(AudioType.DBAnglerfishDetectTarget, 1f);
                        return false;
                    }
                    MonoBehaviour.print("ANGLER DETECT TARGET SOUND BLOCKED");
                }

                return false;
            }
        }
    }
}
