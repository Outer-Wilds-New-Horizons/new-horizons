using HarmonyLib;
using NewHorizons.Components.EOTE;

namespace NewHorizons.Patches.ShipLogPatches;

[HarmonyPatch]
public static class ShipLogSlideReelPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipLogSlideProjector), nameof(ShipLogSlideProjector.CheckStreamingTexturesAvailable))]
    public static bool ShipLogSlideProjector_CheckStreamingTexturesAvailable(ShipLogSlideProjector __instance, ref bool __result)
    {
        if (__instance._collectionIndex >= 0 && __instance._collectionIndex < __instance._slideCollections.Count &&
            __instance._slideCollections[__instance._collectionIndex] is NHSlideCollection)
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ShipLogSlideProjector), nameof(ShipLogSlideProjector.UnloadCurrentStreamingTextures))]
    public static bool ShipLogSlideProjector_UnloadCurrentStreamingTextures(ShipLogSlideProjector __instance)
    {
        if (__instance._collectionIndex >= 0 && __instance._collectionIndex < __instance._slideCollections.Count &&
            __instance._slideCollections[__instance._collectionIndex] is NHSlideCollection collection)
        {
            for (int i = 0; i < collection.slides.Length; i++)
            {
                collection.UnloadSlide(i);
            }
            return false;
        }
        return true;
    }
}
