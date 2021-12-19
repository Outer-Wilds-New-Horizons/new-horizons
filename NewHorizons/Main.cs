using NewHorizons.Atmosphere;
using NewHorizons.Body;
using NewHorizons.External;
using NewHorizons.General;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
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

        public static AssetBundle ShaderBundle;
        public static Main Instance { get; private set; }

        public static List<NewHorizonsBody> BodyList = new List<NewHorizonsBody>();
        public static List<NewHorizonsBody> AdditionalBodies = new List<NewHorizonsBody>();

        public IModAssets CurrentAssets { get; private set; }

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Instance = this;
            ShaderBundle = Main.Instance.ModHelper.Assets.LoadBundle("AssetBundle/shader");

            Utility.Patches.Apply();

            Logger.Log("Begin load of config files...", Logger.LogType.Log);

            try
            {
                LoadConfigs(this);
            }
            catch(Exception)
            {
                Logger.LogWarning("Couldn't find planets folder");
            }

        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "SolarSystem") return;

            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => Locator.GetPlayerBody().gameObject.AddComponent<DebugRaycaster>());

            AstroObjectLocator.RefreshList();
            foreach(AstroObject ao in GameObject.FindObjectsOfType<AstroObject>())
            {
                AstroObjectLocator.AddAstroObject(ao);
            }

            // Should make moons come after planets
            BodyList = BodyList.OrderBy(b => (b.Config?.Orbit?.IsMoon)).ToList();

            while(BodyList.Count != 0)
            {
                foreach (var body in BodyList)
                {
                    LoadBody(body);
                }
                BodyList = AdditionalBodies;
                AdditionalBodies = new List<NewHorizonsBody>();
            }
        }

        private void LoadBody(NewHorizonsBody body)
        {
            var stringID = body.Config.Name.ToUpper().Replace(" ", "_").Replace("'", "");
            if (stringID.Equals("ATTLEROCK")) stringID = "TIMBER_MOON";
            if (stringID.Equals("HOLLOWS_LANTERN")) stringID = "VOLCANIC_MOON";
            if (stringID.Equals("ASH_TWIN")) stringID = "TOWER_TWIN";
            if (stringID.Equals("EMBER_TWIN")) stringID = "CAVE_TWIN";
            if (stringID.Equals("INTERLOPER")) stringID = "COMET";

            AstroObject existingPlanet = null;
            try
            {
                existingPlanet = AstroObjectLocator.GetAstroObject(stringID);
                if (existingPlanet == null) existingPlanet = AstroObjectLocator.GetAstroObject(body.Config.Name.Replace(" ", ""));
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Error when looking for {body.Config.Name}: {e.Message}, {e.StackTrace}");
            }

            if (existingPlanet != null)
            {
                try
                {
                    if (body.Config.Destroy)
                    {
                        Instance.ModHelper.Events.Unity.FireInNUpdates(() => PlanetDestroyer.RemoveBody(existingPlanet), 2);
                    }
                    else UpdateBody(body, existingPlanet);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't update body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                }
            }
            else
            {
                try
                {
                    GameObject planetObject;
                    planetObject = GenerateBody(body);
                    planetObject.SetActive(true);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't generate body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                }
            }
        }


        public void LoadConfigs(IModBehaviour mod)
        {
            CurrentAssets = mod.ModHelper.Assets;
            var folder = mod.ModHelper.Manifest.ModFolderPath;
            foreach (var file in Directory.GetFiles(folder + @"planets\"))
            {
                try
                {
                    var config = mod.ModHelper.Storage.Load<PlanetConfig>(file.Replace(folder, ""));
                    Logger.Log($"Loaded {config.Name}");
                    BodyList.Add(new NewHorizonsBody(config));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't load {file}: {e.Message}, {e.StackTrace}");
                }
            }
        }

        public static GameObject UpdateBody(NewHorizonsBody body, AstroObject ao) 
        {
            Logger.Log($"Updating existing AstroObject {ao}");

            var go = ao.gameObject;

            var sector = go.GetComponentInChildren<Sector>();
            var rb = go.GetAttachedOWRigidbody();

            // Do stuff that's shared between generating new planets and updating old ones
            return SharedGenerateBody(body, go, sector, rb);
        }

        public static GameObject GenerateBody(NewHorizonsBody body)
        {
            Logger.Log("Begin generation sequence of [" + body.Config.Name + "] ...", Logger.LogType.Log);

            var go = new GameObject(body.Config.Name.Replace(" ", "").Replace("'", "") + "_Body");
            go.SetActive(false);

            if(body.Config.Base.GroundSize != 0) GeometryBuilder.Make(go, body.Config.Base.GroundSize);

            AstroObject primaryBody = AstroObjectLocator.GetAstroObject(body.Config.Orbit.PrimaryBody);
            if(primaryBody == null)
            {
                Logger.LogError($"Could not find AstroObject {body.Config.Orbit.PrimaryBody}, defaulting to SUN");
                primaryBody = AstroObjectLocator.GetAstroObject(AstroObject.Name.Sun);
            } 

            var atmoSize = body.Config.Atmosphere != null ? body.Config.Atmosphere.Size : 0f;
            float sphereOfInfluence = Mathf.Max(atmoSize, body.Config.Base.SurfaceSize * 2f);

            // Get initial position but set it at the end
            //var a = body.Config.Orbit.SemiMajorAxis;
            //var omega = Mathf.Deg2Rad * body.Config.Orbit.LongitudeOfAscendingNode;
            //var positionVector = primaryBody.gameObject.transform.position + new Vector3(a * Mathf.Sin(omega), 0, a * Mathf.Cos(omega));
            var positionVector = Kepler.OrbitalHelper.CartesianFromOrbitalElements(body.Config.Orbit.Eccentricity, body.Config.Orbit.SemiMajorAxis, body.Config.Orbit.Inclination,
                body.Config.Orbit.LongitudeOfAscendingNode, body.Config.Orbit.ArgumentOfPeriapsis, body.Config.Orbit.TrueAnomaly);

            var outputTuple = BaseBuilder.Make(go, primaryBody, positionVector, body.Config);
            var ao = (AstroObject)outputTuple.Items[0];
            var rb = (OWRigidbody)outputTuple.Items[1];

            if (body.Config.Base.SurfaceGravity != 0)
                GravityBuilder.Make(go, ao, body.Config.Base.SurfaceGravity, sphereOfInfluence, body.Config.Base.SurfaceSize);
            else Logger.Log("No gravity?");
            
            if(body.Config.Base.HasReferenceFrame)
                RFVolumeBuilder.Make(go, rb, sphereOfInfluence);

            if (body.Config.Base.HasMapMarker)
                MarkerBuilder.Make(go, body.Config.Name, body.Config.Orbit.IsMoon);

            var sector = MakeSector.Make(go, rb, sphereOfInfluence);

            VolumesBuilder.Make(go, body.Config.Base.SurfaceSize, sphereOfInfluence);

            if (body.Config.HeightMap != null)
                HeightMapBuilder.Make(go, body.Config.HeightMap);

            if (body.Config.ProcGen != null)
                ProcGenBuilder.Make(go, body.Config.ProcGen);

            InitialMotionBuilder.Make(go, primaryBody, rb, positionVector, body.Config.Orbit);

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, rb);

            body.Object = go;

            // Some things have to be done the second tick
            if(body.Config.Orbit != null && body.Config.Orbit.ShowOrbitLine)
                Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => OrbitlineBuilder.Make(body.Object, ao, body.Config.Orbit.IsMoon, body.Config.Orbit));

            // Now that we're done move the planet into place
            go.transform.parent = Locator.GetRootTransform();
            go.transform.position = positionVector + primaryBody.transform.position;

            // Spawning on other planets is a bit hacky so we do it last
            if (body.Config.Spawn != null)
            {
                SpawnPointBuilder.Make(go, body.Config.Spawn, rb);
            }

            if (ao.GetAstroObjectName() == AstroObject.Name.CustomString) AstroObjectLocator.RegisterCustomAstroObject(ao);

            Logger.Log("Generation of [" + body.Config.Name + "] completed.", Logger.LogType.Log);

            return go;
        }

        private static GameObject SharedGenerateBody(NewHorizonsBody body, GameObject go, Sector sector, OWRigidbody rb)
        {
            if (body.Config.Ring != null)
                RingBuilder.Make(go, body.Config.Ring);

            if (body.Config.AsteroidBelt != null)
                AsteroidBeltBuilder.Make(body.Config.Name, body.Config.AsteroidBelt);
            
            if(body.Config.Base != null)
            {
                if (body.Config.Base.LavaSize != 0)
                    LavaBuilder.Make(go, sector, rb, body.Config.Base.LavaSize);
                if (body.Config.Base.WaterSize != 0)
                    WaterBuilder.Make(go, sector, rb, body.Config.Base.WaterSize);
            }

            if (body.Config.Atmosphere != null)
            {
                AirBuilder.Make(go, body.Config.Atmosphere.Size, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasOxygen);

                if (body.Config.Atmosphere.Cloud != null)
                {
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere);
                    SunOverrideBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere);
                }

                if (body.Config.Atmosphere.HasRain || body.Config.Atmosphere.HasSnow)
                    EffectsBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere.Size / 2f, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasSnow);

                if (body.Config.Atmosphere.FogSize != 0)
                    FogBuilder.Make(go, sector, body.Config.Atmosphere);

                AtmosphereBuilder.Make(go, body.Config.Atmosphere);
            }

            return go;
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
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            Main.Instance.LoadConfigs(mod);
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyList.FirstOrDefault(x => x.Config.Name == name).Object;
        }
    }
}
