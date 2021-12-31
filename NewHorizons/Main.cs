using NewHorizons.Atmosphere;
using NewHorizons.Body;
using NewHorizons.Builder.Body;
using NewHorizons.Builder.General;
using NewHorizons.Builder.Orbital;
using NewHorizons.Builder.Props;
using NewHorizons.External;
using NewHorizons.OrbitalPhysics;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static List<NewHorizonsBody> NextPassBodies = new List<NewHorizonsBody>();

        public static float FurthestOrbit = 50000f;

        public StarLightController StarLightController { get; private set; }

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        public void Start()
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
            catch (Exception)
            {
                Logger.LogWarning("Couldn't find planets folder");
            }
        }

        public void OnDestroy()
        {
            Logger.Log($"Destroying NewHorizons");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.Log($"Scene Loaded: {scene.name} {mode}");

            if (scene.name != "SolarSystem") { return; }

            // Need to manage this when there are multiple stars
            var sun = GameObject.Find("Sun_Body");
            var starController = sun.AddComponent<StarController>();
            starController.Light = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>();
            starController.AmbientLight = GameObject.Find("Sun_Body/AmbientLight_SUN").GetComponent<Light>();
            starController.FaceActiveCamera = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<FaceActiveCamera>();
            starController.CSMTextureCacher = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<CSMTextureCacher>();
            starController.ProxyShadowLight = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<ProxyShadowLight>();
            starController.Intensity = 0.9859f;
            starController.SunColor = new Color(1f, 0.8845f, 0.6677f, 1f);

            var starLightGO = GameObject.Instantiate(sun.GetComponentInChildren<SunLightController>().gameObject);
            foreach(var comp in starLightGO.GetComponents<Component>())
            {
                if(!(comp is SunLightController) && !(comp is SunLightParamUpdater) && !(comp is Light) && !(comp is Transform))
                {
                    GameObject.Destroy(comp);
                }
            }
            GameObject.Destroy(starLightGO.GetComponent<Light>());
            starLightGO.name = "StarLightController";

            StarLightController = starLightGO.AddComponent<StarLightController>();
            StarLightController.AddStar(starController);

            starLightGO.SetActive(true);

            // TODO: Make this configurable probably
            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => Locator.GetPlayerBody().gameObject.AddComponent<DebugRaycaster>());

            AstroObjectLocator.RefreshList();
            foreach (AstroObject ao in GameObject.FindObjectsOfType<AstroObject>())
            {
                AstroObjectLocator.AddAstroObject(ao);
            }

            // Stars then planets then moons (not necessary but probably speeds things up, maybe)
            var toLoad = BodyList.OrderBy(b =>
                (b.Config.BuildPriority != -1 ? b.Config.BuildPriority :
                (b.Config.FocalPoint != null ? 0 :
                (b.Config.Star != null) ? 0 :
                (b.Config.Orbit.IsMoon ? 2 : 1)
                ))).ToList();

            var passCount = 0;
            while (toLoad.Count != 0)
            {
                Logger.Log($"Starting body loading pass #{++passCount}");
                var flagNoneLoadedThisPass = true;
                foreach (var body in toLoad)
                {
                    if (LoadBody(body)) flagNoneLoadedThisPass = false;
                }
                if (flagNoneLoadedThisPass)
                {
                    Logger.LogWarning("No objects were loaded this pass");
                    // Try again but default to sun
                    foreach (var body in toLoad)
                    {
                        if (LoadBody(body, true)) flagNoneLoadedThisPass = false;
                    }
                }
                if (flagNoneLoadedThisPass)
                {
                    // Give up
                    Logger.Log($"Couldn't finish adding bodies.");
                    return;
                }

                toLoad = NextPassBodies;
                NextPassBodies = new List<NewHorizonsBody>();

                // Infinite loop failsafe
                if (passCount > 10)
                {
                    Logger.Log("Something went wrong");
                    break;
                }
            }

            Logger.Log("Done loading bodies");

            // I don't know what these do but they look really weird from a distance
            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => PlanetDestroyer.RemoveDistantProxyClones());
        }

        private bool LoadBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
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
                    return false;
                }
            }
            else
            {
                try
                {
                    GameObject planetObject = GenerateBody(body, defaultPrimaryToSun);
                    if (planetObject == null) return false;
                    planetObject.SetActive(true);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't generate body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                    return false;
                }
            }
            return true;
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            var folder = mod.ModHelper.Manifest.ModFolderPath;
            foreach (var file in Directory.GetFiles(folder + @"planets\", "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var config = mod.ModHelper.Storage.Load<PlanetConfig>(file.Replace(folder, ""));
                    Logger.Log($"Loaded {config.Name}");
                    BodyList.Add(new NewHorizonsBody(config, mod.ModHelper.Assets));
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't load {file}: {e.Message}, is your Json formatted correctly?");
                }
            }
        }

        public GameObject UpdateBody(NewHorizonsBody body, AstroObject ao)
        {
            Logger.Log($"Updating existing AstroObject {ao}");

            var go = ao.gameObject;

            var sector = go.GetComponentInChildren<Sector>();
            var rb = go.GetAttachedOWRigidbody();

            // Do stuff that's shared between generating new planets and updating old ones
            return SharedGenerateBody(body, go, sector, rb, ao);
        }

        public GameObject GenerateBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            AstroObject primaryBody;
            if (body.Config.Orbit.PrimaryBody != null)
            {
                primaryBody = AstroObjectLocator.GetAstroObject(body.Config.Orbit.PrimaryBody);
                if (primaryBody == null)
                {
                    if (defaultPrimaryToSun)
                    {
                        Logger.Log($"Couldn't find {body.Config.Orbit.PrimaryBody}, defaulting to Sun");
                        primaryBody = AstroObjectLocator.GetAstroObject("Sun");
                    }
                    else
                    {
                        NextPassBodies.Add(body);
                        return null;
                    }
                }
            }
            else
            {
                primaryBody = null;
            }

            Logger.Log($"Begin generation sequence of [{body.Config.Name}]");

            var go = new GameObject(body.Config.Name.Replace(" ", "").Replace("'", "") + "_Body");
            go.SetActive(false);

            if (body.Config.Base.GroundSize != 0) GeometryBuilder.Make(go, body.Config.Base.GroundSize);

            var atmoSize = body.Config.Atmosphere != null ? body.Config.Atmosphere.Size : 0f;
            float sphereOfInfluence = Mathf.Max(atmoSize, body.Config.Base.SurfaceSize * 2f);

            var outputTuple = BaseBuilder.Make(go, primaryBody, body.Config);
            var ao = (AstroObject)outputTuple.Item1;
            var owRigidBody = (OWRigidbody)outputTuple.Item2;

            GravityVolume gv = null;
            if (body.Config.Base.SurfaceGravity != 0)
                gv = GravityBuilder.Make(go, ao, body.Config.Base.SurfaceGravity, sphereOfInfluence * (body.Config.Star != null ? 10f : 1f), body.Config.Base.SurfaceSize, body.Config.Base.GravityFallOff);

            if (body.Config.Base.HasReferenceFrame)
                RFVolumeBuilder.Make(go, owRigidBody, sphereOfInfluence);

            if (body.Config.Base.HasMapMarker)
                MarkerBuilder.Make(go, body.Config.Name, body.Config);

            if (body.Config.Base.HasAmbientLight)
                AmbientLightBuilder.Make(go, sphereOfInfluence);

            var sector = MakeSector.Make(go, owRigidBody, sphereOfInfluence);
            ao.SetValue("_rootSector", sector);

            VolumesBuilder.Make(go, body.Config.Base.SurfaceSize, sphereOfInfluence);

            if (body.Config.HeightMap != null)
                HeightMapBuilder.Make(go, body.Config.HeightMap, body.Assets);

            if (body.Config.ProcGen != null)
                ProcGenBuilder.Make(go, body.Config.ProcGen);

            if (body.Config.Base.BlackHoleSize != 0)
                BlackHoleBuilder.Make(go, body.Config.Base, sector);

            if (body.Config.Star != null) StarLightController.AddStar(StarBuilder.Make(go, sector, body.Config.Star));

            if (body.Config.FocalPoint != null)
                FocalPointBuilder.Make(go, body.Config.FocalPoint);

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, owRigidBody, ao);

            body.Object = go;

            // Now that we're done move the planet into place
            go.transform.parent = Locator.GetRootTransform();
            var positionVector = OrbitalHelper.RotateTo(Vector3.left * body.Config.Orbit.SemiMajorAxis * (1 + body.Config.Orbit.Eccentricity), body.Config.Orbit);
            go.transform.position = positionVector + (primaryBody == null ? Vector3.zero : primaryBody.transform.position);

            if (go.transform.position.magnitude > FurthestOrbit)
            {
                FurthestOrbit = go.transform.position.magnitude + 30000f;
            }

            // Have to do this after setting position
            var initialMotion = InitialMotionBuilder.Make(go, primaryBody, owRigidBody, body.Config.Orbit);

            // Spawning on other planets is a bit hacky so we do it last
            if (body.Config.Spawn != null)
            {
                SpawnPointBuilder.Make(go, body.Config.Spawn, owRigidBody);
            }

            if (body.Config.Orbit.ShowOrbitLine) OrbitlineBuilder.Make(body.Object, ao, body.Config.Orbit.IsMoon, body.Config);

            if (!body.Config.Orbit.IsStatic) DetectorBuilder.Make(go, owRigidBody, primaryBody, ao);

            if (ao.GetAstroObjectName() == AstroObject.Name.CustomString) AstroObjectLocator.RegisterCustomAstroObject(ao);

            return go;
        }

        private GameObject SharedGenerateBody(NewHorizonsBody body, GameObject go, Sector sector, OWRigidbody rb, AstroObject ao)
        {
            if (body.Config.Ring != null)
                RingBuilder.Make(go, body.Config.Ring, body.Assets);

            if (body.Config.AsteroidBelt != null)
                AsteroidBeltBuilder.Make(body.Config.Name, body.Config.AsteroidBelt, body.Assets);

            if (body.Config.Base.HasCometTail)
                CometTailBuilder.Make(go, body.Config.Base, go.GetComponent<AstroObject>().GetPrimaryBody());

            if (body.Config.Base != null)
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
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere, body.Assets);
                    SunOverrideBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere);
                }

                if (body.Config.Atmosphere.HasRain || body.Config.Atmosphere.HasSnow)
                    EffectsBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere.Size / 2f, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasSnow);

                if (body.Config.Atmosphere.FogSize != 0)
                    FogBuilder.Make(go, sector, body.Config.Atmosphere);

                AtmosphereBuilder.Make(go, body.Config.Atmosphere, body.Config.Base.SurfaceSize);
            }

            if (body.Config.Props != null)
            {
                if (body.Config.Props.Scatter != null) PropBuilder.Scatter(go, body.Config.Props.Scatter, body.Config.Base.SurfaceSize, sector);
                /*
                if (body.Config.Props.Rafts != null)
                {
                    foreach(var v in body.Config.Props.Rafts)
                    {
                        Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => RaftBuilder.Make(go, v, sector, rb, ao));
                    }
                }
                */
            }

            return go;
        }
    }

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

            var body = new NewHorizonsBody(planetConfig, mod != null ? mod.ModHelper.Assets : Main.Instance.ModHelper.Assets);

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
