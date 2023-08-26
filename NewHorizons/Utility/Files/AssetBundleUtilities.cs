using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility.Files
{
    public static class AssetBundleUtilities
    {
        public static Dictionary<string, (string starSystem, AssetBundle bundle)> AssetBundles = new();

        public static void ClearCache()
        {
            var bundleKeys = AssetBundles.Keys.ToArray();

            foreach (var key in bundleKeys)
            {
                var (starSystem, bundle) = AssetBundles[key];
                // If the star system is null/empty keep loaded forever, else only unload when we leave the system
                if (!string.IsNullOrEmpty(starSystem) && starSystem != Main.Instance.CurrentStarSystem)
                {
                    if (bundle == null) NHLogger.LogError($"The asset bundle for [{key}] was null when trying to unload");
                    else bundle.Unload(true);

                    AssetBundles.Remove(key);
                }
                else
                {
                    NHLogger.LogVerbose($"Not unloading bundle [{key}] because it is still in use for system [{starSystem}]");
                }
            }
        }

        // On the off chance this was being called from another mod
        public static T Load<T>(string assetBundleRelativeDir, string pathInBundle, IModBehaviour mod) where T : UnityEngine.Object
            => Load<T>(assetBundleRelativeDir, pathInBundle, mod, Main.Instance.CurrentStarSystem);

        public static T Load<T>(string assetBundleRelativeDir, string pathInBundle, IModBehaviour mod, string starSystem) where T : UnityEngine.Object
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

                    AssetBundles[key] = (starSystem, bundle);
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
            var prefab = Load<GameObject>(assetBundleRelativeDir, pathInBundle, mod, Main.Instance.CurrentStarSystem);

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
        }
    }
}
