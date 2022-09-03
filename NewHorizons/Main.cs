using HarmonyLib;
using NewHorizons.OtherMods.AchievementsPlus;
using NewHorizons.Builder.Atmosphere;
using NewHorizons.Builder.Body;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.Components.Orbital;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.DebugMenu;
using NewHorizons.Utility.DebugUtilities;
using NewHorizons.OtherMods.VoiceActing;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.OtherMods.OWRichPresence;
using NewHorizons.Components.SizeControllers;

namespace NewHorizons
{

    public class Main : ModBehaviour
    {
        public static AssetBundle NHAssetBundle { get; private set; }
        public static Main Instance { get; private set; }

        // Settings
        public static bool Debug { get; private set; }
        public static bool VerboseLogs { get; private set; }
        private static bool _useCustomTitleScreen;
        private static bool _wasConfigured = false;
        private static string _defaultSystemOverride;

        public static Dictionary<string, NewHorizonsSystem> SystemDict = new Dictionary<string, NewHorizonsSystem>();
        public static Dictionary<string, List<NewHorizonsBody>> BodyDict = new Dictionary<string, List<NewHorizonsBody>>();
        public static List<IModBehaviour> MountedAddons = new List<IModBehaviour>();

        public static float SecondsElapsedInLoop = -1;

        public static bool IsSystemReady { get; private set; }
        public static float FurthestOrbit { get; set; } = 50000f;

        public string DefaultStarSystem => SystemDict.ContainsKey(_defaultSystemOverride) ? _defaultSystemOverride : _defaultStarSystem;
        public string CurrentStarSystem => _currentStarSystem;
        public bool IsWarpingFromShip { get; private set; } = false;
        public bool IsWarpingFromVessel { get; private set; } = false;
        public bool WearingSuit { get; private set; } = false;

        public bool IsChangingStarSystem { get; private set; } = false;

        public static bool HasWarpDrive { get; private set; } = false;

        private string _defaultStarSystem = "SolarSystem";
        internal string _currentStarSystem = "SolarSystem";
        private bool _firstLoad = true;
        private ShipWarpController _shipWarpController;

        // API events
        public class StarSystemEvent : UnityEvent<string> { }
        public StarSystemEvent OnChangeStarSystem;
        public StarSystemEvent OnStarSystemLoaded;
        public StarSystemEvent OnPlanetLoaded;

        // For warping to the eye system
        private GameObject _ship;

        public static bool HasDLC { get => EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned; }

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        public override void Configure(IModConfig config)
        {
            Logger.LogVerbose("Settings changed");

            var currentScene = SceneManager.GetActiveScene().name;

            Debug = config.GetSettingsValue<bool>("Debug");
            VerboseLogs = config.GetSettingsValue<bool>("Verbose Logs");

            if (currentScene == "SolarSystem")
            {
                DebugReload.UpdateReloadButton();
                DebugMenu.UpdatePauseMenuButton();
            }

            if (VerboseLogs)          Logger.UpdateLogLevel(Logger.LogType.Verbose);
            else if (Debug)           Logger.UpdateLogLevel(Logger.LogType.Log);
            else                      Logger.UpdateLogLevel(Logger.LogType.Error);

            _defaultSystemOverride = config.GetSettingsValue<string>("Default System Override");

            // Else it doesn't get set idk
            if (currentScene == "TitleScreen" && SystemDict.ContainsKey(_defaultSystemOverride))
            {
                _currentStarSystem = _defaultSystemOverride;
            }

            var wasUsingCustomTitleScreen = _useCustomTitleScreen;
            _useCustomTitleScreen = config.GetSettingsValue<bool>("Custom title screen");
            // Reload the title screen if this was updated on it
            // Don't reload if we haven't configured yet (called on game start)
            if (wasUsingCustomTitleScreen != _useCustomTitleScreen && SceneManager.GetActiveScene().name == "TitleScreen" && _wasConfigured)
            {
                Logger.LogVerbose("Reloading");
                SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
            }

            _wasConfigured = true;
        }

