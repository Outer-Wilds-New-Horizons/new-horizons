using System;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = System.Object;

namespace NewHorizons.Builder.StarSystem
{
    internal class SkyboxBuilder
    {
        public static Material LoadMaterial(string assetBundle, string path, string uniqueModName, IModBehaviour mod)
        {
            string key = uniqueModName + "." + assetBundle;
            AssetBundle bundle;
            Material cubemap;

            try
            {
                if (Main.AssetBundles.ContainsKey(key)) bundle = Main.AssetBundles[key];
                else
                {
                    bundle = mod.ModHelper.Assets.LoadBundle(assetBundle);
                    Main.AssetBundles[key] = bundle;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load AssetBundle {assetBundle} : {e.Message}");
                return null;
            }

            try
            {
                cubemap = bundle.LoadAsset<Material>(path);
            }
            catch (Exception e)
            {
                Logger.Log($"Couldn't load asset {path} from AssetBundle {assetBundle} : {e.Message}");
                return null;
            }

            return cubemap;
        }

        public static void Make(StarSystemConfig.SkyboxConfig info, IModBehaviour mod)
        {
            Logger.Log("Building Skybox");
            Material skyBoxMaterial = LoadMaterial(info.assetBundle, info.path, mod.ModHelper.Manifest.UniqueName, mod);
            RenderSettings.skybox = skyBoxMaterial;
            DynamicGI.UpdateEnvironment();
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                foreach (var camera in Resources.FindObjectsOfTypeAll<OWCamera>())
                {
                    camera.clearFlags = CameraClearFlags.Skybox;
                }
            });
        }
        
    }
}
