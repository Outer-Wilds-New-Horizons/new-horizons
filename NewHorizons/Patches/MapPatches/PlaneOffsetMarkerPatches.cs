using HarmonyLib;

namespace NewHorizons.Patches.MapPatches;

[HarmonyPatch(typeof(PlaneOffsetMarker))]
internal class PlaneOffsetMarkerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlaneOffsetMarker.Update))]
    public static bool PlaneOffsetMarker_Update(PlaneOffsetMarker __instance)
    {
        // It tracks the sun originally
        // Too lazy to perfectly time it getting the right static ref transform so we'll just do this
        // It disables itself if the ref frame is ever null so don't let it do that bc the ref frame is null when it starts
        __instance._sunTransform = Locator._centerOfTheUniverse._staticReferenceFrame.transform;
        return __instance._sunTransform != null;
    }
}