        public static void ResetConfigs(bool resetTranslation = true)
        {
            BodyDict.Clear();
            SystemDict.Clear();

            BodyDict["SolarSystem"] = new List<NewHorizonsBody>();
            BodyDict["EyeOfTheUniverse"] = new List<NewHorizonsBody>(); // Keep this empty tho fr
            SystemDict["SolarSystem"] = new NewHorizonsSystem("SolarSystem", new StarSystemConfig(), "", Instance)
            {
                Config =
                {
                    destroyStockPlanets = false,
                    Vessel = new StarSystemConfig.VesselModule()
                    {
                        coords = new StarSystemConfig.NomaiCoordinates
                        {
                            x = new int[5]{ 0,3,2,1,5 },
                            y = new int[5]{ 4,5,3,2,1 },
                            z = new int[5]{ 4,1,2,5,0 }
                        }
                    }
                }
            };
            SystemDict["EyeOfTheUniverse"] = new NewHorizonsSystem("EyeOfTheUniverse", new StarSystemConfig(), "", Instance)
            {
                Config =
                {
                    destroyStockPlanets = false,
                    factRequiredForWarp = "OPC_EYE_COORDINATES_X1",
                    Vessel = new StarSystemConfig.VesselModule()
                    {
                        coords = new StarSystemConfig.NomaiCoordinates
                        {
                            x = new int[3] { 1, 5, 4 },
                            y = new int[4] { 3, 0, 1, 4 },
                            z = new int[6] { 1, 2, 3, 0, 5, 4 }
                        }
                    }
                }
            };

            if (!resetTranslation) return;
            TranslationHandler.ClearTables();
            TextTranslation.Get().SetLanguage(TextTranslation.Get().GetLanguage());
        }

        public void Start()
        {
            // Patches
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            OnChangeStarSystem = new StarSystemEvent();
            OnStarSystemLoaded = new StarSystemEvent();
            OnPlanetLoaded = new StarSystemEvent();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Instance = this;
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnDeath);

            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
            NHAssetBundle = ModHelper.Assets.LoadBundle("Assets/xen.newhorizons");
            VesselWarpHandler.Initialize();

            ResetConfigs(resetTranslation: false);

            Logger.Log("Begin load of config files...");

