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

        [HarmonyPrefix]
        [HarmonyPatch(nameof(RaftController.UpdateMoveToTarget))]
        public static bool UpdateMoveToTarget(RaftController __instance)
        {
            OWRigidbody raftBody = __instance._raftBody;
            OWRigidbody origParentBody = raftBody.GetOrigParentBody();
            Transform transform = origParentBody.transform;
            Vector3 position = transform.TransformPoint(__instance._targetLocalPosition);
            Vector3 distance = position - raftBody.GetPosition();
            float speed = Mathf.Min(__instance._targetSpeed, distance.magnitude / Time.deltaTime);
            Vector3 pointVelocity = raftBody.GetOrigParentBody().GetPointVelocity(raftBody.GetPosition());
            raftBody.SetVelocity(pointVelocity + (distance.normalized * speed));
            float t = (__instance.currentDistanceLerp = Mathf.InverseLerp(__instance._startDistance, 0.001f, distance.magnitude));
            var st = Mathf.SmoothStep(0f, 1f, t);
            // If it has a riverFluid volume then its a regular stranger one
            var isStranger = __instance._riverFluid != null;
            // Base game threshold has this at 1f (after doing smoothstep on it)
            // For whatever reason it never hits that for NH planets (probably since they're moving so much compared to the steady velocity of the Stranger)
            // Might break for somebody with a wacky spinning planet in which case we can adjust this or add some kind of fallback (i.e., wait x seconds and then just say its there)
            // Fixes #1005
            if (isStranger ? (st >= 1f) : (t > 0.999f))
            {
                raftBody.SetPosition(position);
                raftBody.SetRotation(transform.rotation * __instance._targetLocalRotation);
                __instance.StopMovingToTarget();
                __instance.OnArriveAtTarget.Invoke();
            }
            else
            {
                Quaternion quaternion = Quaternion.Slerp(__instance._startLocalRotation, __instance._targetLocalRotation, st);
                Quaternion toRotation = transform.rotation * quaternion;
                Vector3 vector4 = OWPhysics.FromToAngularVelocity(raftBody.GetRotation(), toRotation);
                raftBody.SetAngularVelocity(origParentBody.GetAngularVelocity() + vector4);
            }
            return false;
        }
    }
}
