using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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
                PlanetConfig config = ModHelper.Storage.Load<PlanetConfig>(file.Replace(ModHelper.Manifest.ModFolderPath, ""));

                planetList.Add(config);

                Log(config.GetSettingsValue<Vector3>("position").ToString());
                Log(config.GetSettingsValue<Color32>("fogTint").ToString());
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

                    planet.transform.parent = Locator.GetRootTransform();

                    planet.transform.position = config.GetSettingsValue<Vector3>("position");
                    planet.SetActive(true);
                }
            }
        }

        private GameObject GenerateBody(PlanetConfig config)
        {
            Main.Log("Begin generation sequence of planet [" + config.GetSettingsValue<string>("name") + "] ...");

            float groundScale = 400f;

            var name = config.GetSettingsValue<string>("name");
            var topCloudSize = config.GetSettingsValue<float>("topCloudSize");
            var bottomCloudSize = config.GetSettingsValue<float>("bottomCloudSize");

            GameObject body;

            body = new GameObject(name);
            body.SetActive(false);

            Body.MakeGeometry.Make(body, groundScale);

            General.MakeOrbitingAstroObject.Make(body, 0.02f, config.GetSettingsValue<float>("orbitAngle"), config.GetSettingsValue<bool>("hasGravity"), config.GetSettingsValue<float>("surfaceAcceleration"), groundScale);
            General.MakeRFVolume.Make(body);

            if (config.GetSettingsValue<bool>("hasMapMarker"))
            {
                General.MakeMapMarker.Make(body, name);
            }

            SECTOR = Body.MakeSector.Make(body, topCloudSize);

            if (config.GetSettingsValue<bool>("hasClouds"))
            {
                Atmosphere.MakeClouds.Make(body, topCloudSize, bottomCloudSize, config.GetSettingsValue<Color32>("cloudTint"));
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
            Atmosphere.MakeAtmosphere.Make(body, topCloudSize, config.GetSettingsValue<bool>("hasFog"), config.GetSettingsValue<float>("fogDensity"), config.GetSettingsValue<Color32>("fogTint"));

            if (config.GetSettingsValue<bool>("makeSpawnPoint"))
            {
                SPAWN = General.MakeSpawnPoint.Make(body, new Vector3(0, groundScale + 10, 0));
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
