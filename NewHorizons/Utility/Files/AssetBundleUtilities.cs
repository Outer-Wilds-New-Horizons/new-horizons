using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility.Files
{
    public static class AssetBundleUtilities
    {
        public static Dictionary<string, (AssetBundle bundle, bool keepLoaded)> AssetBundles = new();

        private static readonly List<AssetBundleCreateRequest> _loadingBundles = new();

        public static AssetBundle NHAssetBundle { get; private set; }
        public static AssetBundle NHPrivateAssetBundle { get; private set; }
        public static AssetBundle EyeLightning { get; private set; }

        static AssetBundleUtilities()
        {
            NHAssetBundle = LoadRequiredBundle("Assets/bundles/newhorizons_public");
            NHPrivateAssetBundle = LoadRequiredBundle("Assets/bundles/newhorizons_private");
            EyeLightning = LoadRequiredBundle("Assets/bundles/eyelightning");
        }

        private static AssetBundle LoadRequiredBundle(string path)
        {
            var bundle = Main.Instance.ModHelper.Assets.LoadBundle(path);
            if (bundle == null)
            {
                NHLogger.LogError($"Couldn't find [{Path.GetFileName(path)}]: Some features of NH will not work.");
            }
            return bundle;
        }

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
            _loadingBundles.Add(request);
            NHLogger.Log($"Preloading bundle {assetBundleRelativeDir} - {_loadingBundles.Count} left");
            request.completed += _ =>
            {
                _loadingBundles.Remove(request);
                NHLogger.Log($"Finshed preloading bundle {assetBundleRelativeDir} - {_loadingBundles.Count} left");
                AssetBundles[key] = (request.assetBundle, true);
            };
        }

        /// <summary>
        /// are preloaded bundles done loading?
        /// </summary>
        public static bool AreRequiredAssetsLoaded() => _loadingBundles.Count == 0;

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

            // for dream world underwater fog
            foreach (var ruleset in prefab.GetComponentsInChildren<EffectRuleset>(true))
            {
                var material = ruleset._material;
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
            // for raft splash
            foreach (var fluidDetector in prefab.GetComponentsInChildren<FluidDetector>(true))
            {
                if (fluidDetector._splashEffects == null) continue;
                foreach (var splashEffect in fluidDetector._splashEffects)
                {
                    if (splashEffect == null) continue;
                    if (splashEffect.splashPrefab == null) continue;
                    AssetBundleUtilities.ReplaceShaders(splashEffect.splashPrefab);
                }
            }
        }
    }
}