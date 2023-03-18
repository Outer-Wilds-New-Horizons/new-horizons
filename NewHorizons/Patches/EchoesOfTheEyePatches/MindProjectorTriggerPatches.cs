using HarmonyLib;
using NewHorizons.Builder.Props;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(MindProjectorTrigger))]
    public static class MindProjectorTriggerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(MindProjectorTrigger.OnTriggerVolumeEntry))]
        public static bool MindProjectorTrigger_OnTriggerVolumeEntry(MindProjectorTrigger __instance, GameObject hitObj)
        {
            var t = hitObj.GetComponent<VisionTorchTarget>();
            if (t != null) //(hitObj.CompareTag("PrisonerDetector"))
            {
                __instance._mindProjector.OnProjectionStart += t.onSlidesStart;
                __instance._mindProjector.OnProjectionComplete += t.onSlidesComplete;
                __instance._mindProjector.SetMindSlideCollection(t.slideCollection);

                __instance.OnBeamStartHitPrisoner.Invoke();
                __instance._mindProjector.Play(reset: true);
                __instance._mindProjector.OnProjectionStart += __instance.OnProjectionStart;
                __instance._mindProjector.OnProjectionComplete += __instance.OnProjectionComplete;

                Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>().LockOn(hitObj.transform, Vector3.zero);
                __instance._playerLockedOn = true;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MindProjectorTrigger.OnTriggerVolumeExit))]
        private static bool MindProjectorTrigger_OnTriggerVolumeExit(MindProjectorTrigger __instance, GameObject hitObj)
        {
            var t = hitObj.GetComponent<VisionTorchTarget>();
            if (t != null) //(hitObj.CompareTag("PrisonerDetector"))
            {
                __instance._mindProjector.OnProjectionStart -= t.onSlidesStart;
                __instance._mindProjector.OnProjectionComplete -= t.onSlidesComplete;
            }
            return true;
        }
    }
}
