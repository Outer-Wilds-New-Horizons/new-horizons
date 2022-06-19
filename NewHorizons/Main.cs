using HarmonyLib;
using NewHorizons.AchievementsPlus;
using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.DebugMenu;
using NewHorizons.Utility.DebugUtilities;
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

namespace NewHorizons
{

    public class Main : ModBehaviour
    {
        public static AssetBundle NHAssetBundle { get; private set; }
        public static Main Instance { get; private set; }

        // Settings
        public static bool Debug { get; private set; }
        private static bool _useCustomTitleScreen;
        private static bool _wasConfigured = false;
        private static string _defaultSystemOverride;

        public static Dictionary<string, NewHorizonsSystem> SystemDict = new Dictionary<string, NewHorizonsSystem>();
        public static Dictionary<string, List<NewHorizonsBody>> BodyDict = new Dictionary<string, List<NewHorizonsBody>>();
        public static List<IModBehaviour> MountedAddons = new List<IModBehaviour>();

        public static float SecondsLeftInLoop = -1;

        public static bool IsSystemReady { get; private set; }
        public static float FurthestOrbit { get; set; } = 50000f;

        public string CurrentStarSystem { get { return Instance._currentStarSystem; } }
        public bool IsWarpingFromShip { get; private set; } = false;
        public bool IsWarpingFromVessel { get; private set; } = false;
        public bool WearingSuit { get; private set; } = false;

        public bool IsChangingStarSystem { get; private set; } = false;

        public static bool HasWarpDrive { get; private set; } = false;

        private string _defaultStarSystem = "SolarSystem";
        private string _currentStarSystem = "SolarSystem";
        private bool _firstLoad = true;
        private ShipWarpController _shipWarpController;

        // Vessel
        private SpawnPoint _vesselSpawnPoint;

        // API events
        public class StarSystemEvent : UnityEvent<string> { }
        public StarSystemEvent OnChangeStarSystem;
        public StarSystemEvent OnStarSystemLoaded;

        // For warping to the eye system
        private GameObject _ship;

