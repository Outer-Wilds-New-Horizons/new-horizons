using HarmonyLib;

namespace NewHorizons.Patches;

/// <summary>
/// Bug fix from the Outsider
/// </summary>
[HarmonyPatch]
internal class BrambleProjectionFixPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(FogWarpVolume), nameof(FogWarpVolume.WarpDetector))]
    public static bool FogWarpVolume_WarpDetector()
    {
        // Do not warp the player if they have entered the fog via a projection
        return !PlayerState.UsingNomaiRemoteCamera();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(FogWarpDetector), nameof(FogWarpDetector.FixedUpdate))]
    public static bool FogWarpDetector_FixedUpdate()
    {
        // Do not warp the player if they have entered the fog via a projection
        return !PlayerState.UsingNomaiRemoteCamera();
    }
}
