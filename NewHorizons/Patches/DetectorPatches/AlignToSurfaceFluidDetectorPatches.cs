using HarmonyLib;
using UnityEngine;

namespace NewHorizons.Patches.DetectorPatches
{
    [HarmonyPatch]
    public static class AlignToSurfaceFluidDetectorPatches
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(AsymmetricFluidDetector), nameof(AsymmetricFluidDetector.ManagedFixedUpdate))]
        public static void AsymmetricFluidDetector_ManagedFixedUpdate(AsymmetricFluidDetector __instance)
        {
            // This is like doing base.FixedUpdate
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AlignToSurfaceFluidDetector), nameof(AlignToSurfaceFluidDetector.ManagedFixedUpdate))]
        public static bool AlignToSurfaceFluidDetector_ManagedFixedUpdate(AlignToSurfaceFluidDetector __instance)
        {
            if (__instance._alignmentFluid is not RadialFluidVolume) return true;

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
