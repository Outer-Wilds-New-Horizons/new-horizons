using HarmonyLib;
using NewHorizons.Components.Props;
using UnityEngine;

namespace NewHorizons.Patches.EchoesOfTheEyePatches
{
    [HarmonyPatch(typeof(RaftController))]
    public static class RaftControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(RaftController.FixedUpdate))]
        public static bool RaftController_FixedUpdate(RaftController __instance)
        {
            // If it has a river fluid its a normal one and we don't do anything
            if (__instance._riverFluid != null) return true;

            // Copy paste the original method
            if (__instance._raftBody.IsSuspended())
            {
                return false;
            }
            bool playerInEffectsRange = __instance._playerInEffectsRange;
            __instance._playerInEffectsRange = (Locator.GetPlayerBody().GetPosition() - __instance._raftBody.GetPosition()).sqrMagnitude < 2500f;
            if (playerInEffectsRange && !__instance._playerInEffectsRange)
            {
                __instance._effectsController.StopAllEffects();
            }
            if (__instance._dock != null || __instance._movingToTarget)
            {
                __instance._localAcceleration = Vector3.zero;
                if (__instance._playerInEffectsRange)
                {
                    __instance._effectsController.UpdateMovementAudio(false, __instance._lightSensors);
                }
                if (__instance._movingToTarget)
                {
                    __instance.UpdateMoveToTarget();
                }
                return false;
            }
            if (__instance._fluidDetector.InFluidType(FluidVolume.Type.WATER))
            {
                if (__instance._lightSensors[0].IsIlluminated())
                {
                    __instance._localAcceleration += Vector3.forward * __instance._acceleration;
                }
                if (__instance._lightSensors[1].IsIlluminated())
                {
                    __instance._localAcceleration += Vector3.right * __instance._acceleration;
                }
                if (__instance._lightSensors[2].IsIlluminated())
                {
                    __instance._localAcceleration -= Vector3.forward * __instance._acceleration;
                }
                if (__instance._lightSensors[3].IsIlluminated())
                {
                    __instance._localAcceleration -= Vector3.right * __instance._acceleration;
                }
            }
            if (__instance._localAcceleration.sqrMagnitude > 0.001f)
            {
                __instance._raftBody.AddLocalAcceleration(__instance._localAcceleration);
            }
            if (__instance._playerInEffectsRange)
            {
                // All this to change what fluidVolume we use on this line
                FluidVolume volume = __instance._fluidDetector._alignmentFluid;
                float num = __instance._fluidDetector.InFluidType(FluidVolume.Type.WATER) && volume != null ? volume.GetFractionSubmerged(__instance._fluidDetector) : 0f;
                bool allowMovement = num > 0.25f && num < 1f;
                __instance._effectsController.UpdateMovementAudio(allowMovement, __instance._lightSensors);
                __instance._effectsController.UpdateGroundedAudio(__instance._fluidDetector);
            }
            __instance._localAcceleration = Vector3.zero;

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(RaftController.UpdateMoveToTarget))]
        public static void UpdateMoveToTarget(RaftController __instance)
        {
            // If it has a riverFluid volume then its a regular stranger one
            if (__instance._movingToTarget && __instance._riverFluid == null)
            {
                OWRigidbody raftBody = __instance._raftBody;
                OWRigidbody origParentBody = __instance._raftBody.GetOrigParentBody();
                Transform transform = origParentBody.transform;
                Vector3 vector = transform.TransformPoint(__instance._targetLocalPosition);

                // Base game threshold has this at 1f (after doing smoothstep on it)
                // For whatever reason it never hits that for NH planets (probably since they're moving so much compared to the steady velocity of the Stranger)
                // Might break for somebody with a wacky spinning planet in which case we can adjust this or add some kind of fallback (i.e., wait x seconds and then just say its there)
                // Fixes #1005
                if (__instance.currentDistanceLerp > 0.999f)
                {
                    raftBody.SetPosition(vector);
                    raftBody.SetRotation(transform.rotation * __instance._targetLocalRotation);
                    __instance.StopMovingToTarget();
                    __instance.OnArriveAtTarget.Invoke();
                }
            }
        }
    }
}
