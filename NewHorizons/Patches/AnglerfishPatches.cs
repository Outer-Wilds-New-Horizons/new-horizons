using HarmonyLib;
using UnityEngine;
using static AnglerfishController;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class AnglerfishPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AnglerfishAudioController), nameof(AnglerfishAudioController.OnChangeAnglerState))]
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

        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)] // So any other mod can set the controller before we check
        [HarmonyPatch(typeof(AnglerfishAnimController), nameof(AnglerfishAnimController.Awake))]
        public static bool AnglerfishAnimController_Awake(AnglerfishAnimController __instance)
        {
            if (__instance._anglerfishController == null)
            {
                __instance._animator = __instance.GetComponent<Animator>();
                __instance._lastStateChangeTime = 0f;
                __instance._moveCurrent = (__instance._moveTarget = (__instance._moveStart = 0f));
                __instance._jawCurrent = (__instance._jawTarget = (__instance._jawStart = 0.33f));
                __instance._spinesTarget = 0f;
                __instance._spinesCurrent = new float[__instance._spines.Length];
                __instance._spinesStart = new float[__instance._spines.Length];
                __instance._spineRotations = new Quaternion[__instance._spines.Length];
                __instance._spineOffsets = new float[__instance._spines.Length];
                for (int i = 0; i < __instance._spines.Length; i++)
                {
                    __instance._spinesCurrent[i] = (__instance._spinesStart[i] = 0f);
                    __instance._spineRotations[i] = __instance._spines[i].localRotation;
                    __instance._spineOffsets[i] = Random.value * __instance._spinesFlareVariation;
                }

                __instance.enabled = false;
                __instance.OnChangeAnglerState(AnglerfishController.AnglerState.Lurking);
                __instance._animator.SetFloat("MoveSpeed", __instance._moveCurrent);
                __instance._animator.SetFloat("Jaw", __instance._jawCurrent);

                return false;
            }
            return true;
        }
    }
}
