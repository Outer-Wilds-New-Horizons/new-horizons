using HarmonyLib;
using NewHorizons.Components;
using UnityEngine;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public class RaftPatches : HarmonyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RaftController), nameof(RaftController.FixedUpdate))]
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
            __instance._playerInEffectsRange = ((Locator.GetPlayerBody().GetPosition() - __instance._raftBody.GetPosition()).sqrMagnitude < 2500f);
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
                float num = __instance._fluidDetector.InFluidType(FluidVolume.Type.WATER) ? __instance._fluidDetector._alignmentFluid.GetFractionSubmerged(__instance._fluidDetector) : 0f;
                bool allowMovement = num > 0.25f && num < 1f;
                __instance._effectsController.UpdateMovementAudio(allowMovement, __instance._lightSensors);
                __instance._effectsController.UpdateGroundedAudio(__instance._fluidDetector);
            }
            __instance._localAcceleration = Vector3.zero;

            return false;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(AsymmetricFluidDetector), "ManagedFixedUpdate")]
        public static void AsymmetricFluidDetector_ManagedFixedUpdate(AsymmetricFluidDetector __instance)
        {
            // This is like doing base.FixedUpdate
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AlignToSurfaceFluidDetector), "ManagedFixedUpdate")]
        public static bool AlignToSurfaceFluidDetector_ManagedFixedUpdate(AlignToSurfaceFluidDetector __instance)
        {
            if (!(__instance._alignmentFluid is NHFluidVolume)) return true;

            // Mostly copy pasting from the AlignWithDirection class
            AsymmetricFluidDetector_ManagedFixedUpdate(__instance);

            if (__instance._inAlignmentFluid)
            {
                // Both in world space
                var currentDirection = __instance._owRigidbody.transform.up;
                var alignmentDirection = (__instance.transform.position - __instance._alignmentFluid.transform.position).normalized;
                var degreesToTarget = Vector3.Angle(currentDirection, alignmentDirection);

                var adjustedSlerpRate = Mathf.Clamp01(0.01f * degreesToTarget * Time.fixedDeltaTime);

                Vector3 a = OWPhysics.FromToAngularVelocity(currentDirection, alignmentDirection);
                __instance._owRigidbody.AddAngularVelocityChange(a * adjustedSlerpRate);
            }

            return false;
        }
    }
}
