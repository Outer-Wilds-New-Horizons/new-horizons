using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components.EOTE;

[HarmonyPatch(typeof(SlideCollectionContainer))]
public class NHSlideCollectionContainer : SlideCollectionContainer
{
    public string[] conditionsToSet;
    public string[] persistentConditionsToSet;
    public bool doAsyncLoading = true;

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
            foreach (var factID in __instance._playWithShipLogFacts)
            {
                var fact = Locator.GetShipLogManager().GetFact(factID);
                fact?.RegisterSlideCollection(__instance._slideCollection);
                // in original it logs. we dont want that here ig
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
        if (__instance is NHSlideCollectionContainer container && container.doAsyncLoading)
        {
            __result = ((NHSlideCollection)container.slideCollection).IsSlideLoaded(container.slideIndex + 1);
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
        if (__instance is NHSlideCollectionContainer container && container.doAsyncLoading)
        {
            __result = ((NHSlideCollection)container.slideCollection).IsSlideLoaded(container.slideIndex - 1);
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
        if (__instance is NHSlideCollectionContainer container && container.doAsyncLoading)
        {
            for (int i = 0; i < ((NHSlideCollection)container.slideCollection).slidePaths.Length; i++)
            {
                ((NHSlideCollection)container.slideCollection).UnloadSlide(i);
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
        if (__instance is NHSlideCollectionContainer container && container.doAsyncLoading)
        {
            __result = ((NHSlideCollection)container.slideCollection).LoadSlide(id);
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
        if (__instance is NHSlideCollectionContainer container && container.doAsyncLoading)
        {
            ((NHSlideCollection)container.slideCollection).LoadSlide(__instance._currentSlideIndex);
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
        if (__instance is NHSlideCollectionContainer container && container.doAsyncLoading)
        {
            __result = ((NHSlideCollection)container.slideCollection).slidePaths != null && ((NHSlideCollection)container.slideCollection).slidePaths.Any();
            return false;
        }
        else
        {
            return true;
        }
    }
}
