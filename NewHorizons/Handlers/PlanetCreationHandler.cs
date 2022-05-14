using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Body;
using NewHorizons.Builder.General;
using NewHorizons.Builder.Orbital;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.Components.Orbital;
using NewHorizons.External;
using NewHorizons.External.VariableSize;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class PlanetCreationHandler
    {
        public static List<NewHorizonsBody> NextPassBodies = new List<NewHorizonsBody>();

        // I literally forget what this is for
        private static Dictionary<AstroObject, NewHorizonsBody> ExistingAOConfigs;

        private static Dictionary<NHAstroObject, NewHorizonsBody> _dict;

        public static void Init(List<NewHorizonsBody> bodies)
        {
            Main.FurthestOrbit = 30000;

            ExistingAOConfigs = new Dictionary<AstroObject, NewHorizonsBody>();
            _dict = new Dictionary<NHAstroObject, NewHorizonsBody>();

            // Set up stars
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
            foreach (var comp in starLightGO.GetComponents<Component>())
            {
                if (!(comp is SunLightController) && !(comp is SunLightParamUpdater) && !(comp is Light) && !(comp is Transform))
                {
                    GameObject.Destroy(comp);
                }
            }
            GameObject.Destroy(starLightGO.GetComponent<Light>());
            starLightGO.name = "StarLightController";

            starLightGO.AddComponent<StarLightController>();
            StarLightController.AddStar(starController);

            starLightGO.SetActive(true);

            // Load all planets
            var toLoad = bodies.ToList();
            var newPlanetGraph = new PlanetGraphHandler(toLoad);
            
            foreach (var node in newPlanetGraph)
            {
                LoadBody(node.body);
                toLoad.Remove(node.body);

                if (node is PlanetGraphHandler.FocalPointNode focal)
                {
                    LoadBody(focal.primary.body);
                    LoadBody(focal.secondary.body);

                    toLoad.Remove(focal.primary.body);
                    toLoad.Remove(focal.secondary.body);
                }
            }
            
            // Remaining planets are orphaned and either are stock bodies or just incorrectly set up
            var planetGraphs = PlanetGraphHandler.ConstructStockGraph(toLoad.ToArray());

            foreach(var planetGraph in planetGraphs)
            {
                foreach (var node in planetGraph)
                {
                    LoadBody(node.body);
                    toLoad.Remove(node.body);

                    if (node is PlanetGraphHandler.FocalPointNode focal)
                    {
                        LoadBody(focal.primary.body);
                        LoadBody(focal.secondary.body);

                        toLoad.Remove(focal.primary.body);
                        toLoad.Remove(focal.secondary.body);
                    }
                }
            }

            // Are there more?
            foreach(var body in toLoad)
            {
                LoadBody(body);
            }

            Logger.Log("Loading Deferred Bodies");

            // Make a copy of the next pass of bodies so that the array can be edited while we load them
            toLoad = NextPassBodies.Select(x => x).ToList();
            while (NextPassBodies.Count != 0)
            {
                foreach (var body in toLoad)
                {
                    LoadBody(body, true);
                }
                toLoad = NextPassBodies;
                NextPassBodies = new List<NewHorizonsBody>();
            }

            Logger.Log("Done loading bodies");

            // Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(PlanetDestroyer.RemoveAllProxies);

            if (Main.SystemDict[Main.Instance.CurrentStarSystem].Config.destroyStockPlanets) PlanetDestructionHandler.RemoveSolarSystem();
        }

        public static bool LoadBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            // I don't remember doing this why is it exceptions what am I doing
            GameObject existingPlanet = null; 
            try
            {
                existingPlanet = AstroObjectLocator.GetAstroObject(body.Config.Name).gameObject;
            }
            catch (Exception)
            {
                if (body?.Config?.Name == null) Logger.LogError($"How is there no name for {body}");
                else existingPlanet = GameObject.Find(body.Config.Name.Replace(" ", "") + "_Body");
            }

            if (existingPlanet != null)
            {
                try
                {
                    if (body.Config.Destroy)
                    {
                        var ao = existingPlanet.GetComponent<AstroObject>();
                        if (ao != null) Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => PlanetDestructionHandler.RemoveBody(ao), 2);
                        else Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => existingPlanet.SetActive(false), 2);
                    }
                    else if (body.Config.IsQuantumState)
                    {
                        try
                        {
                            var quantumPlanet = existingPlanet.GetComponent<QuantumPlanet>();
                            if (quantumPlanet == null)
                            {
                                // Have to also add the root orbit and sector
                                quantumPlanet = existingPlanet.AddComponent<QuantumPlanet>();
                                var ao = quantumPlanet.GetComponent<NHAstroObject>();

                                var rootSector = quantumPlanet.GetComponentInChildren<Sector>();
                                var groundOrbit = _dict[ao].Config.Orbit;

                                quantumPlanet.groundState = new QuantumPlanet.State(rootSector, groundOrbit);
                                quantumPlanet.states.Add(quantumPlanet.groundState);

                                var visibilityTracker = new GameObject("VisibilityTracker_Sphere");
                                visibilityTracker.transform.parent = existingPlanet.transform;
                                visibilityTracker.transform.localPosition = Vector3.zero;
                                var sphere = visibilityTracker.AddComponent<SphereShape>();
                                sphere.radius = GetSphereOfInfluence(_dict[ao]);
                                var tracker = visibilityTracker.AddComponent<ShapeVisibilityTracker>();
                                quantumPlanet._visibilityTrackers = new VisibilityTracker[] { tracker };
                            }

                            var rb = existingPlanet.GetComponent<OWRigidbody>();

                            var sector = MakeSector.Make(existingPlanet, rb, GetSphereOfInfluence(body));
                            sector.name = $"Sector-{existingPlanet.GetComponentsInChildren<Sector>().Count()}";

                            SharedGenerateBody(body, existingPlanet, sector, rb);

                            // If nothing was generated then forget the sector
                            if (sector.transform.childCount == 0) sector = quantumPlanet.groundState.sector;

                            // If semimajor axis is 0 then forget the orbit
                            var orbit = body.Config.Orbit.SemiMajorAxis == 0 ? quantumPlanet.groundState.orbit : body.Config.Orbit;

                            quantumPlanet.states.Add(new QuantumPlanet.State(sector, orbit));
                        }
                        catch(Exception ex)
                        {
                            Logger.LogError($"Couldn't make quantum state for [{body.Config.Name}] : {ex.Message}, {ex.StackTrace}");
                            return false;
                        }
                    }
                    else
                    {
                        UpdateBody(body, existingPlanet);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"Couldn't update body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                    return false;
                }
            }
            else
            {
                if(body.Config.IsQuantumState)
                {
                    // If the ground state object isn't made yet do it later
                    NextPassBodies.Add(body);
                }
                else
                {
                    try
                    {
                        GameObject planetObject = GenerateBody(body, defaultPrimaryToSun);
                        if (planetObject == null) return false;
                        planetObject.SetActive(true);
                        _dict.Add(planetObject.GetComponent<NHAstroObject>(), body);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError($"Couldn't generate body {body.Config?.Name}: {e.Message}, {e.StackTrace}");
                        return false;
                    }
                }
            }
            return true;
        }

        public static GameObject UpdateBody(NewHorizonsBody body, GameObject go)
        {
            Logger.Log($"Updating existing Object {go.name}");

            var sector = go.GetComponentInChildren<Sector>();
            var rb = go.GetAttachedOWRigidbody();

            // Since orbits are always there just check if they set a semi major axis
            if (body.Config.Orbit != null && body.Config.Orbit.SemiMajorAxis != 0f)
            {
                UpdateBodyOrbit(body, go);
            }

            if (body.Config.ChildrenToDestroy != null && body.Config.ChildrenToDestroy.Length > 0)
            {
                foreach (var child in body.Config.ChildrenToDestroy)
                {
                    Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => GameObject.Find(go.name + "/" + child)?.SetActive(false), 2);
                }
            }

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, rb);

            return go;
        }

        public static GameObject GenerateBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
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

            var owRigidBody = RigidBodyBuilder.Make(go, body.Config);
            var ao = AstroObjectBuilder.Make(go, primaryBody, body.Config);

            var sphereOfInfluence = GetSphereOfInfluence(body);

            var sector = MakeSector.Make(go, owRigidBody, sphereOfInfluence * 2f);
            ao._rootSector = sector;

            if (body.Config.Base.SurfaceGravity != 0)
            {
                GravityBuilder.Make(go, ao, body.Config);
            }

            if (body.Config.Base.HasReferenceFrame)
            {
                RFVolumeBuilder.Make(go, owRigidBody, sphereOfInfluence);
            }

            if (body.Config.Base.HasMapMarker)
            {
                MarkerBuilder.Make(go, body.Config.Name, body.Config);
            }

            VolumesBuilder.Make(go, body.Config, sphereOfInfluence);

            if (body.Config.FocalPoint != null)
            {
                FocalPointBuilder.Make(go, ao, body.Config, body.Mod);
            }

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, owRigidBody);

            body.Object = go;

            // Now that we're done move the planet into place
            UpdatePosition(go, body.Config.Orbit, primaryBody, ao);

            // Have to do this after setting position
            var initialMotion = InitialMotionBuilder.Make(go, primaryBody, ao, owRigidBody, body.Config.Orbit);

            // Spawning on other planets is a bit hacky so we do it last
            if (body.Config.Spawn != null)
            {
                Logger.Log("Doing spawn point thing");
                Main.SystemDict[body.Config.StarSystem].SpawnPoint = SpawnPointBuilder.Make(go, body.Config.Spawn, owRigidBody);
            }

            if (body.Config.Orbit.ShowOrbitLine && !body.Config.Orbit.IsStatic)
            {
                Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => OrbitlineBuilder.Make(body.Object, ao as NHAstroObject, body.Config.Orbit.IsMoon, body.Config));
            }

            if (!body.Config.Orbit.IsStatic)
            {
                DetectorBuilder.Make(go, owRigidBody, primaryBody, ao, body.Config);
            }

            if (ao.GetAstroObjectName() == AstroObject.Name.CustomString)
            {
                AstroObjectLocator.RegisterCustomAstroObject(ao);
            }

            return go;
        }

        private static float GetSphereOfInfluence(NewHorizonsBody body)
        {
            var atmoSize = body.Config.Atmosphere != null ? body.Config.Atmosphere.Size : 0f;
            float sphereOfInfluence = Mathf.Max(Mathf.Max(atmoSize, 50), body.Config.Base.SurfaceSize * 2f);
            var overrideSOI = body.Config.Base.SphereOfInfluence;
            if (overrideSOI != 0) sphereOfInfluence = overrideSOI;
            return sphereOfInfluence;
        }

        private static GameObject SharedGenerateBody(NewHorizonsBody body, GameObject go, Sector sector, OWRigidbody rb)
        {
            var sphereOfInfluence = GetSphereOfInfluence(body);

            if (body.Config.Base.HasAmbientLight)
            {
                AmbientLightBuilder.Make(go, sector, sphereOfInfluence);
            }

            if (body.Config.Base.GroundSize != 0)
            {
                GeometryBuilder.Make(go, sector, body.Config.Base.GroundSize);
            }

            if (body.Config.HeightMap != null)
            {
                HeightMapBuilder.Make(go, sector, body.Config.HeightMap, body.Mod);
            }

            if (body.Config.ProcGen != null)
            {
                ProcGenBuilder.Make(go, sector, body.Config.ProcGen);
            }

            if (body.Config.Star != null)
            {
                StarLightController.AddStar(StarBuilder.Make(go, sector, body.Config.Star));
            }

            if (body.Config.Ring != null)
            {
                RingBuilder.Make(go, sector, body.Config.Ring, body.Mod);
            }

            if (body.Config.AsteroidBelt != null)
            {
                AsteroidBeltBuilder.Make(body.Config.Name, body.Config, body.Mod);
            }

            if (body.Config.Base.HasCometTail)
            {
                CometTailBuilder.Make(go, sector, body.Config, go.GetComponent<AstroObject>().GetPrimaryBody());
            }

            // Backwards compatability
            if (body.Config.Base.LavaSize != 0)
            {
                var lava = new LavaModule();
                lava.Size = body.Config.Base.LavaSize;
                LavaBuilder.Make(go, sector, rb, lava);
            }

            if (body.Config.Lava != null)
            {
                LavaBuilder.Make(go, sector, rb, body.Config.Lava);
            }

            // Backwards compatability
            if (body.Config.Base.WaterSize != 0)
            {
                var water = new WaterModule();
                water.Size = body.Config.Base.WaterSize;
                water.Tint = body.Config.Base.WaterTint;
                WaterBuilder.Make(go, sector, rb, water);
            }

            if (body.Config.Water != null)
            {
                WaterBuilder.Make(go, sector, rb, body.Config.Water);
            }

            if (body.Config.Sand != null)
            {
                SandBuilder.Make(go, sector, rb, body.Config.Sand);
            }

            if (body.Config.Atmosphere != null)
            {
                var airInfo = new AtmosphereModule.AirInfo()
                {
                    HasOxygen = body.Config.Atmosphere.HasOxygen,
                    IsRaining = body.Config.Atmosphere.HasRain,
                    IsSnowing = body.Config.Atmosphere.HasSnow,
                    Scale = body.Config.Atmosphere.Size
                };

                var surfaceSize = body.Config.Base.SurfaceSize;

                AirBuilder.Make(go, sector, airInfo);

                if (body.Config.Atmosphere.Cloud != null)
                {
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere, body.Mod);
                    SunOverrideBuilder.Make(go, sector, body.Config.Atmosphere, surfaceSize);
                }

                if (body.Config.Atmosphere.HasRain || body.Config.Atmosphere.HasSnow)
                    EffectsBuilder.Make(go, sector, airInfo, surfaceSize);

                if (body.Config.Atmosphere.FogSize != 0)
                    FogBuilder.Make(go, sector, body.Config.Atmosphere);

                AtmosphereBuilder.Make(go, sector, body.Config.Atmosphere, surfaceSize);
            }

            if (body.Config.Props != null)
            {
                PropBuildManager.Make(go, sector, rb, body.Config, body.Mod, body.Mod.ModHelper.Manifest.UniqueName);
            }

            if (body.Config.Signal != null)
            {
                SignalBuilder.Make(go, sector, body.Config.Signal, body.Mod);
            }

            if (body.Config.Base.BlackHoleSize != 0 || body.Config.Singularity != null)
            {
                SingularityBuilder.Make(go, sector, rb, body.Config);
            }

            if (body.Config.Funnel != null)
            {
                FunnelBuilder.Make(go, go.GetComponentInChildren<ConstantForceDetector>(), rb, body.Config.Funnel);
            }

            // Has to go last probably
            if (body.Config.Base.CloakRadius != 0f)
            {
                CloakBuilder.Make(go, sector, rb, body.Config.Base.CloakRadius);
            }
			
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                ProxyBuilder.Make(go, body);
            });

            return go;
        }

        public static void UpdateBodyOrbit(NewHorizonsBody body, GameObject go)
        {
            Logger.Log($"Updating orbit of [{body.Config.Name}]");

            try
            {
                var ao = go.GetComponent<AstroObject>();
                var aoName = ao.GetAstroObjectName();
                var aoType = ao.GetAstroObjectType();

                var owrb = go.GetComponent<OWRigidbody>();

                var im = go.GetComponent<InitialMotion>();

                // By default keep it with the same primary body else update to the new one
                var primary = ao._primaryBody;
                if (!string.IsNullOrEmpty(body.Config.Orbit.PrimaryBody))
                {
                    // If we can't find the new one we want to try again later (return false)
                    primary = AstroObjectLocator.GetAstroObject(body.Config.Orbit.PrimaryBody);
                    if (primary == null) return;
                }

                // Just destroy the existing AO after copying everything over
                var newAO = AstroObjectBuilder.Make(go, primary, body.Config);
                newAO._gravityVolume = ao._gravityVolume;
                newAO._moon = ao._moon;
                newAO._name = ao._name;
                newAO._owRigidbody = ao._owRigidbody;
                newAO._rootSector = ao._rootSector;
                newAO._sandLevelController = ao._sandLevelController;
                newAO._satellite = ao._satellite;
                newAO._type = ao._type;

                // We need these for later
                var children = AstroObjectLocator.GetChildren(ao).Concat(AstroObjectLocator.GetMoons(ao)).ToArray();
                AstroObjectLocator.DeregisterCustomAstroObject(ao);
                GameObject.Destroy(ao);
                Locator.RegisterAstroObject(newAO);
                AstroObjectLocator.RegisterCustomAstroObject(newAO);

                newAO._primaryBody = primary;

                // Since we destroyed the AO we have to replace links to it in other places
                newAO.gameObject.GetComponentInChildren<ReferenceFrameVolume>()._referenceFrame._attachedAstroObject = newAO;

                GameObject.Destroy(go.GetComponentInChildren<OrbitLine>().gameObject);
                var isMoon = newAO.GetAstroObjectType() == AstroObject.Type.Moon || newAO.GetAstroObjectType() == AstroObject.Type.Satellite;
                if(body.Config.Orbit.ShowOrbitLine) OrbitlineBuilder.Make(go, newAO, isMoon, body.Config);

                DetectorBuilder.SetDetector(primary, newAO, go.GetComponentInChildren<ConstantForceDetector>());

                // Get ready to move all the satellites
                var relativeMoonPositions = children.Select(x => x.transform.position - go.transform.position).ToArray();

                // If its tidally locked change the alignment
                var alignment = go.GetComponent<AlignWithTargetBody>();
                if (alignment != null)
                {
                    alignment.SetTargetBody(primary.GetComponent<OWRigidbody>());
                }

                // Move the primary
                UpdatePosition(go, body.Config.Orbit, primary, newAO);

                for (int i = 0; i < children.Count(); i++)
                {
                    var child = children[i];

                    // If the child is an AO we do stuff too
                    var childAO = child.GetComponent<NHAstroObject>() ?? child.GetComponent<AstroObject>();
                    if (childAO != null)
                    {
                        if (childAO is NHAstroObject && ExistingAOConfigs.ContainsKey(childAO))
                        {
                            // If it's already and NH object we repeat the whole process else it doesn't work idk
                            NextPassBodies.Add(ExistingAOConfigs[childAO]);
                        }
                        else
                        {
                            foreach (var childChild in AstroObjectLocator.GetChildren(childAO))
                            {
                                if (childChild == null) continue;
                                var dPos = childChild.transform.position - child.transform.position;
                                childChild.transform.position = go.transform.position + relativeMoonPositions[i] + dPos;
                            }
                            // Make sure the moons get updated to the new AO
                            childAO._primaryBody = newAO;
                        }
                    }

                    child.transform.position = go.transform.position + relativeMoonPositions[i];
                }

                // Have to do this after setting position
                InitialMotionBuilder.SetInitialMotion(im, primary, newAO);

                // Have to register this new AO to the locator
                Locator.RegisterAstroObject(newAO);

                ExistingAOConfigs.Add(newAO, body);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Couldn't update orbit of [{body.Config.Name}]: {ex.Message}, {ex.StackTrace}");
                // If it doesn't work here there's no point trying again so we'll still return true
            }

            return;
        }

        public static void UpdatePosition(GameObject go, IOrbitalParameters orbit, AstroObject primaryBody, AstroObject secondaryBody)
        {
            Logger.Log($"Placing [{secondaryBody?.name}] around [{primaryBody?.name}]");

            go.transform.parent = Locator.GetRootTransform();

            if (primaryBody != null)
            {
                var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
                var secondaryGravity = new Gravity(secondaryBody.GetGravityVolume());;

                go.transform.position = orbit.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialPosition + primaryBody.transform.position;
            }
            else
            {
                go.transform.position = Vector3.zero;
            }

            if (go.transform.position.magnitude > Main.FurthestOrbit)
            {
                Main.FurthestOrbit = go.transform.position.magnitude + 30000f;
            }
        }
    }
}
