using NewHorizons.Builder.Props;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons
{
    public class NewHorizonsApi
    {
        [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use Create(Dictionary<string, object> config, IModBehaviour mod) instead")]
        public void Create(Dictionary<string, object> config)
        {
            Create(config, null);
        }

        public void Create(Dictionary<string, object> config, IModBehaviour mod)
        {
            Logger.Log("Recieved API request to create planet " + (string)config["Name"], Logger.LogType.Log);
            var planetConfig = new PlanetConfig(config);

            var body = new NewHorizonsBody(planetConfig, mod ?? Main.Instance);

            if (!Main.BodyDict.ContainsKey(body.Config.StarSystem)) Main.BodyDict.Add(body.Config.StarSystem, new List<NewHorizonsBody>());
            Main.BodyDict[body.Config.StarSystem].Add(body);
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
