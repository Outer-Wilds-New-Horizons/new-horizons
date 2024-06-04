using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Body;
using NewHorizons.Builder.General;
using NewHorizons.Builder.Orbital;
using NewHorizons.Builder.Props;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Builder.Volumes;
using NewHorizons.Components.Orbital;
using NewHorizons.Components.Quantum;
using NewHorizons.Components.Stars;
using NewHorizons.External;
using NewHorizons.OtherMods.OWRichPresence;
using NewHorizons.Streaming;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class PlanetCreationHandler
    {
        private static List<NewHorizonsBody> _nextPassBodies = new List<NewHorizonsBody>();

        // Stock bodies being updated
        private static Dictionary<NHAstroObject, NewHorizonsBody> _existingBodyDict;

        // Custom bodies being created
        private static Dictionary<NHAstroObject, NewHorizonsBody> _customBodyDict;

        // Farthest distance from the center of the solar system
        public static float SolarSystemRadius { get; private set; }
        public static float DefaultFurthestOrbit => 30000f;

        public static List<Action<GameObject, string>> CustomBuilders = new();

        public static void Init(List<NewHorizonsBody> bodies)
        {
            // Start by destroying all planets if need be
            if (Main.SystemDict[Main.Instance.CurrentStarSystem].Config.destroyStockPlanets)
            {
                PlanetDestructionHandler.RemoveStockPlanets();
            }

            // Base game value
            SolarSystemRadius = DefaultFurthestOrbit;

            _existingBodyDict = new();
            _customBodyDict = new();

            if (Main.Instance.CurrentStarSystem != "EyeOfTheUniverse")
            {
                // Set up stars
                // Need to manage this when there are multiple stars
                var sun = SearchUtilities.Find("Sun_Body");
                SupernovaEffectHandler.RegisterSun(sun.GetComponent<SunController>());
                var starController = sun.AddComponent<StarController>();
                starController.Light = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>();
                starController.AmbientLight = SearchUtilities.Find("Sun_Body/AmbientLight_SUN").GetComponent<Light>();
                starController.FaceActiveCamera = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<FaceActiveCamera>();
                starController.CSMTextureCacher = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<CSMTextureCacher>();
                starController.ProxyShadowLight = SearchUtilities.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<ProxyShadowLight>();
                starController.Intensity = 0.9859f;
                starController.SunColor = new Color(1f, 0.8845f, 0.6677f, 1f);

                var starLightGO = UnityEngine.Object.Instantiate(sun.GetComponentInChildren<SunLightController>().gameObject);
                foreach (var comp in starLightGO.GetComponents<Component>())
                {
                    if (comp is not (SunLightController or SunLightParamUpdater or Light or Transform))
                    {
                        UnityEngine.Object.Destroy(comp);
                    }
                }
                UnityEngine.Object.Destroy(starLightGO.GetComponent<Light>());
                starLightGO.name = "StarLightController";

                starLightGO.AddComponent<SunLightEffectsController>();
                SunLightEffectsController.AddStar(starController);
                SunLightEffectsController.AddStarLight(starController.Light);

                starLightGO.SetActive(true);
            }

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

            foreach (var planetGraph in planetGraphs)
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
            foreach (var body in toLoad)
            {
                LoadBody(body);
            }

            NHLogger.Log("Loading Deferred Bodies");

            // Make a copy of the next pass of bodies so that the array can be edited while we load them
            toLoad = _nextPassBodies.ToList();
            while (_nextPassBodies.Count != 0)
            {
                foreach (var body in toLoad)
                {
                    LoadBody(body, true);
                }
                toLoad = _nextPassBodies;
                _nextPassBodies = new List<NewHorizonsBody>();
            }

            NHLogger.Log("Done loading bodies");

            SingularityBuilder.PairAllSingularities();
        }

        public static bool LoadBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            body.LoadCache();

            // I don't remember doing this why is it exceptions what am I doing
            GameObject existingPlanet = null;
            try
            {
                existingPlanet = AstroObjectLocator.GetAstroObject(body.Config.name).gameObject;
            }
            catch (Exception)
            {
                if (body?.Config?.name == null)
                {
                    NHLogger.LogError($"How is there no name for {body}");
                }
                else
                {
                    existingPlanet = SearchUtilities.Find(body.Config.name.Replace(" ", "") + "_Body", false);
                }
            }

            if (existingPlanet == null && body.Config.destroy)
            {
                NHLogger.LogError($"{body.Config.name} was meant to be destroyed, but was not found");
                return false;
            }

            if (existingPlanet != null)
            {
                try
                {
                    if (body.Config.destroy)
                    {
                        var ao = existingPlanet.GetComponent<AstroObject>();
                        if (ao != null)
                        {
                            Delay.FireInNUpdates(() => PlanetDestructionHandler.DisableAstroObject(ao), 2);
                        }
                        else
                        {
                            Delay.FireInNUpdates(() => PlanetDestructionHandler.DisableGameObject(existingPlanet), 2);
                        }
                    }
                    else if (body.Config.isQuantumState)
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
                                var groundOrbit = _customBodyDict[ao].Config.Orbit;

                                quantumPlanet.groundState = new QuantumPlanet.State(rootSector, groundOrbit);
                                quantumPlanet.states.Add(quantumPlanet.groundState);

                                var visibilityTracker = new GameObject("VisibilityTracker_Sphere");
                                visibilityTracker.transform.parent = existingPlanet.transform;
                                visibilityTracker.transform.localPosition = Vector3.zero;
                                var sphere = visibilityTracker.AddComponent<SphereShape>();
                                sphere.radius = GetSphereOfInfluence(_customBodyDict[ao]);
                                var tracker = visibilityTracker.AddComponent<ShapeVisibilityTracker>();
                                quantumPlanet._visibilityTrackers = new VisibilityTracker[] { tracker };
                            }

                            var rb = existingPlanet.GetComponent<OWRigidbody>();

                            var sector = SectorBuilder.Make(existingPlanet, rb, GetSphereOfInfluence(body));
                            sector.name = $"Sector-{existingPlanet.GetComponentsInChildren<Sector>().Count()}";

                            SharedGenerateBody(body, existingPlanet, sector, rb);

                            // If nothing was generated then forget the sector
                            if (sector.transform.childCount == 0) sector = quantumPlanet.groundState.sector;

                            // If semimajor axis is 0 then forget the orbit
                            var orbit = body.Config.Orbit.semiMajorAxis == 0 ? quantumPlanet.groundState.orbit : body.Config.Orbit;

                            quantumPlanet.states.Add(new QuantumPlanet.State(sector, orbit));
                        }
                        catch (Exception ex)
                        {
                            NHLogger.LogError($"Couldn't make quantum state for [{body.Config.name}]:\n{ex}");
                            body.UnloadCache();
                            return false;
                        }
                    }
                    else if (body.Config.isStellarRemnant)
                    {
                        //Skip
                    }
                    else
                    {
                        UpdateBody(body, existingPlanet);
                    }
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Couldn't update body {body.Config?.name}:\n{e}");
                    body.UnloadCache();
                    return false;
                }
            }
            else
            {
                if (body.Config.isQuantumState)
                {
                    // If the ground state object isn't made yet do it later
                    _nextPassBodies.Add(body);
                }
                else if (body.Config.isStellarRemnant)
                {
                    //Skip
                }
                else
                {
                    try
                    {
                        NHLogger.Log($"Creating [{body.Config.name}]");
                        var planetObject = GenerateBody(body, defaultPrimaryToSun) 
                            ?? throw new NullReferenceException("Something went wrong when generating the body but no errors were logged.");
                        
                        planetObject.SetActive(true);

                        var ao = planetObject.GetComponent<NHAstroObject>();

                        var solarSystemRoot = SearchUtilities.Find("SolarSystemRoot").transform;
                        planetObject.GetComponent<OWRigidbody>()._origParent = ao.IsDimension ? solarSystemRoot.Find("Dimensions") : solarSystemRoot;

                        _customBodyDict.Add(ao, body);
                    }
                    catch (Exception e)
                    {
                        NHLogger.LogError($"Couldn't generate body {body.Config?.name}:\n{e}");
                        body.UnloadCache();
                        return false;
                    }
                }
            }

            try
            {
                Main.Instance.OnPlanetLoaded?.Invoke(body.Config.name);
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Error in event handler for OnPlanetLoaded on body {body.Config.name}: {e}");
            }
            
            body.UnloadCache(true);
            return true;
        }

        public static Sector CreateSectorFromParent(GameObject planetGO, OWRigidbody rigidbody)
        {
            switch (planetGO.name)
            {
                case "TimeLoopRing_Body":
                    return SectorBuilder.Make(planetGO, rigidbody, SearchUtilities.Find("TowerTwin_Body/Sector_TowerTwin/Sector_TimeLoopInterior").GetComponent<Sector>());
                case "SandFunnel_Body":
                    return SectorBuilder.Make(planetGO, rigidbody, SearchUtilities.Find("FocalBody/Sector_HGT").GetComponent<Sector>());
                case "SS_Debris_Body":
                    return SectorBuilder.Make(planetGO, rigidbody, SearchUtilities.Find("SunStation_Body/Sector_SunStation").GetComponent<Sector>());
                case "WhiteholeStationSuperstructure_Body":
                    return SectorBuilder.Make(planetGO, rigidbody, SearchUtilities.Find("WhiteholeStation_Body/Sector_WhiteholeStation").GetComponent<Sector>());
                case "MiningRig_Body":
                    return SectorBuilder.Make(planetGO, rigidbody, SearchUtilities.Find("TimberHearth_Body/Sector_TH/Sector_ZeroGCave").GetComponent<Sector>());
                default:
                    return null;
            }
        }

        // Called when updating an existing planet
        public static GameObject UpdateBody(NewHorizonsBody body, GameObject go)
        {
            NHLogger.Log($"Updating existing Object {go.name}");

            var rb = go.GetAttachedOWRigidbody();
            var sector = go.GetComponentInChildren<Sector>() ?? CreateSectorFromParent(go, rb);

            // Since orbits are always there just check if they set a semi major axis
            if (body.Config.Orbit != null && body.Config.Orbit.semiMajorAxis != 0f)
            {
                UpdateBodyOrbit(body, go);
            }

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, rb);

            if (body.Config.ShipLog?.mapMode != null)
            {
                MapModeBuilder.TryReplaceExistingMapModeIcon(body, body.Mod as ModBehaviour, body.Config.ShipLog.mapMode);
            }

            body.Object = go;

            return go;
        }

        // Only called when making new planets
        public static GameObject GenerateBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            if (body.Config?.Bramble?.dimension != null)
            {
                return GenerateBrambleDimensionBody(body);
            }
            else
            {
                return GenerateStandardBody(body, defaultPrimaryToSun);
            }
        }

        public static GameObject GenerateBrambleDimensionBody(NewHorizonsBody body)
        {
            var go = new GameObject(body.Config.name.Replace(" ", "").Replace("'", "") + "_Body");
            go.SetActive(false);

            body.Config.Base.showMinimap = false;
            body.Config.Base.hasMapMarker = false;

            const float sphereOfInfluence = 2000f;
            
            var owRigidBody = RigidBodyBuilder.Make(go, sphereOfInfluence, body.Config);
            var ao = AstroObjectBuilder.Make(go, null, body, false);

            var sector = SectorBuilder.Make(go, owRigidBody, sphereOfInfluence);
            ao._rootSector = sector;
            ao._type = AstroObject.Type.None;

            BrambleDimensionBuilder.Make(body, go, ao, sector, body.Mod, owRigidBody);

            go = SharedGenerateBody(body, go, sector, owRigidBody);
            
            // Not included in SharedGenerate to not mess up gravity on base game planets
            if (body.Config.Base.surfaceGravity != 0)
            {
                GravityBuilder.Make(go, ao, owRigidBody, body.Config);
            }

            body.Object = go;

            AstroObjectLocator.RegisterCustomAstroObject(ao);

            // Now that we're done move the planet into place
            SetPositionFromVector(go, body.Config.Orbit.staticPosition);

            NHLogger.LogVerbose($"Finished creating Bramble Dimension [{body.Config.name}]");

            return go;
        }

        public static GameObject GenerateStandardBody(NewHorizonsBody body, bool defaultPrimaryToSun = false)
        {
            // Focal points are weird
            if (body.Config.FocalPoint != null) FocalPointBuilder.ValidateConfig(body.Config);

            AstroObject primaryBody;
            if (body.Config.Orbit.primaryBody != null)
            {
                primaryBody = AstroObjectLocator.GetAstroObject(body.Config.Orbit.primaryBody);
                if (primaryBody == null)
                {
                    if (defaultPrimaryToSun)
                    {
                        NHLogger.LogError($"Couldn't find {body.Config.Orbit.primaryBody}, defaulting to center of solar system");
                        // TODO: Make this work in other systems. We tried using Locator.GetCenterOfUniverse before but that doesn't work since its too early now
                        primaryBody = SearchUtilities.Find("Sun_Body")?.GetComponent<AstroObject>();
                    }
                    else
                    {
                        _nextPassBodies.Add(body);
                        return null;
                    }
                }
            }
            else
            {
                primaryBody = null;
            }

            var go = new GameObject(body.Config.name.Replace(" ", "").Replace("'", "") + "_Body");
            go.SetActive(false);

            if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse")
            {
                // Disable any bodies when not at eye, vessel, or vortex.
                EyeStateActivationController eyeStateActivation = SearchUtilities.Find("SolarSystemRoot").AddComponent<EyeStateActivationController>();
                eyeStateActivation._object = go;
                eyeStateActivation._activeStates = new EyeState[3]
                {
                    EyeState.AboardVessel,
                    EyeState.WarpedToSurface,
                    EyeState.IntoTheVortex
                };
            }

            var sphereOfInfluence = GetSphereOfInfluence(body);
            
            var owRigidBody = RigidBodyBuilder.Make(go, sphereOfInfluence, body.Config);
            var ao = AstroObjectBuilder.Make(go, primaryBody, body, false);

            var sector = SectorBuilder.Make(go, owRigidBody, sphereOfInfluence * 2f);
            ao._rootSector = sector;

            if (body.Config.Base.surfaceGravity != 0)
            {
                GravityBuilder.Make(go, ao, owRigidBody, body.Config);
            }

            RFVolumeBuilder.Make(go, owRigidBody, sphereOfInfluence, body.Config.ReferenceFrame);

            if (body.Config.Base.hasMapMarker)
            {
                MarkerBuilder.Make(go, body.Config.name, body.Config);
            }

            VolumesBuilder.Make(go, owRigidBody, body.Config, sphereOfInfluence);

            if (body.Config.FocalPoint != null)
            {
                FocalPointBuilder.Make(go, ao, body.Config, body.Mod);
            }

            // Do stuff that's shared between generating new planets and updating old ones
            go = SharedGenerateBody(body, go, sector, owRigidBody);

            body.Object = go;

            // Now that we're done move the planet into place
            if (body.Config.Orbit?.staticPosition != null)
            {
                SetPositionFromVector(go, body.Config.Orbit.staticPosition);
            }
            else
            {
                UpdatePosition(go, body.Config.Orbit, primaryBody, ao);
            }

            // Have to do this after setting position
            var initialMotion = InitialMotionBuilder.Make(go, primaryBody, ao, owRigidBody, body.Config.Orbit);

            // Spawning on other planets is a bit hacky so we do it last
            if (body.Config.Spawn != null)
            {
                NHLogger.LogVerbose($"Making spawn point on {body.Config.name}");
                var spawnPoint = SpawnPointBuilder.Make(go, body.Config.Spawn, owRigidBody);
                var isVanillaSystem = body.Config.starSystem == "SolarSystem" || body.Config.starSystem == "EyeOfTheUniverse";
                var needsSpawnPoint = Main.SystemDict[body.Config.starSystem].SpawnPoint == null || isVanillaSystem;
                var isDefaultSpawn = body.Config.Spawn.playerSpawn?.isDefault ?? true; // Backwards compat
                if (needsSpawnPoint || isDefaultSpawn)
                {
                    Main.SystemDict[body.Config.starSystem].SpawnPoint = spawnPoint;
                }
            }

            if (body.Config.Orbit.showOrbitLine && !body.Config.Orbit.isStatic)
            {
                Delay.FireOnNextUpdate(() => OrbitlineBuilder.Make(body.Object, ao, body.Config.Orbit.isMoon, body.Config));
            }

            DetectorBuilder.Make(go, owRigidBody, primaryBody, ao, body.Config);

            AstroObjectLocator.RegisterCustomAstroObject(ao);

            var otherBodies = Main.BodyDict[Main.Instance.CurrentStarSystem];
            var remnant = otherBodies.Where(x => x.Config.isStellarRemnant && x.Config.name == body.Config.name).FirstOrDefault();
            // TODO: add proxies for quantum states
            //var quantumStates = otherBodies.Where(x => x.Config.isQuantumState && x.Config.name == body.Config.name).ToArray();
            if (!(body.Config.Cloak != null && body.Config.Cloak.radius != 0f) && !body.Config.Base.hideProxy)
            {
                Delay.FireOnNextUpdate(() =>
                {
                    ProxyBuilder.Make(go, body, remnant);
                });
            }

            RichPresenceHandler.SetUpPlanet(body.Config.name, go, sector, body.Config.Star != null, body.Config.Atmosphere != null);

            NHLogger.LogVerbose($"Finished creating [{body.Config.name}]");

            return go;
        }

        private static float GetSphereOfInfluence(NewHorizonsBody body)
        {
            var atmoSize = body.Config.Atmosphere != null ? body.Config.Atmosphere.size : 0f;
            float sphereOfInfluence = Mathf.Max(Mathf.Max(atmoSize, 50), body.Config.Base.surfaceSize * 2f);
            var overrideSOI = body.Config.Base.soiOverride;
            if (overrideSOI != 0) sphereOfInfluence = overrideSOI;
            return sphereOfInfluence;
        }

        // What is called both on existing planets and new planets
        private static GameObject SharedGenerateBody(NewHorizonsBody body, GameObject go, Sector sector, OWRigidbody rb)
        {
            var sphereOfInfluence = GetSphereOfInfluence(body);

            Light[] ambientLight = null;
            if (body.Config.AmbientLights != null)
            {
                ambientLight = new Light[0];
                foreach (var light in body.Config.AmbientLights) ambientLight.Append(AmbientLightBuilder.Make(go, sector, light, body.Config.Base.surfaceSize)).ToArray();
            }

            if (body.Config.Base.groundSize != 0)
            {
                GeometryBuilder.Make(go, sector, body.Config.Base.groundSize);
            }

            if (body.Config.HeightMap != null)
            {
                // resolution = tris on edge per face
                // divide by 4 to account for all the way around the equator
                var res = body.Config.HeightMap.resolution / 4;
                HeightMapBuilder.Make(go, sector, body.Config.HeightMap, body.Mod, res, true);
            }

            GameObject procGen = null;
            if (body.Config.ProcGen != null)
            {
                procGen = ProcGenBuilder.Make(go, sector, body.Config.ProcGen);
            }

            if (body.Config.Star != null)
            {
                var (star, starController, starEvolutionController, starLight) = StarBuilder.Make(go, sector, body.Config.Star, body.Mod, body.Config.isStellarRemnant);

                if (starController != null) SunLightEffectsController.AddStar(starController);
                if (starLight != null) SunLightEffectsController.AddStarLight(starLight);

                // If it has an evolution controller that means it will die -> we make a remnant (unless its a remnant)
                if (starEvolutionController != null && !body.Config.isStellarRemnant)
                {
                    GameObject remnantGO;

                    // Create the remnant as if it were a planet
                    if (body.Config.Star.stellarRemnantType == External.Modules.VariableSize.StellarRemnantType.Custom)
                    {
                        var remnant = Main.BodyDict[body.Config.starSystem].Where(x => x.Config.name == body.Config.name && x.Config.isStellarRemnant).FirstOrDefault();

                        var remnantSector = SectorBuilder.Make(go, rb, sphereOfInfluence);
                        remnantSector.name = "CustomStellarRemnant";

                        SharedGenerateBody(remnant, go, remnantSector, rb);

                        remnantGO = remnantSector.gameObject;
                    }
                    else
                    {
                        remnantGO = StellarRemnantBuilder.Make(go, rb, sphereOfInfluence, body.Mod, body);
                    }

                    if (remnantGO != null)
                    {
                        remnantGO.SetActive(false);
                        starEvolutionController.SetStellarRemnant(remnantGO);
                    }
                }
            }

            if (body.Config?.Bramble != null)
            {
                if (body.Config.Bramble.nodes != null)
                {
                    BrambleNodeBuilder.Make(go, sector, body.Config.Bramble.nodes, body.Mod);
                }

                if (body.Config.Bramble.dimension != null)
                {
                    BrambleNodeBuilder.FinishPairingNodesForDimension(body.Config.name, go.GetComponent<AstroObject>());
                }
            }

            if (body.Config.Rings != null)
            {
                foreach (var ring in body.Config.Rings)
                {
                    RingBuilder.Make(go, sector, ring, body.Mod);
                }
            }

            if (body.Config.AsteroidBelt != null)
            {
                AsteroidBeltBuilder.Make(body.Config.name, body.Config, body.Mod);
            }

            if (body.Config.CometTail != null)
            {
                CometTailBuilder.Make(go, sector, body.Config.CometTail, body.Config);
            }

            if (body.Config.Lava != null)
            {
                LavaBuilder.Make(go, sector, rb, body.Config.Lava);
            }

            if (body.Config.Water != null)
            {
                WaterBuilder.Make(go, sector, rb, body.Config);
            }

            if (body.Config.Sand != null)
            {
                SandBuilder.Make(go, sector, rb, body.Config.Sand);
            }

            var willHaveCloak = body.Config.Cloak != null && body.Config.Cloak.radius != 0f;
            PlanetaryFogController fog = null;
            LODGroup atmosphere = null;
            if (body.Config.Atmosphere != null)
            {
                var surfaceSize = body.Config.Base.surfaceSize;

                if (body.Config.Atmosphere.size != 0)
                    AirBuilder.Make(go, sector, body.Config);

                if (!string.IsNullOrEmpty(body.Config.Atmosphere?.clouds?.texturePath))
                {
                    CloudsBuilder.Make(go, sector, body.Config.Atmosphere, willHaveCloak, body.Mod);
                    if (body.Config.Atmosphere.clouds.cloudsPrefab != External.Modules.CloudPrefabType.Transparent)
                    {
                        SunOverrideBuilder.Make(go, sector, body.Config.Atmosphere, body.Config.Water, surfaceSize);
                    }
                }

                if (body.Config.Atmosphere.fogSize != 0)
                {
                    fog = FogBuilder.Make(go, sector, body.Config.Atmosphere, body.Mod);
                }

                atmosphere = AtmosphereBuilder.Make(go, sector, body.Config.Atmosphere, surfaceSize).GetComponentInChildren<LODGroup>();
            }

            if (body.Config.ParticleFields != null)
            {
                EffectsBuilder.Make(go, sector, body.Config);
            }

            if (body.Config.Props != null)
            {
                PropBuildManager.Make(go, sector, rb, body);
            }

            if (body.Config.Volumes != null)
            {
                VolumesBuildManager.Make(go, sector, rb, body.Config, body.Mod);
            }

            if (body.Config.Funnel != null)
            {
                FunnelBuilder.Make(go, sector, rb, body.Config.Funnel);
            }

            if (body.Config.extras != null)
            {
                foreach (var customBuilder in CustomBuilders)
                {
                    try
                    {
                        customBuilder.Invoke(go, JsonConvert.SerializeObject(body.Config.extras));
                    }
                    catch (Exception e)
                    {
                        NHLogger.LogError($"Failed to use custom builder on body {body.Config.name} - {e}");
                    }
                }
            }

            // Has to go last probably
            if (willHaveCloak)
            {
                CloakBuilder.Make(go, sector, rb, body.Config.Cloak, !body.Config.ReferenceFrame.hideInMap, body.Mod);
            }

            if (body.Config.ShockEffect?.hasSupernovaShockEffect == true)
            {
                SupernovaEffectBuilder.Make(go, sector, body.Config, body.Mod, procGen, ambientLight, fog, atmosphere, null, fog?._fogImpostor);
            }

            // We allow removing children afterwards so you can also take bits off of the modules you used
            if (body.Config.removeChildren != null) RemoveChildren(go, body);

            return go;
        }

        public static void UpdateBodyOrbit(NewHorizonsBody body, GameObject go)
        {
            NHLogger.Log($"Updating orbit of [{body.Config.name}]");

            try
            {
                var ao = go.GetComponent<AstroObject>();
                var aoName = ao.GetAstroObjectName();
                var aoType = ao.GetAstroObjectType();

                // When updating orbits of the twins be sure the FocalBody is gone
                // Don't do it if it's already an NHAstroObject since that means this was already done
                if (ao is not NHAstroObject && (aoName == AstroObject.Name.TowerTwin || aoName == AstroObject.Name.CaveTwin))
                {
                    var hourglassTwinsFocal = SearchUtilities.Find("FocalBody");

                    // Have to copy the HGT sector bc it has some ruleset stuff on it we want
                    var clonedSectorHGT = hourglassTwinsFocal.FindChild("Sector_HGT").Instantiate().Rename("Sector_HGT");
                    clonedSectorHGT.transform.parent = go.transform;
                    clonedSectorHGT.transform.localPosition = Vector3.zero;
                    clonedSectorHGT.transform.localRotation = Quaternion.identity;

                    // Don't need this part
                    GameObject.Destroy(clonedSectorHGT.GetComponentInChildren<SectorStreaming>().gameObject);

                    // Take the hourglass twins shader effect controller off the focal body so it can stay active
                    var shaderController = hourglassTwinsFocal.GetComponentInChildren<HourglassTwinsShaderController>();
                    if (shaderController != null) shaderController.transform.parent = null;

                    hourglassTwinsFocal.SetActive(false);

                    // Remove the drift tracker since its unneeded now
                    Component.Destroy(go.GetComponent<DriftTracker>());

                    // Fix sectors
                    VanillaStreamingFix.UnparentSectorStreaming(ao.GetRootSector(), ao.gameObject, AstroObject.Name.HourglassTwins, Sector.Name.HourglassTwins);
                    // Not to be confused with Sector.GetRootSector, this returns the highest sector on the astro object not in the chain
                    // CaveTwin/TowerTwin sectors both have HGT as parent so we want to get rid of that link
                    ao.GetRootSector().SetParentSector(null);
                }

                var owrb = go.GetComponent<OWRigidbody>();

                var im = go.GetComponent<InitialMotion>();

                // By default keep it with the same primary body else update to the new one
                var primary = ao._primaryBody;
                if (!string.IsNullOrEmpty(body.Config.Orbit.primaryBody))
                {
                    // If we can't find the new one we want to try again later (return false)
                    primary = AstroObjectLocator.GetAstroObject(body.Config.Orbit.primaryBody);
                    if (primary == null) return;
                }

                // Just destroy the existing AO after copying everything over
                var newAO = AstroObjectBuilder.Make(go, primary, body, true);
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
                UnityEngine.Object.Destroy(ao);
                Locator.RegisterAstroObject(newAO);
                AstroObjectLocator.RegisterCustomAstroObject(newAO);

                newAO._primaryBody = primary;

                // Since we destroyed the AO we have to replace links to it in other places
                var referenceFrame = newAO.gameObject.GetComponentInChildren<ReferenceFrameVolume>()._referenceFrame;
                if (referenceFrame != null) referenceFrame._attachedAstroObject = newAO;

                // QM and stuff don't have orbit lines
                var orbitLine = go.GetComponentInChildren<OrbitLine>()?.gameObject;
                if (orbitLine != null) UnityEngine.Object.Destroy(orbitLine);

                var isMoon = newAO.GetAstroObjectType() is AstroObject.Type.Moon or AstroObject.Type.Satellite or AstroObject.Type.SpaceStation;
                if (body.Config.Orbit.showOrbitLine) OrbitlineBuilder.Make(go, newAO, isMoon, body.Config);

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
                        if (childAO is NHAstroObject childNHAO && _existingBodyDict.ContainsKey(childNHAO))
                        {
                            // If it's already an NH object we repeat the whole process else it doesn't work idk
                            _nextPassBodies.Add(_existingBodyDict[childNHAO]);
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
                InitialMotionBuilder.SetInitialMotionFromConfig(im, primary, newAO, body.Config.Orbit);

                // Have to register this new AO to the locator
                Locator.RegisterAstroObject(newAO);

                _existingBodyDict.Add(newAO, body);
            }
            catch (Exception ex)
            {
                NHLogger.LogError($"Couldn't update orbit of [{body.Config.name}]:\n{ex}");
                // If it doesn't work here there's no point trying again so we'll still return true
            }

            return;
        }

        public static void UpdatePosition(GameObject go, IOrbitalParameters orbit, AstroObject primaryBody, AstroObject secondaryBody)
        {
            NHLogger.LogVerbose($"Placing [{secondaryBody?.name}] around [{primaryBody?.name}]");

            if (primaryBody != null)
            {
                var primaryGravity = new Gravity(primaryBody.GetGravityVolume());
                var secondaryGravity = new Gravity(secondaryBody.GetGravityVolume());

                var pos = orbit.GetOrbitalParameters(primaryGravity, secondaryGravity).InitialPosition + primaryBody.transform.position;
                SetPositionFromVector(go, pos);
            }
            else
            {
                SetPositionFromVector(go, Vector3.zero);
            }
        }

        public static void SetPositionFromVector(GameObject go, Vector3 position)
        {
            var rb = go.GetAttachedOWRigidbody();
            if (rb)
            {
                var allChildren = CenterOfTheUniverse.s_rigidbodies.Where(x => x.GetOrigParentBody() == rb).ToArray();

                var localPositions = allChildren.Select(x => rb.transform.InverseTransformPoint(x.transform.position)).ToArray();

                go.transform.parent = Locator.GetRootTransform();
                go.transform.position = position;

                for (var i = 0; i < allChildren.Length; i++)
                {
                    if (allChildren[i].TryGetComponent<NomaiInterfaceOrb>(out var orb))
                    {
                        orb.SetOrbPosition(go.transform.TransformPoint(localPositions[i]));
                    }
                    else
                    {
                        allChildren[i].transform.position = go.transform.TransformPoint(localPositions[i]);
                    }
                }
            }
            else
            {
                go.transform.parent = Locator.GetRootTransform();
                go.transform.position = position;
            }

            // Uses the ratio of the interlopers furthest point to what the base game considers the edge of the solar system
            var distanceToCenter = go.transform.position.magnitude / (24000 / 30000f);
            if (distanceToCenter > SolarSystemRadius)
            {
                SolarSystemRadius = distanceToCenter;
            }
        }

        private static void RemoveChildren(GameObject go, NewHorizonsBody body)
        {
            var goPath = go.transform.GetPath();
            var transforms = go.GetComponentsInChildren<Transform>(true);
            foreach (var childPath in body.Config.removeChildren)
            {
                // Multiple children can have the same path so we delete all that match
                var path = $"{goPath}/{childPath}";

                var flag = true;
                foreach (var childObj in transforms.Where(x => x.GetPath() == path))
                {
                    flag = false;
                    // idk why we wait here but we do
                    Delay.FireInNUpdates(() =>
                    {
                        if (childObj != null && childObj.gameObject != null)
                        {
                            childObj.gameObject.SetActive(false);
                        }
                    }, 2);
                }

                if (flag) NHLogger.LogWarning($"Couldn't find \"{childPath}\".");
            }
        }
    }
}
