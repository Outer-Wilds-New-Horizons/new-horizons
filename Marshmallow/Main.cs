using Marshmallow.External;
using Marshmallow.Utility;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow
{
    public class Main : ModBehaviour
    {
        public static IModHelper helper;

        public static List<MarshmallowBody> planetList = new List<MarshmallowBody>();

        public override object GetApi()
        {
            return new MarshmallowApi();
        }

        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            helper = base.ModHelper;

            Logger.Log("Begin load of planet config...", Logger.LogType.Log);

            try
            {
                foreach (var file in Directory.GetFiles(ModHelper.Manifest.ModFolderPath + @"planets\"))
                {
                    PlanetConfig config = ModHelper.Storage.Load<PlanetConfig>(file.Replace(ModHelper.Manifest.ModFolderPath, ""));
                    planetList.Add(new MarshmallowBody(config));

                    Logger.Log("* " + config.Name + " at position " + config.Position.ToVector3() + " relative to " + config.PrimaryBody + ". Moon? : " + config.IsMoon, Logger.LogType.Log);
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error! - " + ex.Message, Logger.LogType.Error);
            }

            if (planetList.Count != 0)
            {
                Logger.Log("Loaded [" + planetList.Count + "] planet config files.", Logger.LogType.Log);
            }
            else
            {
                Logger.Log("No planet config files found!", Logger.LogType.Warning);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (var planet in planetList)
            {
                var planetObject = GenerateBody(planet.Config);

                var primayBody = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(planet.Config.PrimaryBody));

                planetObject.transform.parent = Locator.GetRootTransform();
                planetObject.transform.position = primayBody.gameObject.transform.position + planet.Config.Position.ToVector3();
                planetObject.SetActive(true);

                planet.Object = planetObject;
            }
        }

        public static GameObject GenerateBody(IPlanetConfig config)
        {
            Logger.Log("Begin generation sequence of planet [" + config.Name + "] ...", Logger.LogType.Log);

            GameObject body;

            body = new GameObject(config.Name);
            body.SetActive(false);

            Body.MakeGeometry.Make(body, config.GroundSize);

            var owRigidbody = General.MakeOrbitingAstroObject.Make(body, Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(config.PrimaryBody)), config);
            General.MakeRFVolume.Make(body, owRigidbody);

            if (config.HasMapMarker)
            {
                General.MakeMapMarker.Make(body, config);
            }

            var sector = Body.MakeSector.Make(body, owRigidbody, config.TopCloudSize);

            if (config.HasClouds)
            {
                Atmosphere.MakeClouds.Make(body, sector, config);
                Atmosphere.MakeSunOverride.Make(body, sector, config);
            }

            Atmosphere.MakeAir.Make(body, config.TopCloudSize / 2, config.HasRain);

            if (config.HasWater)
            {
                Body.MakeWater.Make(body, sector, config);
            }

            Atmosphere.MakeBaseEffects.Make(body, sector);
            Atmosphere.MakeVolumes.Make(body, config);
            General.MakeAmbientLight.Make(body, sector);
            Atmosphere.MakeAtmosphere.Make(body, config);

            if (config.HasSpawnPoint)
            {
                General.MakeSpawnPoint.Make(body, new Vector3(0, config.GroundSize + 10, 0));
            }

            Logger.Log("Generation of planet [" + config.Name + "] completed.", Logger.LogType.Log);

            return body;
        }

        public static void CreateBody(IPlanetConfig config)
        {
            var planet = Main.GenerateBody(config);

            planet.transform.parent = Locator.GetRootTransform();
            planet.transform.position = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(config.PrimaryBody)).gameObject.transform.position + config.Position.ToVector3();
            planet.SetActive(true);

            planet.GetComponent<OWRigidbody>().SetVelocity(Locator.GetCenterOfTheUniverse().GetOffsetVelocity());

            var primary = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(config.PrimaryBody)).GetAttachedOWRigidbody();
            var initialMotion = primary.GetComponent<InitialMotion>();
            if (initialMotion != null)
            {
                planet.GetComponent<OWRigidbody>().AddVelocityChange(-initialMotion.GetInitVelocity());
                planet.GetComponent<OWRigidbody>().AddVelocityChange(primary.GetVelocity());
            }
        }
    }

    public class MarshmallowApi
    {
        public void Create(Dictionary<string, object> config)
        {
            var planetConfig = new PlanetConfig
            {
                Name = (string)config["name"],
                Position = (MVector3)config["position"],
                OrbitAngle = (int)config["orbitAngle"],
                IsMoon = (bool)config["isMoon"],
                PrimaryBody = (string)config["primaryBody"],
                HasSpawnPoint = (bool)config["hasSpawnPoint"],
                HasClouds = (bool)config["hasClouds"],
                TopCloudSize = (float)config["topCloudSize"],
                BottomCloudSize = (float)config["bottomCloudSize"],
                TopCloudTint = (MColor32)config["topCloudTint"],
                BottomCloudTint = (MColor32)config["bottomCloudTint"],
                HasWater = (bool)config["hasWater"],
                WaterSize = (float)config["waterSize"],
                HasRain = (bool)config["hasRain"],
                HasGravity = (bool)config["hasGravity"],
                SurfaceAcceleration = (float)config["surfaceAcceleration"],
                HasMapMarker = (bool)config["hasMapMarker"],
                HasFog = (bool)config["hasFog"],
                FogTint = (MColor32)config["fogTint"],
                FogDensity = (float)config["fogDensity"],
                GroundSize = (float)config["groundScale"]
            };

            Main.CreateBody(planetConfig);
        }

        public GameObject GetPlanet(string name)
        {
            return Main.planetList.FirstOrDefault(x => x.Config.Name == name).Object;
        }
    }
}
