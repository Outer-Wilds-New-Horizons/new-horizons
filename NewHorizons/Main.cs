using NewHorizons.Atmosphere;
using NewHorizons.Body;
using NewHorizons.External;
using NewHorizons.General;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons
{
    public class Main : ModBehaviour
    {
        public static IModHelper helper;

        public static List<NewHorizonsBody> BodyList = new List<NewHorizonsBody>();

        public static List<AstroObject> AstroObjects = new List<AstroObject>();

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            helper = base.ModHelper;

            Utility.Patches.Apply();

            Logger.Log("Begin load of config files...", Logger.LogType.Log);

            foreach (var file in Directory.GetFiles(ModHelper.Manifest.ModFolderPath + @"planets\"))
            {
                try
                {
                    var config = ModHelper.Storage.Load<PlanetConfig>(file.Replace(ModHelper.Manifest.ModFolderPath, ""));
                    Logger.Log($"Loaded {config.Name}");
                    BodyList.Add(new NewHorizonsBody(config));
                }            
                catch(Exception e)
                {
                    Logger.LogError($"Couldn't load {file}: {e.Message}, {e.StackTrace}");
                }
            }

            if (BodyList.Count != 0)
            {
                Logger.Log("Loaded [" + BodyList.Count + "] config files.", Logger.LogType.Log);
            }
            else
            {
                Logger.Log("No config files found!", Logger.LogType.Warning);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "SolarSystem")
            {
                return;
            }

            foreach (var ao in GameObject.FindObjectsOfType<AstroObject>())
            {
                AstroObjects.Add(ao);
            }

            foreach (var body in BodyList)
            {
                var astroObjectName = AstroObject.StringIDToAstroObjectName(body.Config.Name.ToUpper().Replace(" ", "_"));
                var existingPlanet = astroObjectName != AstroObject.Name.None;

                GameObject planetObject;

                if (existingPlanet)
                {
                    var astroObject = Locator.GetAstroObject(astroObjectName);
                    planetObject = UpdateBody(body, astroObject);
                }
                else
                {
                    try
                    {
                        planetObject = GenerateBody(body);
                        var primaryBody = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(body.Config.PrimaryBody));

                        planetObject.transform.parent = Locator.GetRootTransform();
                        var a = body.Config.SemiMajorAxis;
                        var omega = Mathf.Deg2Rad * body.Config.LongitudeOfAscendingNode;
                        planetObject.transform.position = primaryBody.gameObject.transform.position + new Vector3(a * Mathf.Sin(omega), 0, a * Mathf.Cos(omega));
                        planetObject.SetActive(true);
                    }
                    catch(Exception e)
                    {
                        Logger.LogError($"Couldn't generate body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                    }
                }
            }
        }

        public static GameObject UpdateBody(NewHorizonsBody body, AstroObject ao) 
        {
            Logger.Log($"Updating existing AstroObject {ao}");

            var go = ao.gameObject;

            if (body.Config.Destroy)
            { 
                RemoveBody(ao);
                return null;
            }

            var mainSector = go.GetComponentInChildren<Sector>();

            if (body.Config.HasRings)
            {
                RingBuilder.Make(go, body.Config.RingInnerRadius, body.Config.RingOuterRadius, body.Config.RingInclination, body.Config.RingLongitudeOfAscendingNode, body.Config.RingTexture);
            }
            if (body.Config.HasLava)
            {
                LavaBuilder.Make(go, body.Config.LavaSize);
            }
            if (body.Config.HasWater)
            {
                WaterBuilder.Make(go, mainSector, body.Config);
            }
            if(body.Config.HasRain || body.Config.HasSnow)
            {
                EffectsBuilder.Make(go, mainSector, body.Config.WaterSize, body.Config.GroundSize, body.Config.AtmoEndSize / 2f, body.Config.HasRain, body.Config.HasSnow);
            }

            return go;
        }

        private static void RemoveBody(AstroObject ao)
        {
            Logger.Log($"Removing {ao.name}");

            if (ao.GetAstroObjectName() == AstroObject.Name.BrittleHollow)
                RemoveBody(Locator.GetAstroObject(AstroObject.Name.WhiteHole));

            // Check if any other objects depend on it and remove them too
            for(int i = 0; i < AstroObjects.Count; i++)
            {
                var obj = AstroObjects[i];
                if(ao.Equals(obj.GetPrimaryBody()))
                {
                    AstroObjects.Remove(obj);
                    RemoveBody(obj);
                    i--;
                }
            }
            Destroy(ao.gameObject);
        }

        public static GameObject GenerateBody(NewHorizonsBody body)
        {
            Logger.Log("Begin generation sequence of [" + body.Config.Name + "] ...", Logger.LogType.Log);

            var go = new GameObject(body.Config.Name);
            go.SetActive(false);

            if(body.Config.HasGround) GeometryBuilder.Make(go, body.Config.GroundSize);

            AstroObject primaryBody = Locator.GetAstroObject(AstroObject.Name.Sun);
            try
            {
                primaryBody = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(body.Config.PrimaryBody));
            }
            catch(Exception)
            {
                Logger.LogError($"Could not find AstroObject {body.Config.PrimaryBody}, defaulting to SUN");
            }
            
            var outputTuple = BaseBuilder.Make(go, primaryBody, body.Config);

            var owRigidbody = (OWRigidbody)outputTuple.Items[1];
            RFVolumeBuilder.Make(go, owRigidbody, body.Config.AtmoEndSize);

            if (body.Config.HasMapMarker)
            {
                MarkerBuilder.Make(go, body.Config);
            }

            var sector = MakeSector.Make(go, owRigidbody, body.Config);

            if (body.Config.HasClouds)
            {
                CloudsBuilder.Make(go, sector, body.Config);
                SunOverrideBuilder.Make(go, sector, body.Config);
            }

            AirBuilder.Make(go, body.Config.TopCloudSize, body.Config.HasRain);

            if (body.Config.HasWater)
            {
                WaterBuilder.Make(go, sector, body.Config);
            }

            EffectsBuilder.Make(go, sector, body.Config.WaterSize, body.Config.GroundSize, body.Config.AtmoEndSize/2f, body.Config.HasRain, body.Config.HasSnow);
            VolumesBuilder.Make(go, body.Config);
            AmbientLightBuilder.Make(go, sector, body.Config);
            AtmosphereBuilder.Make(go, body.Config);
            if (body.Config.HasRings) 
                RingBuilder.Make(go, body.Config.RingInnerRadius, body.Config.RingOuterRadius, body.Config.RingInclination, body.Config.RingLongitudeOfAscendingNode, body.Config.RingTexture);
            if (body.Config.HasBlackHole) 
                BlackHoleBuilder.Make(go);
            if (body.Config.HasLava)
                LavaBuilder.Make(go, body.Config.LavaSize);

            Logger.Log("Generation of [" + body.Config.Name + "] completed.", Logger.LogType.Log);

            body.Object = go;

            helper.Events.Unity.FireOnNextUpdate(() => OrbitlineBuilder.Make(body.Object, body.Object.GetComponent<AstroObject>(), body.Config.IsMoon));

            return go;
        }

        public static void CreateBody(NewHorizonsBody body)
        {
            Logger.Log($"Running CreateBody for {body.Config.Name}");

            var planet = GenerateBody(body);

            /*
            planet.transform.parent = Locator.GetRootTransform();
            planet.transform.position = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(body.Config.PrimaryBody)).gameObject.transform.position + body.Config.Position;
            planet.SetActive(true);

            planet.GetComponent<OWRigidbody>().SetVelocity(Locator.GetCenterOfTheUniverse().GetOffsetVelocity());

            var primary = Locator.GetAstroObject(AstroObject.StringIDToAstroObjectName(body.Config.PrimaryBody)).GetAttachedOWRigidbody();
            var initialMotion = primary.GetComponent<InitialMotion>();
            if (initialMotion != null)
            {
                planet.GetComponent<OWRigidbody>().AddVelocityChange(-initialMotion.GetInitVelocity());
                planet.GetComponent<OWRigidbody>().AddVelocityChange(primary.GetVelocity());
            }
            */
        }
    }

    public class NewHorizonsApi
    {
        public void Create(Dictionary<string, object> config)
        {
            Logger.Log("Recieved API request to create planet " + (string)config["Name"] + " at position " + (Vector3)config["Position"], Logger.LogType.Log);
            var planetConfig = new PlanetConfig(config);

            var body = new NewHorizonsBody(planetConfig);

            Main.BodyList.Add(body);

            Main.helper.Events.Unity.RunWhen(() => Locator.GetCenterOfTheUniverse() != null, () => Main.CreateBody(body));
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyList.FirstOrDefault(x => x.Config.Name == name).Object;
        }
    }
}
