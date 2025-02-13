using HarmonyLib;
using NewHorizons.Utility.Files;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Components.EOTE;

[HarmonyPatch]
public class NHSlideCollection : SlideCollection
{
    
    public string[] slidePaths;
    public IModBehaviour mod;

    public NHSlideCollection(int startArrSize, IModBehaviour mod, string[] slidePaths) : base(startArrSize)
    {
        this.mod = mod;
        this.slidePaths = slidePaths;
    }

    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollection), nameof(SlideCollection.RequestStreamSlides))]
    public static bool SlideCollection_RequestStreamSlides(SlideCollection __instance, int[] slideIndices)
    {
        if (__instance is NHSlideCollection collection)
        {
            foreach (var id in slideIndices)
            {
                collection.slides[id]._image = ImageUtilities.GetTexture(collection.mod, collection.slidePaths[id]);
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollection), nameof(SlideCollection.IsStreamedTextureIndexLoaded))]
    public static bool SlideCollection_IsStreamedTextureIndexLoaded(SlideCollection __instance, int streamIdx, ref bool __result)
    {
        if (__instance is NHSlideCollection collection)
        {
            __result = ImageUtilities.IsTextureLoaded(collection.mod, collection.slidePaths[streamIdx]);
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollection), nameof(SlideCollection.GetStreamingTexture))]
    public static bool SlideCollection_GetStreamingTexture(SlideCollection __instance, int id, ref Texture __result)
    {
        if (__instance is NHSlideCollection collection)
        {
            __result = ImageUtilities.GetTexture(collection.mod, collection.slidePaths[id]);
            return false;
        }
        else
        {
            return true;
        }
    }
    */
}
