using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Components.EOTE;

[HarmonyPatch]
public class NHSlideCollection : SlideCollection
{
    public string[] slidePaths;
    public IModBehaviour mod;
    private HashSet<string> _pathsBeingLoaded = new();
    /// <summary>
    /// map of slide path to collections that have this path loaded. used to only unload slide when nothing else is using it
    /// </summary>
    public static Dictionary<string, HashSet<NHSlideCollection>> _slidesRequiringPath = new();

    private static ShipLogSlideProjector _shipLogSlideProjector;

    static NHSlideCollection()
    {
        SceneManager.sceneUnloaded += (_) =>
        {
            foreach (var (slide, collections) in _slidesRequiringPath)
            {
                // If it has null, that means some other permanent thing loaded this texture and it will get cleared elsewhere
                // Otherwise it was loaded by an NHSlideCollection and should be deleted
                if (collections.Any() && !collections.Contains(null))
                {
                    ImageUtilities.DeleteTexture(slide);
                }
            }
            _slidesRequiringPath.Clear();
        };
    }

    public NHSlideCollection(int startArrSize, IModBehaviour mod, string[] slidePaths) : base(startArrSize)
    {
        this.mod = mod;
        this.slidePaths = slidePaths;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollection), nameof(SlideCollection.RequestStreamSlides))]
    public static bool SlideCollection_RequestStreamSlides(SlideCollection __instance, int[] slideIndices)
    {
        if (__instance is NHSlideCollection collection)
        {
            foreach (var id in slideIndices)
            {
                collection.LoadSlide(id);
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SlideCollection), nameof(SlideCollection.RequestRelease))]
    public static bool SlideCollection_RequestRelease(SlideCollection __instance, int[] slideIndices)
    {
        if (__instance is NHSlideCollection collection)
        {
            foreach (var id in slideIndices)
            {
                collection.UnloadSlide(id);
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
            __result = collection.IsSlideLoaded(streamIdx);
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
            __result = collection.LoadSlide(id);
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
            var wrappedIndex = (index + slides.Length) % slides.Length;
            var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.InvertedSlideReelCacheFolder, slidePaths[wrappedIndex]);

            // We are the first slide collection container to try and load this image
            var key = ImageUtilities.GetKey(mod, path);
            if (!_slidesRequiringPath.ContainsKey(key))
            {
                // Something else has loaded this image i.e., AutoProjector or Vision torch. We want to ensure we do not delete it
                if (ImageUtilities.IsTextureLoaded(mod, path))
                {
                    // null is dummy value to ensure its never empty (so its not deleted)
                    _slidesRequiringPath[key] = new() { null };
                }
                else
                {
                    _slidesRequiringPath[key] = new();
                }
                _slidesRequiringPath[key].Add(this);
            }

            if (ImageUtilities.IsTextureLoaded(mod, path))
            {
                // already loaded
                var texture = ImageUtilities.GetTexture(mod, path);
                slides[wrappedIndex]._image = texture;
                return texture;
            }
            else if (!_pathsBeingLoaded.Contains(path))
            {
                // not loaded yet, we need to load it
                var loader = new SlideReelAsyncImageLoader();
                loader.PathsToLoad.Add((wrappedIndex, path));
                loader.Start(true, false);
                loader.imageLoadedEvent.AddListener((Texture2D tex, int index, string originalPath) =>
                {
                    // weird: sometimes we set image, sometimes we return from GetStreamingTexture. oh well
                    slides[wrappedIndex]._image = tex;
                    _pathsBeingLoaded.Remove(path);
                    if (_shipLogSlideProjector == null)
                    {
                        _shipLogSlideProjector = GameObject.FindObjectOfType<ShipLogSlideProjector>();
                    }
                    if (_shipLogSlideProjector != null)
                    {
                        // gotta tell ship log we updated the image
                        _shipLogSlideProjector._slideDirty = true;
                    }
                    else
                    {
                        NHLogger.LogVerbose("No ship log slide reel projector exists");
                    }
                });
                _pathsBeingLoaded.Add(path);
                return null;
            }
            else
            {
                // It is being loaded so we just wait
                return null;
            }
        }
        var texture = LoadSlideInt(index);
        LoadSlideInt(index - 1);
        LoadSlideInt(index + 1);

        return texture;
    }

    public bool IsSlideLoaded(int index)
    {
        var wrappedIndex = (index + slides.Length) % slides.Length;
        var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.InvertedSlideReelCacheFolder, slidePaths[wrappedIndex]);
        return ImageUtilities.IsTextureLoaded(mod, path);
    }

    public void UnloadSlide(int index)
    {
        var wrappedIndex = (index + slides.Length) % slides.Length;
        var path = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, ProjectionBuilder.InvertedSlideReelCacheFolder, slidePaths[wrappedIndex]);

        // Only unload textures that we were the ones to load in
        if (ImageUtilities.IsTextureLoaded(mod, path))
        {
            var key = ImageUtilities.GetKey(mod, path);
            _slidesRequiringPath[key].Remove(this);
            if (!_slidesRequiringPath[key].Any())
            {
                NHLogger.LogVerbose($"Slide reel deleting {key} since nobody is using it anymore");
                ImageUtilities.DeleteTexture(mod, path, ImageUtilities.GetTexture(mod, path));
                slides[wrappedIndex]._image = null;
            }
        }
    }
}
