using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Props.EchoesOfTheEye;
using NewHorizons.Utility.Files;
using OWML.Common;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components.EOTE;

[HarmonyPatch(typeof(SlideCollectionContainer))]
public class NHSlideCollectionContainer : SlideCollectionContainer
{
    public string[] conditionsToSet;
    public string[] persistentConditionsToSet;
    public string[] slidePaths;
    public IModBehaviour mod;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.Initialize))]
    public static bool SlideCollectionContainer_Initialize(SlideCollectionContainer __instance)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            if (__instance._initialized)
                return false;
            __instance.SetupReadFlags();
            __instance.RegisterPerSlideCompletion();
            if (__instance.streamingTexturesAvailable)
                __instance.SetupStreaming();
            __instance.BuildMusicRangesIndex();
            __instance._changeSlidesAllowed = true;
            __instance._initialized = true;
            __instance._slideCollection.isVision = __instance._owningItem == null;
            foreach (var factID in __instance._playWithShipLogFacts ?? Array.Empty<string>())
            {
                var fact = Locator.GetShipLogManager().GetFact(factID);
                fact?.RegisterSlideCollection(__instance._slideCollection);
            }
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.SetReadFlag))]
    public static void SlideCollectionContainer_SetReadFlag(SlideCollectionContainer __instance)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            if (container._unreadSlideIndices.Count == 0)
            {
                if (container.conditionsToSet != null)
                {
                    foreach (var condition in container.conditionsToSet)
                    {
                        DialogueConditionManager.SharedInstance.SetConditionState(condition, true);
                    }
                }
                if (container.persistentConditionsToSet != null)
                {
                    foreach (var condition in container.persistentConditionsToSet)
                    {
                        PlayerData.SetPersistentCondition(condition, true);
                    }
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.NextSlideAvailable))]
    public static bool SlideCollectionContainer_NextSlideAvailable(SlideCollectionContainer __instance, ref bool __result)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            __result = container.IsSlideLoaded(container.slideIndex + 1);
            return false;
        }
        else
        {
            return true;
        }
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.PrevSlideAvailable))]
    public static bool SlideCollectionContainer_PrevSlideAvailable(SlideCollectionContainer __instance, ref bool __result)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            __result = container.IsSlideLoaded(container.slideIndex - 1);
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.UnloadStreamingTextures))]
    public static bool SlideCollectionContainer_UnloadStreamingTextures(SlideCollectionContainer __instance)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            for (int i = 0; i < container.slidePaths.Length; i++)
            {
                container.UnloadSlide(i);
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.GetStreamingTexture))]
    public static bool SlideCollectionContainer_GetStreamingTexture(SlideCollectionContainer __instance, int id, ref Texture __result)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            __result = container.LoadSlide(id);
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.RequestManualStreamSlides))]
    public static bool SlideCollectionContainer_RequestManualStreamSlides(SlideCollectionContainer __instance)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            container.LoadSlide(__instance._currentSlideIndex);
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollectionContainer), nameof(SlideCollectionContainer.streamingTexturesAvailable), MethodType.Getter)]
    public static bool SlideCollectionContainer_streamingTexturesAvailable(SlideCollectionContainer __instance, ref bool __result)
    {
        if (__instance is NHSlideCollectionContainer container)
        {
            __result = container.slidePaths != null && container.slidePaths.Any();
            return false;
        }
        else
        {
            return true;
        }
    }

    public Texture LoadSlide(int index)
    {
        Texture LoadSlideInt(int index)
        {
            var wrappedIndex = (index + this.slideCollection.slides.Length) % this.slideCollection.slides.Length;
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.InvertedSlideReelCacheFolder, slidePaths[wrappedIndex]);

            var texture = ImageUtilities.GetTexture(mod, path);
            this.slideCollection.slides[wrappedIndex]._image = texture;
            return texture;
        }
        var texture = LoadSlideInt(index);
        LoadSlideInt(index - 1);
        LoadSlideInt(index + 1);

        return texture;
    }

    public bool IsSlideLoaded(int index)
    {
        var wrappedIndex = (index + this.slideCollection.slides.Length) % this.slideCollection.slides.Length;
        var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.InvertedSlideReelCacheFolder, slidePaths[wrappedIndex]);
        return ImageUtilities.IsTextureLoaded(mod, path);
    }

    public void UnloadSlide(int index)
    {
        var wrappedIndex = (index + this.slideCollection.slides.Length) % this.slideCollection.slides.Length;
        var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.InvertedSlideReelCacheFolder, slidePaths[wrappedIndex]);

        if (ImageUtilities.IsTextureLoaded(mod, path))
        {
            ImageUtilities.DeleteTexture(mod, path, ImageUtilities.GetTexture(mod, path));
            slideCollection.slides[wrappedIndex]._image = null;
        }
    }
}
