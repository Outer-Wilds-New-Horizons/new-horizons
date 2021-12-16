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
        public static Main Instance { get; private set; }
        //public static AssetBundle bundle;

        public static List<NewHorizonsBody> BodyList = new List<NewHorizonsBody>();

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Instance = this;

            Utility.Patches.Apply();

            //bundle = ModHelper.Assets.LoadBundle("assets/new-horizons");

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

            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => Locator.GetPlayerBody().gameObject.AddComponent<DebugRaycaster>());

            AstroObjectLocator.RefreshList();
            foreach(AstroObject ao in GameObject.FindObjectsOfType<AstroObject>())
            {
                AstroObjectLocator.AddAstroObject(ao);
            }

            //BodyList = BodyList.OrderBy(x => x.Config.Destroy).ToList();

            foreach (var body in BodyList)
            {
                var stringID = body.Config.Name.ToUpper().Replace(" ", "_").Replace("'", "");
                if (stringID.Equals("ATTLEROCK")) stringID = "TIMBER_MOON";
                if (stringID.Equals("HOLLOWS_LANTERN")) stringID = "VOLCANIC_MOON";
                if (stringID.Equals("ASH_TWIN")) stringID = "TOWER_TWIN";
                if (stringID.Equals("EMBER_TWIN")) stringID = "CAVE_TWIN";
                if (stringID.Equals("INTERLOPER")) stringID = "COMET";

                Logger.Log($"Checking if [{stringID}] already exists");
                AstroObject existingPlanet = null;
                try
                {
                    existingPlanet = AstroObjectLocator.GetAstroObject(stringID);
                    if (existingPlanet == null)
                        existingPlanet = existingPlanet = AstroObjectLocator.GetAstroObject(body.Config.Name.Replace(" ", ""));
                }
                catch(Exception e)
                {
                    Logger.LogWarning($"Error when looking for {body.Config.Name}: {e.Message}, {e.StackTrace}");
                }


                if (existingPlanet != null)
                {
                    try
                    {
                        if (body.Config.Destroy)
                        {
                            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => RemoveBody(existingPlanet));
                        } 
                        else UpdateBody(body, existingPlanet);
                    }
                    catch(Exception e)
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

            var sector = go.GetComponentInChildren<Sector>();
            var rb = go.GetAttachedOWRigidbody();

            if (body.Config.Ring != null)
            {
                RingBuilder.Make(go, body.Config.Ring);
            }
            if (body.Config.Base.LavaSize != 0)
            {
                LavaBuilder.Make(go, sector, rb, body.Config.Base.LavaSize);
            }
            if (body.Config.Base.WaterSize != 0)
            {
                WaterBuilder.Make(go, sector, rb, body.Config.Base.WaterSize);
            }
            if(body.Config.Atmosphere != null)
            {
                AirBuilder.Make(go, body.Config.Atmosphere.Size, body.Config.Atmosphere.HasRain);

                if (body.Config.Atmosphere.Cloud != null)
                {
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere);
                    SunOverrideBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere);
                }

                if (body.Config.Atmosphere.HasRain || body.Config.Atmosphere.HasSnow)
                    EffectsBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere.Size / 2f, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasSnow);

                if (body.Config.Atmosphere.FogSize != 0)
                    FogBuilder.Make(go, sector, body.Config.Atmosphere);

                AtmosphereBuilder.Make(go, body.Config);
            }

            return go;
        }

        private static void RemoveBody(AstroObject ao, List<AstroObject> toDestroy = null)
        {
            Logger.Log($"Removing {ao.name}");

            if (ao.gameObject == null || !ao.gameObject.activeInHierarchy) return;

            if (toDestroy == null) toDestroy = new List<AstroObject>();

            if(toDestroy.Contains(ao))
            {
                Logger.LogError($"Possible infinite recursion in RemoveBody: {ao.name} might be it's own primary body?");
                return;
            }

            toDestroy.Add(ao);

            if (ao.GetAstroObjectName() == AstroObject.Name.BrittleHollow)
                RemoveBody(AstroObjectLocator.GetAstroObject(AstroObject.Name.WhiteHole), toDestroy);

            // Check if any other objects depend on it and remove them too
            var aoArray = AstroObjectLocator.GetAllAstroObjects();
            foreach(AstroObject obj in aoArray)
            {
                if (obj?.gameObject == null || !obj.gameObject.activeInHierarchy)
                {
                    AstroObjectLocator.RemoveAstroObject(obj);
                    continue;
                }
                if (ao.Equals(obj.GetPrimaryBody()))
                {
                    AstroObjectLocator.RemoveAstroObject(obj);
                    RemoveBody(obj, toDestroy);
                }
            }

            if (ao.GetAstroObjectName() == AstroObject.Name.CaveTwin || ao.GetAstroObjectName() == AstroObject.Name.TowerTwin)
            {
                var focalBody = GameObject.Find("FocalBody");
                if(focalBody != null) focalBody.SetActive(false);
            }
            if (ao.GetAstroObjectName() == AstroObject.Name.MapSatellite)
            {
                var msb = GameObject.Find("MapSatellite_Body");
                if (msb != null) msb.SetActive(false);
            }
            if (ao.GetAstroObjectName() == AstroObject.Name.TowerTwin)
                GameObject.Find("TimeLoopRing_Body").SetActive(false);
            if (ao.GetAstroObjectName() == AstroObject.Name.ProbeCannon)
            {
                GameObject.Find("NomaiProbe_Body").SetActive(false);
                GameObject.Find("CannonMuzzle_Body").SetActive(false);
                GameObject.Find("FakeCannonMuzzle_Body (1)").SetActive(false);
                GameObject.Find("CannonBarrel_Body").SetActive(false);
                GameObject.Find("FakeCannonBarrel_Body (1)").SetActive(false);
                GameObject.Find("Debris_Body (1)").SetActive(false);
            }
            if(ao.GetAstroObjectName() == AstroObject.Name.SunStation)
            {
                GameObject.Find("SS_Debris_Body").SetActive(false);
            }
            if(ao.GetAstroObjectName() == AstroObject.Name.GiantsDeep)
            {
                GameObject.Find("BrambleIsland_Body").SetActive(false);
                GameObject.Find("GabbroIsland_Body").SetActive(false);
                GameObject.Find("QuantumIsland_Body").SetActive(false);
                GameObject.Find("StatueIsland_Body").SetActive(false);
                GameObject.Find("ConstructionYardIsland_Body").SetActive(false);
            }
            if(ao.GetAstroObjectName() == AstroObject.Name.WhiteHole)
            {
                GameObject.Find("WhiteholeStation_Body").SetActive(false);
                GameObject.Find("WhiteholeStationSuperstructure_Body").SetActive(false);
            }

            // Deal with proxies
            foreach(var p in GameObject.FindObjectsOfType<ProxyOrbiter>())
            {
                if(p.GetValue<AstroObject>("_originalBody") == ao.gameObject)
                {
                    p.gameObject.SetActive(false);
                    break;
                }
            }
            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => RemoveProxy(ao.name.Replace("_Body", "")));

            ao.transform.root.gameObject.SetActive(false);
        }

        private static void RemoveProxy(string name)
        {
            if (name.Equals("TowerTwin")) name = "AshTwin";
            if (name.Equals("CaveTwin")) name = "EmberTwin";
            var distantProxy = GameObject.Find(name + "_DistantProxy");
            var distantProxyClone = GameObject.Find(name + "_DistantProxy(Clone)");


            if (distantProxy != null) Destroy(distantProxy.gameObject);
            else Logger.LogWarning($"Couldn't find distant proxy {name + "_DistantProxy"}");
            if (distantProxyClone != null) Destroy(distantProxyClone.gameObject);
            else Logger.LogWarning($"Couldn't find distant proxy {name + "_DistantProxy(Clone)"}");
        }

        public static GameObject GenerateBody(NewHorizonsBody body)
        {
            Logger.Log("Begin generation sequence of [" + body.Config.Name + "] ...", Logger.LogType.Log);

            var go = new GameObject(body.Config.Name.Replace(" ", "").Replace("'", "") + "_Body");
            go.SetActive(false);

            if(body.Config.Base.GroundSize != 0) GeometryBuilder.Make(go, body.Config.Base.GroundSize);

            AstroObject primaryBody = AstroObjectLocator.GetAstroObject(AstroObject.Name.Sun);
            try
            {
                primaryBody = AstroObjectLocator.GetAstroObject(body.Config.Orbit.PrimaryBody);
            }
            catch(Exception)
            {
                Logger.LogError($"Could not find AstroObject {body.Config.Orbit.PrimaryBody}, defaulting to SUN");
            }

            var atmoSize = body.Config.Atmosphere != null ? body.Config.Atmosphere.Size : 0f;
            float sphereOfInfluence = Mathf.Max(atmoSize, body.Config.Base.SurfaceSize * 2f);

            // Get initial position but set it at the end
            var a = body.Config.Orbit.SemiMajorAxis;
            var omega = Mathf.Deg2Rad * body.Config.Orbit.LongitudeOfAscendingNode;
            var positionVector = primaryBody.gameObject.transform.position + new Vector3(a * Mathf.Sin(omega), 0, a * Mathf.Cos(omega));

            var outputTuple = BaseBuilder.Make(go, primaryBody, positionVector, body.Config);
            var ao = (AstroObject)outputTuple.Items[0];
            var rb = (OWRigidbody)outputTuple.Items[1];

            if (body.Config.Base.SurfaceGravity != 0)
                GravityBuilder.Make(go, ao, body.Config.Base.SurfaceGravity, sphereOfInfluence, body.Config.Base.SurfaceSize);
            else Logger.Log("No gravity?");
            
            RFVolumeBuilder.Make(go, rb, sphereOfInfluence);

            if (body.Config.Base.HasMapMarker)
                MarkerBuilder.Make(go, body.Config.Name, body.Config.Orbit.IsMoon);

            var sector = MakeSector.Make(go, rb, sphereOfInfluence);

            VolumesBuilder.Make(go, body.Config.Base.SurfaceSize, sphereOfInfluence);

            if (body.Config.HeightMap != null)
                HeightMapBuilder.Make(go, body.Config.HeightMap);

            // These can be shared between creating new planets and updating planets
            if (body.Config.Atmosphere != null)
            {
                AirBuilder.Make(go, body.Config.Atmosphere.Size, body.Config.Atmosphere.HasRain);

                if (body.Config.Atmosphere.Cloud != null)
                {
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere);
                    SunOverrideBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere);
                }

                if(body.Config.Atmosphere.HasRain || body.Config.Atmosphere.HasSnow)
                    EffectsBuilder.Make(go, sector, body.Config.Base.SurfaceSize, body.Config.Atmosphere.Size / 2f, body.Config.Atmosphere.HasRain, body.Config.Atmosphere.HasSnow);

                if (body.Config.Atmosphere.FogSize != 0)
                    FogBuilder.Make(go, sector, body.Config.Atmosphere);

                AtmosphereBuilder.Make(go, body.Config);
            }

            AmbientLightBuilder.Make(go, sector, body.Config.Base.LightTint, sphereOfInfluence);

            if (body.Config.Ring != null) 
                RingBuilder.Make(go, body.Config.Ring);
            
            if (body.Config.Base.BlackHoleSize != 0) 
                BlackHoleBuilder.Make(go);
            
            if (body.Config.Base.LavaSize != 0)
                LavaBuilder.Make(go, sector, rb, body.Config.Base.LavaSize);

            if (body.Config.Base.WaterSize != 0)
                WaterBuilder.Make(go, sector, rb, body.Config.Base.WaterSize);

            Logger.Log("Generation of [" + body.Config.Name + "] completed.", Logger.LogType.Log);

            body.Object = go;

            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => OrbitlineBuilder.Make(body.Object, ao, body.Config.Orbit.IsMoon));

            go.transform.parent = Locator.GetRootTransform();
            go.transform.localPosition = positionVector;

            if (body.Config.Spawn != null)
            {
                SpawnPointBuilder.Make(go, body.Config.Spawn, rb);
            }

            if (ao.GetAstroObjectName() == AstroObject.Name.CustomString) AstroObjectLocator.RegisterCustomAstroObject(ao);

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

            //Main.helper.Events.Unity.RunWhen(() => Locator.GetCenterOfTheUniverse() != null, () => Main.CreateBody(body));
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyList.FirstOrDefault(x => x.Config.Name == name).Object;
        }
    }
}
