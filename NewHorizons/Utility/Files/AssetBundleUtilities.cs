using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility.Files;

public static class AssetBundleUtilities
{
    public static Dictionary<string, (AssetBundle bundle, bool keepLoaded)> AssetBundles = new Dictionary<string, (AssetBundle, bool)>();

    public static void ClearCache()
    {
        foreach (var pair in AssetBundles)
        {
            if (!pair.Value.keepLoaded)
            {
                if (pair.Value.bundle == null)
                {
                    NHLogger.LogError($"The asset bundle for {pair.Key} was null when trying to unload");
                }
                else
                {
                    pair.Value.bundle.Unload(true);
                }
            }

        }
        AssetBundles = AssetBundles.Where(x => x.Value.keepLoaded).ToDictionary(x => x.Key, x => x.Value);
    }

    public static void PreloadBundle(string assetBundleRelativeDir, IModBehaviour mod)
    {
        string key = Path.GetFileName(assetBundleRelativeDir);
        var completePath = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, assetBundleRelativeDir);
        var request = AssetBundle.LoadFromFileAsync(completePath);
        request.completed += _ =>
        {
            NHLogger.Log($"Finished loading async bundle {assetBundleRelativeDir}");
            AssetBundles[key] = (request.assetBundle, true);
        };
    }

    public static T Load<T>(string assetBundleRelativeDir, string pathInBundle, IModBehaviour mod) where T : UnityEngine.Object
    {
        string key = Path.GetFileName(assetBundleRelativeDir);
        T obj;

        try
        {
            AssetBundle bundle;

            if (AssetBundles.ContainsKey(key))
            {
                bundle = AssetBundles[key].bundle;
            }
            else
            {
                var completePath = Path.Combine(mod.ModHelper.Manifest.ModFolderPath, assetBundleRelativeDir);
                bundle = AssetBundle.LoadFromFile(completePath);
                if (bundle == null)
                {
                    NHLogger.LogError($"Couldn't load AssetBundle at [{completePath}] for [{mod.ModHelper.Manifest.Name}]");
                    return null;
                }

                AssetBundles[key] = (bundle, false);
            }

            obj = bundle.LoadAsset<T>(pathInBundle);
        }
        catch (Exception e)
        {
            NHLogger.LogError($"Couldn't load asset {pathInBundle} from AssetBundle {assetBundleRelativeDir}:\n{e}");
            return null;
        }

        return obj;
    }

    public static GameObject LoadPrefab(string assetBundleRelativeDir, string pathInBundle, IModBehaviour mod)
    {
        var prefab = Load<GameObject>(assetBundleRelativeDir, pathInBundle, mod);

        prefab.SetActive(false);

        ReplaceShaders(prefab);

        return prefab;
    }

    public static void ReplaceShaders(GameObject prefab)
    {
        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;

                var replacementShader = Shader.Find(material.shader.name);
                if (replacementShader == null) continue;

                // preserve override tag and render queue (for Standard shader)
                // keywords and properties are already preserved
                if (material.renderQueue != material.shader.renderQueue)
                {
                    var renderType = material.GetTag("RenderType", false);
                    var renderQueue = material.renderQueue;
                    material.shader = replacementShader;
                    material.SetOverrideTag("RenderType", renderType);
                    material.renderQueue = renderQueue;
                }
                else
                {
                    material.shader = replacementShader;
                }
            }
        }

        foreach (var trenderer in prefab.GetComponentsInChildren<TessellatedRenderer>(true))
        {
            foreach (var material in trenderer.sharedMaterials)
            {
                if (material == null) continue;

                var replacementShader = Shader.Find(material.shader.name);
                if (replacementShader == null) continue;

                // preserve override tag and render queue (for Standard shader)
                // keywords and properties are already preserved
                if (material.renderQueue != material.shader.renderQueue)
                {
                    var renderType = material.GetTag("RenderType", false);
                    var renderQueue = material.renderQueue;
                    material.shader = replacementShader;
                    material.SetOverrideTag("RenderType", renderType);
                    material.renderQueue = renderQueue;
                }
                else
                {
                    material.shader = replacementShader;
                }
            }
        }
    }
}