            try
            {
                LoadConfigs(this);
            }
            catch (Exception)
            {
                Logger.LogWarning("Couldn't find planets folder");
            }

            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single));
            Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => _firstLoad = false);
            Instance.ModHelper.Menus.PauseMenu.OnInit += DebugReload.InitializePauseMenu;

            AchievementHandler.Init();
            VoiceHandler.Init();
            RichPresenceHandler.Init();
            OnStarSystemLoaded.AddListener(RichPresenceHandler.OnStarSystemLoaded);
            OnChangeStarSystem.AddListener(RichPresenceHandler.OnChangeStarSystem);

            LoadAddonManifest("Assets/addon-manifest.json", this);
        }

        public void OnDestroy()
        {
            Logger.Log($"Destroying NewHorizons");
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnDeath);
            GlobalMessenger.RemoveListener("WakeUp", new Callback(OnWakeUp));

            AchievementHandler.OnDestroy();
        }

        private static void OnWakeUp()
        {
            IsSystemReady = true;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            SearchUtilities.ClearCache();
            ImageUtilities.ClearCache();
            AudioUtilities.ClearCache();
            AssetBundleUtilities.ClearCache();
            IsSystemReady = false;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogVerbose($"Scene Loaded: {scene.name} {mode}");

            var isTitleScreen = scene.name == "TitleScreen";
            var isSolarSystem = scene.name == "SolarSystem";
            var isEyeOfTheUniverse = scene.name == "EyeOfTheUniverse";
            if (isEyeOfTheUniverse) _currentStarSystem = scene.name;

            if (!SystemDict.ContainsKey(_currentStarSystem) || !BodyDict.ContainsKey(_currentStarSystem))
            {
                Logger.LogError($"System \"{_currentStarSystem}\" does not exist!");
                _currentStarSystem = DefaultStarSystem;
            }

            // Set time loop stuff if its enabled and if we're warping to a new place
            if (IsChangingStarSystem && (SystemDict[_currentStarSystem].Config.enableTimeLoop || _currentStarSystem == "SolarSystem") && SecondsElapsedInLoop > 0f)
            {
                TimeLoopUtilities.SetSecondsElapsed(SecondsElapsedInLoop);
                // Prevent the OPC from firing
                var launchController = GameObject.FindObjectOfType<OrbitalProbeLaunchController>();
                if (launchController != null)
                {
                    GlobalMessenger<int>.RemoveListener("StartOfTimeLoop", launchController.OnStartOfTimeLoop);
                    foreach (var fakeDebris in launchController._fakeDebrisBodies)
                    {
                        fakeDebris.gameObject.SetActive(false);
                    }
                    launchController.enabled = false;
                }
                var nomaiProbe = SearchUtilities.Find("NomaiProbe_Body");
                if (nomaiProbe != null) nomaiProbe.gameObject.SetActive(false);
            }

            // Reset this
            SecondsElapsedInLoop = -1;

            IsChangingStarSystem = false;

            if (isTitleScreen && _useCustomTitleScreen)
            {
                TitleSceneHandler.DisplayBodyOnTitleScreen(BodyDict.Values.ToList().SelectMany(x => x).ToList());
                TitleSceneHandler.InitSubtitles();
            }

            if (isSolarSystem && _ship == null)
            {
                var ship = SearchUtilities.Find("Ship_Body", false);
                if (ship != null)
                {
                    _ship = ship.InstantiateInactive();
                    _ship.name = ship.name;
                    _ship.AddComponent<ShipWarpController>().Init();
                    DontDestroyOnLoad(_ship);
                }
            }

            // EOTU fixes
            if (isEyeOfTheUniverse)
            {
                // Create astro objects for eye and vessel because they didn't have them somehow.

                var eyeOfTheUniverse = SearchUtilities.Find("EyeOfTheUniverse_Body");
                var eyeSector = eyeOfTheUniverse.FindChild("Sector_EyeOfTheUniverse").GetComponent<Sector>();
                var eyeAO = eyeOfTheUniverse.AddComponent<EyeAstroObject>();
                var eyeBody = eyeOfTheUniverse.GetAttachedOWRigidbody();
                var eyeMarker = eyeOfTheUniverse.AddComponent<MapMarker>();
                var eyeSphere = eyeSector.GetComponent<SphereShape>();
                eyeSphere.SetLayer(Shape.Layer.Sector);
                eyeAO._owRigidbody = eyeBody;
                eyeAO._rootSector = eyeSector;
                eyeAO._gravityVolume = eyeSector.GetComponentInChildren<GravityVolume>();
                eyeAO._customName = "Eye Of The Universe";
                eyeAO._name = AstroObject.Name.Eye;
                eyeAO._type = AstroObject.Type.None;
                eyeAO.Register();
                eyeMarker._markerType = MapMarker.MarkerType.Sun;
                eyeMarker._labelID = UITextType.LocationEye_Cap;
                Builder.General.RFVolumeBuilder.Make(eyeOfTheUniverse, eyeBody, 400, new External.Modules.ReferenceFrameModule());

                var vessel = SearchUtilities.Find("Vessel_Body");
                var vesselSector = vessel.FindChild("Sector_VesselBridge").GetComponent<Sector>();
                var vesselAO = vessel.AddComponent<EyeAstroObject>();
                var vesselBody = vessel.GetAttachedOWRigidbody();
                var vesselMapMarker = vessel.AddComponent<MapMarker>();
                vesselAO._owRigidbody = vesselBody;
                vesselAO._primaryBody = eyeAO;
                eyeAO._satellite = vesselAO;
                vesselAO._rootSector = vesselSector;
                vesselAO._customName = "Vessel";
                vesselAO._name = AstroObject.Name.CustomString;
                vesselAO._type = AstroObject.Type.SpaceStation;
                vesselAO.Register();
                vesselMapMarker._markerType = MapMarker.MarkerType.Moon;
                vesselMapMarker._labelID = (UITextType)TranslationHandler.AddUI("VESSEL");
                Builder.General.RFVolumeBuilder.Make(vessel, vesselBody, 600, new External.Modules.ReferenceFrameModule { localPosition = new MVector3(0, 0, -207.375f) });

                // Resize vessel sector so that the vessel is fully collidable.
                var vesselSectorTrigger = vesselSector.gameObject.FindChild("SectorTriggerVolume_VesselBridge");
                vesselSectorTrigger.transform.localPosition = new Vector3(0, 0, -207.375f);
                var vesselSectorTriggerBox = vesselSectorTrigger.GetComponent<BoxShape>();
                vesselSectorTriggerBox.size = new Vector3(600, 600, 600);
                vesselSectorTriggerBox.SetLayer(Shape.Layer.Sector);

                // Why were the vessel's lights inside the eye? Let's move them from the eye to vessel.
                var vesselPointlight = eyeSector.gameObject.FindChild("Pointlight_NOM_Vessel");
                vesselPointlight.transform.SetParent(vesselSector.transform, true);
                var vesselSpotlight = eyeSector.gameObject.FindChild("Spotlight_NOM_Vessel");
                vesselSpotlight.transform.SetParent(vesselSector.transform, true);
                var vesselAmbientLight = eyeSector.gameObject.FindChild("AmbientLight_Vessel");
                vesselAmbientLight.transform.SetParent(vesselSector.transform, true);

                // Add sector streaming to vessel just in case some one moves the vessel.
                var vesselStreaming = new GameObject("Sector_Streaming");
                vesselStreaming.transform.SetParent(vesselSector.transform, false);
                var vessel_ss = vesselStreaming.AddComponent<SectorStreaming>();
                vessel_ss._streamingGroup = eyeSector.gameObject.FindChild("StreamingGroup_EYE").GetComponent<StreamingGroup>();
                vessel_ss.SetSector(vesselSector);

                var solarSystemRoot = SearchUtilities.Find("SolarSystemRoot");

                // Disable forest and observatory so that a custom body doesn't accidentally pass through them. (they are 6.5km away from eye)
                // idk why mobius didn't add activation controllers for these

                var forestActivation = solarSystemRoot.AddComponent<EyeStateActivationController>();
                forestActivation._object = eyeSector.gameObject.FindChild("ForestOfGalaxies_Root");
                forestActivation._activeStates = new EyeState[]
                {
                    EyeState.Observatory,
                    EyeState.ZoomOut,
                    EyeState.ForestOfGalaxies,
                    EyeState.ForestIsDark
                };

                var observatoryActivation = solarSystemRoot.AddComponent<EyeStateActivationController>();
                observatoryActivation._object = eyeSector.gameObject.FindChild("Sector_Observatory");
                observatoryActivation._activeStates = new EyeState[]
                {
                    EyeState.IntoTheVortex,
                    EyeState.Observatory,
                    EyeState.ZoomOut
                };

                if (IsWarpingFromShip && _ship != null)
                {
                    var eyeShip = GameObject.Instantiate(_ship);
                    eyeShip.name = "Ship_Body";
                    _shipWarpController = eyeShip.GetComponent<ShipWarpController>();
                    SceneManager.MoveGameObjectToScene(eyeShip, scene);
                    eyeShip.SetActive(true);
                }
            }

            if (isSolarSystem || isEyeOfTheUniverse)
            {
                IsSystemReady = false;

                NewHorizonsData.Load();

                // Some builders have to be reset each loop
                SignalBuilder.Init();
                BrambleDimensionBuilder.Init();
                AstroObjectLocator.Init();
                StreamingHandler.Init();
                AudioTypeHandler.Init();
                RemoteHandler.Init();
                AtmosphereBuilder.Init();
                BrambleNodeBuilder.Init(BodyDict[CurrentStarSystem].Select(x => x.Config).Where(x => x.Bramble?.dimension != null).ToArray());
                StarEvolutionController.Init();

                // Has to go before loading planets else the Discord Rich Presence mod won't show the right text
                LoadTranslations(ModHelper.Manifest.ModFolderPath + "Assets/", this);

                if (isSolarSystem)
                {
                    foreach (var supernovaPlanetEffectController in GameObject.FindObjectsOfType<SupernovaPlanetEffectController>())
                    {
                        SupernovaEffectBuilder.ReplaceVanillaWithNH(supernovaPlanetEffectController);
                    }
                }

                PlanetCreationHandler.Init(BodyDict[CurrentStarSystem]);
                VesselWarpHandler.LoadVessel();
                SystemCreationHandler.LoadSystem(SystemDict[CurrentStarSystem]);

                StarChartHandler.Init(SystemDict.Values.ToArray());

                if (isSolarSystem)
                {
                    // Warp drive
                    HasWarpDrive = StarChartHandler.CanWarp();
                    if (_shipWarpController == null)
                    {
                        _shipWarpController = SearchUtilities.Find("Ship_Body").AddComponent<ShipWarpController>();
                        _shipWarpController.Init();
                    }
                    if (HasWarpDrive == true) EnableWarpDrive();

                    var shouldWarpInFromShip = IsWarpingFromShip && _shipWarpController != null;
                    var shouldWarpInFromVessel = IsWarpingFromVessel && VesselWarpHandler.VesselSpawnPoint != null;
                    Instance.ModHelper.Events.Unity.RunWhen(() => IsSystemReady, () => OnSystemReady(shouldWarpInFromShip, shouldWarpInFromVessel));

                    IsWarpingFromShip = false;
                    IsWarpingFromVessel = false;

                    var map = GameObject.FindObjectOfType<MapController>();
                    if (map != null) map._maxPanDistance = FurthestOrbit * 1.5f;
                    // Fix the map satellite
                    SearchUtilities.Find("HearthianMapSatellite_Body", false).AddComponent<MapSatelliteOrbitFix>();


                    // Sector changes (so that projection pools actually turn off proxies and cull groups on these moons)

                    //Fix attlerock vanilla sector components (they were set to timber hearth's sector)
                    var thm = SearchUtilities.Find("Moon_Body/Sector_THM").GetComponent<Sector>();
                    foreach (var component in thm.GetComponentsInChildren<Component>(true))
                    {
                        if (component is ISectorGroup sectorGroup)
                        {
                            sectorGroup.SetSector(thm);
                        }

                        if (component is SectoredMonoBehaviour behaviour)
                        {
                            behaviour.SetSector(thm);
                        }
                    }
                    var thm_ss_obj = new GameObject("Sector_Streaming");
                    thm_ss_obj.transform.SetParent(thm.transform, false);
                    var thm_ss = thm_ss_obj.AddComponent<SectorStreaming>();
                    thm_ss._streamingGroup = SearchUtilities.Find("TimberHearth_Body/StreamingGroup_TH").GetComponent<StreamingGroup>();
                    thm_ss.SetSector(thm);


                    //Fix hollow's lantern vanilla sector components (they were set to brittle hollow's sector)
                    var vm = SearchUtilities.Find("VolcanicMoon_Body/Sector_VM").GetComponent<Sector>();
                    foreach (var component in vm.GetComponentsInChildren<Component>(true))
                    {
                        if (component is ISectorGroup sectorGroup)
                        {
                            sectorGroup.SetSector(vm);
                        }

                        if (component is SectoredMonoBehaviour behaviour)
                        {
                            behaviour.SetSector(vm);
                        }
                    }
                    var vm_ss_obj = new GameObject("Sector_Streaming");
                    vm_ss_obj.transform.SetParent(vm.transform, false);
                    var vm_ss = vm_ss_obj.AddComponent<SectorStreaming>();
                    vm_ss._streamingGroup = SearchUtilities.Find("BrittleHollow_Body/StreamingGroup_BH").GetComponent<StreamingGroup>();
                    vm_ss.SetSector(vm);

                    //Fix brittle hollow north pole projection platform
                    var northPoleSurface = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_NorthPoleSurface").GetComponent<Sector>();
                    var remoteViewer = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_NorthPoleSurface/Interactables_NorthPoleSurface/LowBuilding/Prefab_NOM_RemoteViewer").GetComponent<NomaiRemoteCameraPlatform>();
                    remoteViewer._visualSector = northPoleSurface;
                }
                else if (isEyeOfTheUniverse)
                {
                    // There is no wake up in eye scene
                    Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
                    {
                        IsSystemReady = true;
                        OnSystemReady(false, false);
                    });
                    IsWarpingFromShip = false;
                    IsWarpingFromVessel = false;
                }

                //Stop starfield from disappearing when there is no lights
                var playerBody = SearchUtilities.Find("Player_Body");
                var playerLight = playerBody.AddComponent<Light>();
                playerLight.innerSpotAngle = 0;
                playerLight.spotAngle = 179;
                playerLight.range = 1;
                playerLight.intensity = 0.001f;

                //Do the same for map
                var solarSystemRoot = SearchUtilities.Find("SolarSystemRoot");
                var ssrLight = solarSystemRoot.AddComponent<Light>();
                ssrLight.innerSpotAngle = 0;
                ssrLight.spotAngle = 179;
                ssrLight.range = Main.FurthestOrbit * (4f/3f);
                ssrLight.intensity = 0.001f;

                var fluid = playerBody.FindChild("PlayerDetector").GetComponent<DynamicFluidDetector>();
                fluid._splashEffects = fluid._splashEffects.AddToArray(new SplashEffect
                {
                    fluidType = FluidVolume.Type.PLASMA,
                    ignoreSphereAligment = false,
                    minImpactSpeed = 15,
                    splashPrefab = SearchUtilities.Find("Probe_Body/ProbeDetector").GetComponent<FluidDetector>()._splashEffects.FirstOrDefault(sfx => sfx.fluidType == FluidVolume.Type.PLASMA).splashPrefab,
                    triggerEvent = SplashEffect.TriggerEvent.OnEntry
                });

                try
                {
                    Logger.Log($"Star system finished loading [{Instance.CurrentStarSystem}]");
                    Instance.OnStarSystemLoaded?.Invoke(Instance.CurrentStarSystem);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Exception thrown when invoking star system loaded event with parameter [{Instance.CurrentStarSystem}]:\n{e}");
                }
            }
            else
            {
                // Reset back to original solar system after going to main menu.
                // If the override is a valid system then we go there
                if (SystemDict.ContainsKey(_defaultSystemOverride))
                {
                    _currentStarSystem = _defaultSystemOverride;
                    IsWarpingFromShip = true; // always do this else sometimes the spawn gets messed up
                }
                else
                {
                    _currentStarSystem = _defaultStarSystem;
                }
            }
        }

        // Had a bunch of separate unity things firing stuff when the system is ready so I moved it all to here
        private void OnSystemReady(bool shouldWarpInFromShip, bool shouldWarpInFromVessel)
        {
            Locator.GetPlayerBody().gameObject.AddComponent<DebugRaycaster>();
            Locator.GetPlayerBody().gameObject.AddComponent<DebugPropPlacer>();
            Locator.GetPlayerBody().gameObject.AddComponent<DebugNomaiTextPlacer>();
            Locator.GetPlayerBody().gameObject.AddComponent<DebugMenu>();
            // DebugArrow.CreateArrow(Locator.GetPlayerBody().gameObject); // This is for NH devs mostly. It shouldn't be active in debug mode for now. Someone should make a dev tools submenu for it though.

            if (shouldWarpInFromShip) _shipWarpController.WarpIn(WearingSuit);
            else if (shouldWarpInFromVessel) VesselWarpHandler.TeleportToVessel();
            else FindObjectOfType<PlayerSpawner>().DebugWarp(SystemDict[_currentStarSystem].SpawnPoint);

            VesselCoordinatePromptHandler.RegisterPrompts(SystemDict.Where(system => system.Value.Config.Vessel?.coords != null).Select(x => x.Value).ToList());
        }

        public void EnableWarpDrive()
        {
            Logger.LogVerbose("Setting up warp drive");
            PlanetCreationHandler.LoadBody(LoadConfig(this, "Assets/WarpDriveConfig.json"));
            HasWarpDrive = true;
        }


        #region Load
        public void LoadConfigs(IModBehaviour mod)
        {
            try
            {
                if (_firstLoad)
                {
                    MountedAddons.Add(mod);
                }
                var folder = mod.ModHelper.Manifest.ModFolderPath;

                // Load systems first so that when we load bodies later we can check for missing ones
                if (Directory.Exists(folder + @"systems\"))
                {
                    foreach (var file in Directory.GetFiles(folder + @"systems\", "*.json?", SearchOption.AllDirectories))
                    {
                        var name = Path.GetFileNameWithoutExtension(file);

                        Logger.LogVerbose($"Loading system {name}");

                        var relativePath = file.Replace(folder, "");
                        var starSystemConfig = mod.ModHelper.Storage.Load<StarSystemConfig>(relativePath);
                        starSystemConfig.Migrate();
                        starSystemConfig.FixCoordinates();

                        if (starSystemConfig.startHere)
                        {
                            // We always want to allow mods to overwrite setting the main SolarSystem as default but not the other way around
                            if (name != "SolarSystem") 
                            {
                                SetDefaultSystem(name);
                                _currentStarSystem = name;
                            }
                        }

                        if (SystemDict.ContainsKey(name))
                        {
                            if (string.IsNullOrEmpty(SystemDict[name].Config.travelAudio) && SystemDict[name].Config.Skybox == null)
                                SystemDict[name].Mod = mod;
                            SystemDict[name].Config.Merge(starSystemConfig);
                        }
                        else
                        {
                            SystemDict[name] = new NewHorizonsSystem(name, starSystemConfig, relativePath, mod);
                        }
                    }
                }
                if (Directory.Exists(folder + "planets"))
                {
                    foreach (var file in Directory.GetFiles(folder + @"planets\", "*.json?", SearchOption.AllDirectories))
                    {
                        var relativeDirectory = file.Replace(folder, "");
                        var body = LoadConfig(mod, relativeDirectory);

                        if (body != null)
                        {
                            // Wanna track the spawn point of each system
                            if (body.Config.Spawn != null) SystemDict[body.Config.starSystem].Spawn = body.Config.Spawn;

                            // Add the new planet to the planet dictionary
                            if (!BodyDict.ContainsKey(body.Config.starSystem)) BodyDict[body.Config.starSystem] = new List<NewHorizonsBody>();
                            BodyDict[body.Config.starSystem].Add(body);
                        }
                    }
                }
                // Has to go before translations for achievements
                if (File.Exists(folder + "addon-manifest.json"))
                {
                    LoadAddonManifest("addon-manifest.json", mod);
                }
                if (Directory.Exists(folder + @"translations\"))
                {
                    LoadTranslations(folder, mod);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        private void LoadAddonManifest(string file, IModBehaviour mod)
        {
            Logger.LogVerbose($"Loading addon manifest for {mod.ModHelper.Manifest.Name}");

            var addonConfig = mod.ModHelper.Storage.Load<AddonConfig>(file);

            if (addonConfig.achievements != null) AchievementHandler.RegisterAddon(addonConfig, mod as ModBehaviour);
            if (addonConfig.credits != null) CreditsHandler.RegisterCredits(mod.ModHelper.Manifest.Name, addonConfig.credits);
        }

        private void LoadTranslations(string folder, IModBehaviour mod)
        {
            var foundFile = false;
            foreach (TextTranslation.Language language in Enum.GetValues(typeof(TextTranslation.Language)))
            {
                if (language == TextTranslation.Language.UNKNOWN || language == TextTranslation.Language.TOTAL) continue;

                var relativeFile = $"translations/{language.ToString().ToLower()}.json";

                if (File.Exists($"{folder}{relativeFile}"))
                {
                    Logger.LogVerbose($"Registering {language} translation from {mod.ModHelper.Manifest.Name} from {relativeFile}");

                    var config = new TranslationConfig($"{folder}{relativeFile}");

                    foundFile = true;

                    TranslationHandler.RegisterTranslation(language, config);

                    if (AchievementHandler.Enabled)
                    {
                        AchievementHandler.RegisterTranslationsFromFiles(mod as ModBehaviour, "translations");
                    }
                }
            }
            if (!foundFile) Logger.LogWarning($"{mod.ModHelper.Manifest.Name} has a folder for translations but none were loaded");
        }

        public NewHorizonsBody LoadConfig(IModBehaviour mod, string relativePath)
        {
            NewHorizonsBody body = null;
            try
            {
                var config = mod.ModHelper.Storage.Load<PlanetConfig>(relativePath);
                if (config == null)
                {
                    Logger.LogError($"Couldn't load {relativePath}. Is your Json formatted correctly?");
                    return null;
                }

                Logger.LogVerbose($"Loaded {config.name}");

                if (!SystemDict.ContainsKey(config.starSystem))
                {
                    // Since we didn't load it earlier there shouldn't be a star system config
                    var starSystemConfig = mod.ModHelper.Storage.Load<StarSystemConfig>($"systems/{config.starSystem}.json");
                    if (starSystemConfig == null) starSystemConfig = new StarSystemConfig();
                    else Logger.LogWarning($"Loaded system config for {config.starSystem}. Why wasn't this loaded earlier?");

                    starSystemConfig.Migrate();
                    starSystemConfig.FixCoordinates();

                    var system = new NewHorizonsSystem(config.starSystem, starSystemConfig, $"", mod);

                    SystemDict.Add(config.starSystem, system);

                    BodyDict.Add(config.starSystem, new List<NewHorizonsBody>());
                }

                // Has to happen after we make sure theres a system config
                config.Validate();
                config.Migrate();

                body = new NewHorizonsBody(config, mod, relativePath);
            }
            catch (Exception e)
            {
                Logger.LogError($"Error encounter when loading {relativePath}:\n{e}");
            }

            return body;
        }

        public void SetDefaultSystem(string defaultSystem)
        {
            _defaultStarSystem = defaultSystem;
        }

        #endregion Load

        #region Change star system
        public void ChangeCurrentStarSystem(string newStarSystem, bool warp = false, bool vessel = false)
        {
            // If we're just on the title screen set the system for later
            if (LoadManager.GetCurrentScene() == OWScene.TitleScreen)
            {
                _currentStarSystem = newStarSystem;

                var newGame = SearchUtilities.Find("TitleMenu/TitleCanvas/TitleLayoutGroup/MainMenuBlock/MainMenuLayoutGroup/Button-NewGame")?.GetComponent<SubmitActionLoadScene>();
                var resumeGame = SearchUtilities.Find("TitleMenu/TitleCanvas/TitleLayoutGroup/MainMenuBlock/MainMenuLayoutGroup/Button-ResumeGame")?.GetComponent<SubmitActionLoadScene>();
                if (newStarSystem == "EyeOfTheUniverse")
                {
                    PlayerData.SaveWarpedToTheEye(TimeLoopUtilities.LOOP_DURATION_IN_SECONDS);
                    if (newGame != null) newGame._sceneToLoad = SubmitActionLoadScene.LoadableScenes.EYE;
                    if (resumeGame != null) resumeGame._sceneToLoad = SubmitActionLoadScene.LoadableScenes.EYE;
                }
                else
                {
                    PlayerData.SaveEyeCompletion();
                    if (newGame != null) newGame._sceneToLoad = SubmitActionLoadScene.LoadableScenes.GAME;
                    if (resumeGame != null) resumeGame._sceneToLoad = SubmitActionLoadScene.LoadableScenes.GAME;
                }

                return;
            }

            if (IsChangingStarSystem) return;

            IsWarpingFromShip = warp;
            IsWarpingFromVessel = vessel;
            OnChangeStarSystem?.Invoke(newStarSystem);

            Logger.Log($"Warping to {newStarSystem}");
            if (warp && _shipWarpController) _shipWarpController.WarpOut();
            IsChangingStarSystem = true;
            WearingSuit = PlayerState.IsWearingSuit();

            OWScene sceneToLoad;

            if (newStarSystem == "EyeOfTheUniverse")
            {
                PlayerData.SaveWarpedToTheEye(TimeLoopUtilities.GetVanillaSecondsRemaining());
                sceneToLoad = OWScene.EyeOfTheUniverse;
            }
            else
            {
                PlayerData.SaveEyeCompletion(); // So that the title screen doesn't keep warping you back to eye

                if (SystemDict[_currentStarSystem].Config.enableTimeLoop) SecondsElapsedInLoop = TimeLoop.GetSecondsElapsed();
                else SecondsElapsedInLoop = -1;

                sceneToLoad = OWScene.SolarSystem;
            }

            _currentStarSystem = newStarSystem;

            // Freeze player inputs
            OWInput.ChangeInputMode(InputMode.None);

            LoadManager.LoadSceneAsync(sceneToLoad, !vessel, LoadManager.FadeType.ToBlack, 0.1f, true);
        }

        void OnDeath(DeathType _)
        {
            // We reset the solar system on death
            if (!IsChangingStarSystem)
            {
                // If the override is a valid system then we go there
                if (SystemDict.ContainsKey(_defaultSystemOverride))
                {
                    _currentStarSystem = _defaultSystemOverride;
                    IsWarpingFromShip = true; // always do this else sometimes the spawn gets messed up
                }
                else
                {
                    _currentStarSystem = _defaultStarSystem;
                }

                IsWarpingFromShip = false;
            }
        }
        #endregion Change star system
    }
}