        public static bool HasDLC { get => EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned; }

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        public override void Configure(IModConfig config)
        {
            Logger.Log("Settings changed");

            Debug = config.GetSettingsValue<bool>("Debug");
            DebugReload.UpdateReloadButton();
            DebugMenu.UpdatePauseMenuButton();
            Logger.UpdateLogLevel(Debug ? Logger.LogType.Log : Logger.LogType.Error);

            _defaultSystemOverride = config.GetSettingsValue<string>("Default System Override");

            // Else it doesn't get set idk
            if (SceneManager.GetActiveScene().name == "TitleScreen") _currentStarSystem = _defaultSystemOverride;

            var wasUsingCustomTitleScreen = _useCustomTitleScreen;
            _useCustomTitleScreen = config.GetSettingsValue<bool>("Custom title screen");
            // Reload the title screen if this was updated on it
            // Don't reload if we haven't configured yet (called on game start)
            if (wasUsingCustomTitleScreen != _useCustomTitleScreen && SceneManager.GetActiveScene().name == "TitleScreen" && _wasConfigured)
            {
                Logger.Log("Reloading");
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
            SystemDict["SolarSystem"] = new NewHorizonsSystem("SolarSystem", new StarSystemConfig(), Instance)
            {
                Config =
                {
                    destroyStockPlanets = false,
                    coords = new StarSystemConfig.NomaiCoordinates
                    {
                        x = new int[5]{ 0,3,2,1,5 },
                        y = new int[5]{ 4,5,3,2,1 },
                        z = new int[5]{ 4,1,2,5,0 }
                    }
                }
            };
            SystemDict["EyeOfTheUniverse"] = new NewHorizonsSystem("EyeOfTheUniverse", new StarSystemConfig(), Instance)
            {
                Config =
                {
                    coords = new StarSystemConfig.NomaiCoordinates
                    {
                        x = new int[3]{ 1,5,4 },
                        y = new int[4]{ 3,0,1,4 },
                        z = new int[6]{ 1,2,3,0,5,4 }
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

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Instance = this;
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnDeath);

            GlobalMessenger.AddListener("WakeUp", OnWakeUp);
            NHAssetBundle = ModHelper.Assets.LoadBundle("Assets/xen.newhorizons");
            VesselWarpHandler.Initialize();

            ResetConfigs(resetTranslation: false);

            Logger.Log("Begin load of config files...", Logger.LogType.Log);

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
            try
            {
                Logger.Log($"Star system loaded [{Instance.CurrentStarSystem}]");
                Instance.OnStarSystemLoaded?.Invoke(Instance.CurrentStarSystem);
            }
            catch (Exception e)
            {
                Logger.LogError($"Exception thrown when invoking star system loaded event with parameter [{Instance.CurrentStarSystem}] : {e.GetType().FullName} {e.Message} {e.StackTrace}");
            }
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
            Logger.Log($"Scene Loaded: {scene.name} {mode}");

            // Set time loop stuff if its enabled and if we're warping to a new place
            if (IsChangingStarSystem && (SystemDict[_currentStarSystem].Config.enableTimeLoop || _currentStarSystem == "SolarSystem") && SecondsLeftInLoop > 0f)
            {
                TimeLoop.SetSecondsRemaining(SecondsLeftInLoop);
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
            SecondsLeftInLoop = -1;

            IsChangingStarSystem = false;

            if (scene.name == "TitleScreen" && _useCustomTitleScreen)
            {
                TitleSceneHandler.DisplayBodyOnTitleScreen(BodyDict.Values.ToList().SelectMany(x => x).ToList());
                TitleSceneHandler.InitSubtitles();
            }

            if (scene.name == "EyeOfTheUniverse" && IsWarpingFromShip)
            {
                if (_ship != null) SceneManager.MoveGameObjectToScene(_ship, SceneManager.GetActiveScene());
                _ship.transform.position = new Vector3(50, 0, 0);
                _ship.SetActive(true);
            }

            if (scene.name == "SolarSystem")
            {
                foreach (var body in GameObject.FindObjectsOfType<AstroObject>())
                {
                    Logger.Log($"{body.name}, {body.transform.rotation}");
                }

                if (_ship != null)
                {
                    _ship = SearchUtilities.Find("Ship_Body").InstantiateInactive();
                    DontDestroyOnLoad(_ship);
                }

                IsSystemReady = false;

                NewHorizonsData.Load();
                SignalBuilder.Init();
                AstroObjectLocator.Init();
                OWAssetHandler.Init();
                PlanetCreationHandler.Init(BodyDict[CurrentStarSystem]);
                if (IsWarpingFromVessel)
                    _vesselSpawnPoint = CurrentStarSystem == "SolarSystem" ? VesselWarpHandler.UpdateVessel() : VesselWarpHandler.CreateVessel();
                else
                    _vesselSpawnPoint = SearchUtilities.Find("DB_VesselDimension_Body/Sector_VesselDimension").GetComponentInChildren<SpawnPoint>();
                SystemCreationHandler.LoadSystem(SystemDict[CurrentStarSystem]);
                LoadTranslations(ModHelper.Manifest.ModFolderPath + "Assets/", this);

                // Warp drive
                StarChartHandler.Init(SystemDict.Values.ToArray());
                HasWarpDrive = StarChartHandler.CanWarp();
                _shipWarpController = SearchUtilities.Find("Ship_Body").AddComponent<ShipWarpController>();
                _shipWarpController.Init();
                if (HasWarpDrive == true) EnableWarpDrive();

                var shouldWarpInFromShip = IsWarpingFromShip && _shipWarpController != null;
                var shouldWarpInFromVessel = IsWarpingFromVessel && _vesselSpawnPoint != null;
                Instance.ModHelper.Events.Unity.RunWhen(() => IsSystemReady, () => OnSystemReady(shouldWarpInFromShip, shouldWarpInFromVessel));

                IsWarpingFromShip = false;
                IsWarpingFromVessel = false;

                var map = GameObject.FindObjectOfType<MapController>();
                if (map != null) map._maxPanDistance = FurthestOrbit * 1.5f;

                // Fix the map satellite
                SearchUtilities.Find("HearthianMapSatellite_Body", false).AddComponent<MapSatelliteOrbitFix>();
            }
            else
            {
                // Reset back to original solar system after going to main menu.
                // If the override is a valid system then we go there
                if (SystemDict.Keys.Contains(_defaultSystemOverride))
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
            else if (shouldWarpInFromVessel)
            {
                FindObjectOfType<PlayerSpawner>().DebugWarp(_vesselSpawnPoint);
                Builder.General.SpawnPointBuilder.SuitUp();
            }
            else FindObjectOfType<PlayerSpawner>().DebugWarp(SystemDict[_currentStarSystem].SpawnPoint);
        }

        public void EnableWarpDrive()
        {
            Logger.Log("Setting up warp drive");
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

                        Logger.Log($"Loading system {name}");

                        var relativePath = file.Replace(folder, "");
                        var starSystemConfig = mod.ModHelper.Storage.Load<StarSystemConfig>(relativePath);
                        starSystemConfig.FixCoordinates();

                        if (starSystemConfig.startHere)
                        {
                            // We always want to allow mods to overwrite setting the main SolarSystem as default but not the other way around
                            if (name != "SolarSystem") SetDefaultSystem(name);
                        }

                        if (SystemDict.ContainsKey(name))
                        {
                            SystemDict[name].Config.Merge(starSystemConfig);
                        }
                        else
                        {
                            SystemDict[name] = new NewHorizonsSystem(name, starSystemConfig, mod);
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
                    var addonConfig = mod.ModHelper.Storage.Load<AddonConfig>("addon-manifest.json");

                    AchievementHandler.RegisterAddon(addonConfig, mod as ModBehaviour);
                }
                if (Directory.Exists(folder + @"translations\"))
                {
                    LoadTranslations(folder, mod);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError($"{ex.Message}, {ex.StackTrace}");
            }
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
                    Logger.Log($"Registering {language} translation from {mod.ModHelper.Manifest.Name} from {relativeFile}");

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

        public NewHorizonsBody LoadConfig(IModBehaviour mod, string relativeDirectory)
        {
            NewHorizonsBody body = null;
            try
            {
                var config = mod.ModHelper.Storage.Load<PlanetConfig>(relativeDirectory);
                // var config = JsonConvert.DeserializeObject<PlanetConfig>(File.ReadAllText($"{mod.ModHelper.Manifest.ModFolderPath}/{relativeDirectory}"));

                Logger.Log($"Loaded {config.name}");

                if (!SystemDict.ContainsKey(config.starSystem))
                {
                    // Since we didn't load it earlier there shouldn't be a star system config
                    var starSystemConfig = mod.ModHelper.Storage.Load<StarSystemConfig>($"systems/{config.starSystem}.json");
                    if (starSystemConfig == null) starSystemConfig = new StarSystemConfig();
                    else Logger.LogWarning($"Loaded system config for {config.starSystem}. Why wasn't this loaded earlier?");

                    starSystemConfig.FixCoordinates();

                    var system = new NewHorizonsSystem(config.starSystem, starSystemConfig, mod);

                    SystemDict.Add(config.starSystem, system);

                    BodyDict.Add(config.starSystem, new List<NewHorizonsBody>());
                }

                // Has to happen after we make sure theres a system config
                config.MigrateAndValidate();

                body = new NewHorizonsBody(config, mod, relativeDirectory);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load {relativeDirectory}: {e.Message} {e.StackTrace}, is your Json formatted correctly?");
            }

            return body;
        }

        public void SetDefaultSystem(string defaultSystem)
        {
            _defaultStarSystem = defaultSystem;
            _currentStarSystem = defaultSystem;
        }

        #endregion Load

        #region Change star system
        public void ChangeCurrentStarSystem(string newStarSystem, bool warp = false, bool vessel = false)
        {
            if (IsChangingStarSystem) return;

            IsWarpingFromShip = warp;
            IsWarpingFromVessel = vessel;
            OnChangeStarSystem?.Invoke(newStarSystem);

            Logger.Log($"Warping to {newStarSystem}");
            if (warp && _shipWarpController) _shipWarpController.WarpOut();
            IsChangingStarSystem = true;
            WearingSuit = PlayerState.IsWearingSuit();

            // We kill them so they don't move as much
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);

            OWScene sceneToLoad;

            if (newStarSystem == "EyeOfTheUniverse")
            {
                PlayerData.SaveWarpedToTheEye(TimeLoop.GetSecondsRemaining());
                sceneToLoad = OWScene.EyeOfTheUniverse;
            }
            else
            {
                if (SystemDict[_currentStarSystem].Config.enableTimeLoop) SecondsLeftInLoop = TimeLoop.GetSecondsRemaining();
                else SecondsLeftInLoop = -1;

                sceneToLoad = OWScene.SolarSystem;
            }

            _currentStarSystem = newStarSystem;

            LoadManager.LoadSceneAsync(sceneToLoad, !vessel, LoadManager.FadeType.ToBlack, 0.1f, true);
        }

        void OnDeath(DeathType _)
        {
            // We reset the solar system on death (unless we just killed the player)
            if (!IsChangingStarSystem)
            {
                // If the override is a valid system then we go there
                if (SystemDict.Keys.Contains(_defaultSystemOverride))
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
