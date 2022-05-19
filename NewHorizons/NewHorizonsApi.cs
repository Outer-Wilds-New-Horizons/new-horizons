using NewHorizons.Builder.Props;
using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons
{
    public class NewHorizonsApi
    {
        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config)
        {
            Create(config, null);
        }

        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config, IModBehaviour mod)
        {
            try
            {
                var name = (string)config["Name"];

                Logger.LogWarning($"Recieved API request to create planet [{name}]");

                if (name == null) return;

                var relativePath = $"temp/{name}.json";
                var fullPath = Main.Instance.ModHelper.Manifest.ModFolderPath + relativePath;
                if (!Directory.Exists(Main.Instance.ModHelper.Manifest.ModFolderPath + "temp"))
                {
                    Directory.CreateDirectory(Main.Instance.ModHelper.Manifest.ModFolderPath + "temp");
                }
                JsonHelper.SaveJsonObject(fullPath, config);
                var body = Main.Instance.LoadConfig(Main.Instance, relativePath);
                File.Delete(fullPath);

                // Update it to point to their mod for textures and stuff
                body.Mod = mod ?? Main.Instance;

                if (!Main.BodyDict.ContainsKey(body.Config.StarSystem)) Main.BodyDict.Add(body.Config.StarSystem, new List<NewHorizonsBody>());
                Main.BodyDict[body.Config.StarSystem].Add(body);
            }
            catch(Exception ex)
            {
                Logger.LogError($"Error in Create API: {ex.Message} {ex.StackTrace}");
            }
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            Main.Instance.LoadConfigs(mod);
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyDict.Values.SelectMany(x => x)?.ToList()?.FirstOrDefault(x => x.Config.Name == name)?.Object;
        }

        public string GetCurrentStarSystem()
        {
            return Main.Instance.CurrentStarSystem;
        }

        public UnityEvent<string> GetChangeStarSystemEvent()
        {
            return Main.Instance.OnChangeStarSystem;
        }

        public UnityEvent<string> GetStarSystemLoadedEvent()
        {
            return Main.Instance.OnStarSystemLoaded;
        }

        public bool ChangeCurrentStarSystem(string name)
        {
            if (!Main.SystemDict.ContainsKey(name)) return false;

            Main.Instance.ChangeCurrentStarSystem(name);
            return true;
        }

        public string[] GetInstalledAddons()
        {
            try
            {
                return Main.MountedAddons.Select(x => x?.ModHelper?.Manifest?.UniqueName).ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Couldn't get installed addons {ex.Message}, {ex.StackTrace}");
                return new string[] { };
            }
        }

        public GameObject SpawnObject(GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, float scale, bool alignWithNormal)
        {
            return DetailBuilder.MakeDetail(planet, sector, propToCopyPath, position, eulerAngles, scale, alignWithNormal);
        }
    }
}
