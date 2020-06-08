using Marshmallow.External;
using Marshmallow.Utility;
using Newtonsoft.Json.Linq;
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
        public static AstroObject ASTROOBJECT;

        public static IModHelper helper;

        GameObject planet;

        static List<PlanetConfig> planetList = new List<PlanetConfig>();

        public override object GetApi()
        {
            return new MarshmallowApi();
        }

        void Start()
        {
            base.ModHelper.Events.Subscribe<Flashlight>(Events.AfterStart);
            IModEvents events = base.ModHelper.Events;
            events.OnEvent = (Action<MonoBehaviour, Events>)Delegate.Combine(events.OnEvent, new Action<MonoBehaviour, Events>(this.OnEvent));

            helper = base.ModHelper;

            Main.Log("Begin load of planet config...");

            try
            {
                foreach (var file in Directory.GetFiles(ModHelper.Manifest.ModFolderPath + @"planets\"))
                {
                    PlanetConfig config = ModHelper.Storage.Load<PlanetConfig>(file.Replace(ModHelper.Manifest.ModFolderPath, ""));
                    planetList.Add(config);

                    Main.Log("* " + config.name + " at position " + config.position.ToVector3());
                }
            }
            catch (Exception ex)
            {
                Main.Log("Error! - " + ex.Message);
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
                    planet = GenerateBody(config);

                    planet.transform.parent = Locator.GetRootTransform();
                    planet.transform.position = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(config.primaryBody)).gameObject.transform.position + config.position.ToVector3();
                    planet.SetActive(true);

                    try
                    {
                        OWRB.SetVelocity(Locator.GetCenterOfTheUniverse().GetOffsetVelocity());
                        var primary = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(config.primaryBody)).GetAttachedOWRigidbody();
                        var initialMotion = primary.GetComponent<InitialMotion>();
                        if (initialMotion != null)
                        {
                            OWRB.AddVelocityChange(-initialMotion.GetInitVelocity());
                        }
                        OWRB.AddVelocityChange(primary.GetVelocity());
                    }
                    catch { }
                }
            }
        }

        public static GameObject GenerateBody(IPlanetConfig config)
        {
            Main.Log("Begin generation sequence of planet [" + config.name + "] ...");

            GameObject body;

            body = new GameObject(config.name);
            body.SetActive(false);

            Body.MakeGeometry.Make(body, config.groundSize);

            ASTROOBJECT = General.MakeOrbitingAstroObject.Make(body, 0.02f, config.orbitAngle, config.hasGravity, config.surfaceAcceleration, config.groundSize);
            General.MakeRFVolume.Make(body);

            if (config.hasMapMarker)
            {
                General.MakeMapMarker.Make(body, config.name);
            }

            SECTOR = Body.MakeSector.Make(body, config.topCloudSize);

            if (config.hasClouds)
            {
                Atmosphere.MakeClouds.Make(body, config.topCloudSize, config.bottomCloudSize, config.bottomCloudTint.ToColor32(), config.topCloudTint.ToColor32());
                Atmosphere.MakeSunOverride.Make(body, config.topCloudSize, config.bottomCloudSize, config.waterSize);
            }

            Atmosphere.MakeAir.Make(body, config.topCloudSize / 2, config.hasRain);

            if (config.hasWater)
            {
                Body.MakeWater.Make(body, config.waterSize);
            }

            Atmosphere.MakeBaseEffects.Make(body);
            Atmosphere.MakeVolumes.Make(body, config.groundSize, config.topCloudSize);
            General.MakeAmbientLight.Make(body);
            Atmosphere.MakeAtmosphere.Make(body, config.topCloudSize, config.hasFog, config.fogDensity, config.fogTint.ToColor32());

            if (config.hasSpawnPoint)
            {
                SPAWN = General.MakeSpawnPoint.Make(body, new Vector3(0, config.groundSize + 10, 0));
            }

            Main.Log("Generation of planet [" + config.name + "] completed.");

            return body;
        }

        public static void CreateBody(IPlanetConfig config)
        {
            var planet = Main.GenerateBody(config);

            planet.transform.parent = Locator.GetRootTransform();
            planet.transform.position = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(config.primaryBody)).gameObject.transform.position + config.position.ToVector3();
            planet.SetActive(true);

            planet.GetComponent<OWRigidbody>().SetVelocity(Locator.GetCenterOfTheUniverse().GetOffsetVelocity());

            var primary = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(config.primaryBody)).GetAttachedOWRigidbody();
            var initialMotion = primary.GetComponent<InitialMotion>();
            if (initialMotion != null)
            {
                planet.GetComponent<OWRigidbody>().AddVelocityChange(-initialMotion.GetInitVelocity());
                planet.GetComponent<OWRigidbody>().AddVelocityChange(primary.GetVelocity());
            }
        }

        public static void Log(string text)
        {
            helper.Console.WriteLine(text);
        }
    }

    public class MarshmallowApi
    {
        public void Create(Dictionary<string, object> config)
        {
            var planetConfig = new PlanetConfig
            {
                name = (string)config["name"],
                position = (MVector3)config["position"],
                orbitAngle = (int)config["orbitAngle"],
                primaryBody = (string)config["primaryBody"],
                hasSpawnPoint = (bool)config["hasSpawnPoint"],
                hasClouds = (bool)config["hasClouds"],
                topCloudSize = (float)config["topCloudSize"],
                bottomCloudSize = (float)config["bottomCloudSize"],
                topCloudTint = (MColor32)config["topCloudTint"],
                bottomCloudTint = (MColor32)config["bottomCloudTint"],
                hasWater = (bool)config["hasWater"],
                waterSize = (float)config["waterSize"],
                hasRain = (bool)config["hasRain"],
                hasGravity = (bool)config["hasGravity"],
                surfaceAcceleration = (float)config["surfaceAcceleration"],
                hasMapMarker = (bool)config["hasMapMarker"],
                hasFog = (bool)config["hasFog"],
                fogTint = (MColor32)config["fogTint"],
                fogDensity = (float)config["fogDensity"],
                groundSize = (float)config["groundScale"]
            };

            Main.CreateBody(planetConfig);
        }
    }
}
