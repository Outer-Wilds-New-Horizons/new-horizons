using Newtonsoft.Json;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Marshmallow
{
    public class Main : ModBehaviour
    {
        public static OWRigidbody OWRB;
        public static Sector SECTOR;
        public static SpawnPoint SPAWN;

        public static IModHelper helper;

        static List<PlanetConfig> planetList = new List<PlanetConfig>();

        void Start()
        {
            base.ModHelper.Events.Subscribe<Flashlight>(Events.AfterStart);
            IModEvents events = base.ModHelper.Events;
            events.OnEvent = (Action<MonoBehaviour, Events>)Delegate.Combine(events.OnEvent, new Action<MonoBehaviour, Events>(this.OnEvent));

            helper = base.ModHelper;

            foreach (var file in Directory.GetFiles(ModHelper.Manifest.ModFolderPath + @"planets\"))
            {
                var config = ModHelper.Storage.Load<PlanetConfig>(file);
                planetList.Add(config);
            }

            if (planetList.Count != 0)
            {
                Main.Log("Loaded [" + planetList.Count + "] planet config files.");
            }
            else
            {
                Main.Log("ERROR! - No planet config files found!");
            }
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev)
        {
            bool flag = behaviour.GetType() == typeof(Flashlight) && ev == Events.AfterStart;
            if (flag)
            {
                foreach (var config in planetList)
                {
                    var planet = GenerateBody(config);

                    if (config.GetSettingsValue<AstroObject>("primaryBody") == Locator.GetAstroObject(AstroObject.Name.Sun))
                    {
                        planet.transform.parent = Locator.GetRootTransform();
                    }
                    else
                    {
                        planet.transform.parent = config.GetSettingsValue<AstroObject>("primaryBody").transform;
                    }

                    planet.transform.position = config.GetSettingsValue<Vector3>("position");
                    planet.SetActive(true);
                }

                PlanetStructure inputStructure = new PlanetStructure
                {
                    name = "Mister_Nebula's Custom Planet!",

                    primaryBody = Locator.GetAstroObject(AstroObject.Name.Sun),
                    aoType = AstroObject.Type.Planet,
                    aoName = AstroObject.Name.InvisiblePlanet,

                    position = new Vector3(0, 0, 30000),

                    makeSpawnPoint = true,

                    hasClouds = true,
                    topCloudSize = 650f,
                    bottomCloudSize = 600f,
                    cloudTint = new Color32(0, 75, 15, 128),

                    hasWater = true,
                    waterSize = 401f,

                    hasRain = true,

                    hasGravity = true,
                    surfaceAccel = 12f,

                    hasMapMarker = true,

                    hasFog = true,
                    fogTint = new Color32(0, 75, 15, 128),
                    fogDensity = 0.75f,

                    hasOrbit = true
                };
            }
        }

        private GameObject GenerateBody(PlanetConfig config)
        {
            Main.Log("Begin generation sequence of planet [" + config.GetSettingsValue<string>("name") + "] ...");

            float groundScale = 400f;

            var name = config.GetSettingsValue<string>("name");
            var topCloudSize = config.GetSettingsValue<float>("topCloudSize");
            var bottomCloudSize = config.GetSettingsValue<float>("topCloudSize");

            GameObject body;

            body = new GameObject(name);
            body.SetActive(false);

            Body.MakeGeometry.Make(body, groundScale);

            General.MakeOrbitingAstroObject.Make(body, config.GetSettingsValue<AstroObject>("primaryBody"), 0.02f, config.GetSettingsValue<bool>("hasGravity"), config.GetSettingsValue<float>("surfaceAcceleration"), groundScale);
            General.MakeRFVolume.Make(body);

            if (config.GetSettingsValue<bool>("hasMapMarker"))
            {
                General.MakeMapMarker.Make(body, name);
            }

            SECTOR = Body.MakeSector.Make(body, topCloudSize);

            if (config.GetSettingsValue<bool>("hasClouds"))
            {
                Atmosphere.MakeClouds.Make(body, topCloudSize, bottomCloudSize, config.GetSettingsValue<Color>("cloudTint"));
                Atmosphere.MakeSunOverride.Make(body, topCloudSize, bottomCloudSize, config.GetSettingsValue<float>("waterSize"));
            }

            Atmosphere.MakeAir.Make(body, topCloudSize / 2, config.GetSettingsValue<bool>("hasRain"));

            if (config.GetSettingsValue<bool>("hasWater"))
            {
                Body.MakeWater.Make(body, config.GetSettingsValue<float>("waterSize"));
            }

            Atmosphere.MakeBaseEffects.Make(body);
            Atmosphere.MakeVolumes.Make(body, groundScale, topCloudSize);
            General.MakeAmbientLight.Make(body);
            Atmosphere.MakeAtmosphere.Make(body, topCloudSize, config.GetSettingsValue<bool>("hasFog"), config.GetSettingsValue<float>("fogDensity"), config.GetSettingsValue<Color>("fogTint"));

            if (config.GetSettingsValue<bool>("makeSpawnPoint"))
            {
                SPAWN = General.MakeSpawnPoint.Make(body, new Vector3(0, groundScale+10, 0));
            }

            Main.Log("Generation of planet [" + name + "] completed.");

            return body;
        }

        public static void Log(string text)
        {
            helper.Console.WriteLine("[Marshmallow] : " + text);
        }
    }
}
