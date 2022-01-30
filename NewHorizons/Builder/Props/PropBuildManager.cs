using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Logger = NewHorizons.Utility.Logger;
using System.Reflection;
using NewHorizons.Utility;
using OWML.Common;

namespace NewHorizons.Builder.Props
{
    public static class PropBuildManager
    {
        public static void Make(GameObject go, Sector sector, IPlanetConfig config, IModAssets assets, string uniqueModName)
        {
            if (config.Props.Scatter != null)
            {
                ScatterBuilder.Make(go, sector, config, assets, uniqueModName);
            }
            if(config.Props.Details != null)
            {
                DetailBuilder.Make(go, sector, config, assets, uniqueModName);    
            }
            if(config.Props.Geysers != null)
            {
                foreach(var geyserInfo in config.Props.Geysers)
                {
                    GeyserBuilder.Make(go, sector, geyserInfo);
                }
            }
            if(config.Props.Tornados != null)
            {
                foreach(var tornadoInfo in config.Props.Tornados)
                {
                    TornadoBuilder.Make(go, sector, tornadoInfo, config.Atmosphere?.Cloud != null);
                }
            }
        }

        public static GameObject LoadPrefab(string assetBundle, string path, string uniqueModName, IModAssets assets)
        {
            string key = uniqueModName + "." + assetBundle;
            AssetBundle bundle;
            GameObject prefab;

            try
            {
                if (Main.AssetBundles.ContainsKey(key)) bundle = Main.AssetBundles[key];
                else
                {
                    bundle = assets.LoadBundle(assetBundle);
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
                prefab = bundle.LoadAsset<GameObject>(path);
                prefab.SetActive(false);
            }
            catch (Exception e)
            {
                Logger.Log($"Couldn't load asset {path} from AssetBundle {assetBundle} : {e.Message}");
                return null;
            }

            return prefab;
        }
    }
}
