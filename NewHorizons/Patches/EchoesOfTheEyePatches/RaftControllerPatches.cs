using HarmonyLib;
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
    }
}
