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
using NewHorizons.Builder.General;
using NewHorizons.Utility;
using OWML.Common;
using NewHorizons.Builder.ShipLog;
using NewHorizons.External.Configs;

namespace NewHorizons.Builder.Props
{
    public static class PropBuildManager
    {
        public static void Make(GameObject go, Sector sector, IPlanetConfig config, IModBehaviour mod, string uniqueModName)
        {
            if (config.Props.Scatter != null)
            {
                ScatterBuilder.Make(go, sector, config, mod, uniqueModName);
            }
            if(config.Props.Details != null)
            {
                foreach (var detail in config.Props.Details)
                {
                    DetailBuilder.Make(go, sector, config, mod, uniqueModName, detail);
                }
            }
            if(config.Props.Geysers != null)
            {
                foreach(var geyserInfo in config.Props.Geysers)
                {
                    GeyserBuilder.Make(go, sector, geyserInfo);
                }
            }
            if(config.Props.Rafts != null)
            {
                // TODO
            }
            if(config.Props.Tornados != null)
            {
                foreach(var tornadoInfo in config.Props.Tornados)
                {
                    TornadoBuilder.Make(go, sector, tornadoInfo, config.Atmosphere?.Cloud != null);
                }
            }
            if (config.Props.Volcanoes != null)
            {
                foreach (var volcanoInfo in config.Props.Volcanoes)
                {
                    VolcanoBuilder.Make(go, sector, volcanoInfo);
                }
            }
            // Reminder that dialogue has to be built after props if they're going to be using CharacterAnimController stuff
            if (config.Props.Dialogue != null)
            {
                foreach(var dialogueInfo in config.Props.Dialogue)
                {
                    DialogueBuilder.Make(go, sector, dialogueInfo, mod);
                }
            }
            if (config.Props.Reveal != null)
            {
                foreach (var revealInfo in config.Props.Reveal)
                {
                    RevealBuilder.Make(go, sector, revealInfo, mod);
                }
            }
            if (config.Props.EntryLocation != null)
            {
                foreach (var entryLocationInfo in config.Props.EntryLocation)
                {
                    EntryLocationBuilder.Make(go, sector, entryLocationInfo, mod);
                }
            }
        }

        public static GameObject LoadPrefab(string assetBundle, string path, string uniqueModName, IModBehaviour mod)
        {
            string key = uniqueModName + "." + assetBundle;
            AssetBundle bundle;
            GameObject prefab;

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
                prefab = bundle.LoadAsset<GameObject>(path);
                prefab.SetActive(false);
            }
            catch (Exception e)
            {
                Logger.Log($"Couldn't load asset {path} from AssetBundle {assetBundle} : {e.Message}");
                return null;
            }

            ReplaceShaders(prefab);

            return prefab;
        }

        public static void ReplaceShaders(GameObject prefab)
        {
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null)
                    {
                        continue;
                    }

                    var replacementShader = Shader.Find(material.shader.name);
                    if (replacementShader != null) material.shader = replacementShader;
                }
            }
        }
    }
}
