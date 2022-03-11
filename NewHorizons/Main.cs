using NewHorizons.Builder.Body;
using NewHorizons.Builder.General;
using NewHorizons.Builder.Orbital;
using NewHorizons.Builder.Props;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Builder.Updater;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.VariableSize;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using Newtonsoft.Json.Linq;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OWML.Common.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.Builder.Atmosphere;
using PacificEngine.OW_CommonResources.Geometry.Orbits;
using NewHorizons.Utility.CommonResources;

namespace NewHorizons
{
    public class Main : ModBehaviour
    {
        public static AssetBundle ShaderBundle;
        public static Main Instance { get; private set; }

        public static bool Debug;
        private static IModButton _reloadButton;

        public static Dictionary<string, NewHorizonsSystem> SystemDict = new Dictionary<string, NewHorizonsSystem>();
        public static Dictionary<string, List<NewHorizonsBody>> BodyDict = new Dictionary<string, List<NewHorizonsBody>>();
        public static Dictionary<string, AssetBundle> AssetBundles = new Dictionary<string, AssetBundle>();
        public static List<IModBehaviour> MountedAddons = new List<IModBehaviour>();

        public static bool IsSystemReady { get; private set; }
        public static float FurthestOrbit { get; set; } = 50000f;

        public string CurrentStarSystem { get { return Instance._currentStarSystem; } }
        public bool IsWarping { get; private set; } = false;
        public bool WearingSuit { get; private set; } = false;

        public static bool HasWarpDrive { get; private set; } = false;

        private string _defaultStarSystem = "SolarSystem";
        private string _currentStarSystem = "SolarSystem";
        private bool _isChangingStarSystem = false;
        private bool _firstLoad = true;
        private ShipWarpController _shipWarpController;

        private GameObject _ship;

        public override object GetApi()
        {
            return new NewHorizonsApi();
        }

        public override void Configure(IModConfig config)
        {
            Debug = config.GetSettingsValue<bool>("Debug");
            UpdateReloadButton();
            string logLevel = config.GetSettingsValue<string>("LogLevel");
            Logger.LogType logType;
            switch (logLevel)
            {
                case "Info":
                    logType = Logger.LogType.Log;
                    break;
                case "Warning":
                    logType = Logger.LogType.Warning;
                    break;
                case "Critical":
                    logType = Logger.LogType.Error;
                    break;
                default:
                    logType = Logger.LogType.Error;
                    break;
            }
            Logger.UpdateLogLevel(logType);
        }

        public void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Instance = this;
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnDeath);
            GlobalMessenger.AddListener("WakeUp", new Callback(OnWakeUp));
            ShaderBundle = Main.Instance.ModHelper.Assets.LoadBundle("AssetBundle/shader");
            BodyDict["SolarSystem"] = new List<NewHorizonsBody>();
            BodyDict["EyeOfTheUniverse"] = new List<NewHorizonsBody>(); // Keep this empty tho fr
            SystemDict["SolarSystem"] = new NewHorizonsSystem("SolarSystem", new StarSystemConfig(null), this);

            Tools.Patches.Apply();
            Tools.WarpDrivePatches.Apply();
            Tools.OWCameraFix.Apply();
            Tools.ShipLogPatches.Apply();
            Tools.TranslationPatches.Apply();

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
            Instance.ModHelper.Menus.PauseMenu.OnInit += InitializePauseMenu;
        }
        
        #region Reloading
        private void InitializePauseMenu()
        {
            _reloadButton = ModHelper.Menus.PauseMenu.OptionsButton.Duplicate(TranslationHandler.GetTranslation("Reload Configs", TranslationHandler.TextType.UI).ToUpper());
            _reloadButton.OnClick += ReloadConfigs;
            UpdateReloadButton();
        }

        private void UpdateReloadButton()
        {
            if (_reloadButton != null)
            {
                if (Debug) _reloadButton.Show();
                else _reloadButton.Hide();
            }
        }

        private void ReloadConfigs()
        {
            BodyDict = new Dictionary<string, List<NewHorizonsBody>>();
            SystemDict = new Dictionary<string, NewHorizonsSystem>();

            BodyDict["SolarSystem"] = new List<NewHorizonsBody>();
            SystemDict["SolarSystem"] = new NewHorizonsSystem("SolarSystem", new StarSystemConfig(null), this);
            foreach (AssetBundle bundle in AssetBundles.Values)
            {
                bundle.Unload(true);
            }
            AssetBundles.Clear();
            
            Logger.Log("Begin reload of config files...", Logger.LogType.Log);

            try
            {
                foreach (IModBehaviour mountedAddon in MountedAddons)
                {
                    LoadConfigs(mountedAddon);
                }
            }
            catch (Exception)
            {
                Logger.LogWarning("Error While Reloading");
            }
            
            ChangeCurrentStarSystem(_currentStarSystem);
        }
        #endregion

        public void OnDestroy()
        {
            Logger.Log($"Destroying NewHorizons");
            SceneManager.sceneLoaded -= OnSceneLoaded;
            GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", OnDeath);
            GlobalMessenger.RemoveListener("WakeUp", new Callback(OnWakeUp));
        }

        private static void OnWakeUp()
        {
            IsSystemReady = true;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.Log($"Scene Loaded: {scene.name} {mode}");

            _isChangingStarSystem = false;

            if (scene.name == "TitleScreen")
            {
                TitleSceneHandler.DisplayBodyOnTitleScreen(BodyDict.Values.ToList().SelectMany(x => x).ToList());
            }

            if(scene.name == "EyeOfTheUniverse" && IsWarping)
            {
                if(_ship != null) SceneManager.MoveGameObjectToScene(_ship, SceneManager.GetActiveScene());
                _ship.transform.position = new Vector3(50, 0, 0);
                _ship.SetActive(true);
            }

            if(scene.name == "SolarSystem")
            {
                if(_ship != null)
                {
                    _ship = GameObject.Find("Ship_Body").InstantiateInactive();
                    DontDestroyOnLoad(_ship);
                }

                IsSystemReady = false;

                HeavenlyBodyBuilder.Reset();
                NewHorizonsData.Load();
                SignalBuilder.Init();
                AstroObjectLocator.RefreshList();
                OWAssetHandler.Init();
                PlanetCreationHandler.Init(BodyDict[CurrentStarSystem]);
                LoadTranslations(ModHelper.Manifest.ModFolderPath + "AssetBundle/", this);

                Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => Locator.GetPlayerBody().gameObject.AddComponent<DebugRaycaster>());

                // Warp drive
                StarChartHandler.Init(SystemDict.Values.ToArray());
                HasWarpDrive = StarChartHandler.CanWarp();
                _shipWarpController = GameObject.Find("Ship_Body").AddComponent<ShipWarpController>();
                _shipWarpController.Init();
                if (HasWarpDrive == true) EnableWarpDrive();

                Logger.Log($"Is the player warping in? {IsWarping}");
                if (IsWarping && _shipWarpController) Instance.ModHelper.Events.Unity.RunWhen(() => IsSystemReady, () => _shipWarpController.WarpIn(WearingSuit));
                else Instance.ModHelper.Events.Unity.RunWhen(() => IsSystemReady, () => FindObjectOfType<PlayerSpawner>().DebugWarp(SystemDict[_currentStarSystem].SpawnPoint));
                IsWarping = false;

                var map = GameObject.FindObjectOfType<MapController>();
                if (map != null) map._maxPanDistance = FurthestOrbit * 1.5f;
            }
            else
            {
                // Reset back to original solar system after going to main menu.
                _currentStarSystem = _defaultStarSystem;
            }
        }

        public void EnableWarpDrive()
        {
            Logger.Log("Setting up warp drive");
            PlanetCreationHandler.LoadBody(LoadConfig(this, "AssetBundle/WarpDriveConfig.json"));
            HasWarpDrive = true;
        }


        #region Load
        public void LoadConfigs(IModBehaviour mod)
        {
            if (_firstLoad)
            {
                MountedAddons.Add(mod);
            }
            var folder = mod.ModHelper.Manifest.ModFolderPath;
            if(Directory.Exists(folder + "planets"))
            {
                foreach (var file in Directory.GetFiles(folder + @"planets\", "*.json", SearchOption.AllDirectories))
                {
                    var relativeDirectory = file.Replace(folder, "");
                    var body = LoadConfig(mod, relativeDirectory);

                    if (body != null)
                    {
                        // Wanna track the spawn point of each system
                        if (body.Config.Spawn != null) SystemDict[body.Config.StarSystem].Spawn = body.Config.Spawn;

                        // Add the new planet to the planet dictionary
                        BodyDict[body.Config.StarSystem].Add(body);
                    }
                }
            }
            if(Directory.Exists(folder + @"translations\"))
            {
                LoadTranslations(folder, mod);
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
                    if (config == null)
                    {
                        Logger.Log($"Found {folder}{relativeFile} but couldn't load it");
                        continue;
                    }

                    foundFile = true;

                    TranslationHandler.RegisterTranslation(language, config);
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
                Logger.Log($"Loaded {config.Name}");
                if (config.Base.CenterOfSolarSystem) config.Orbit.IsStatic = true;
                if (!SystemDict.ContainsKey(config.StarSystem))
                {
                    // See if theres a star system config
                    var starSystemConfig = mod.ModHelper.Storage.Load<StarSystemConfig>($"systems/{config.StarSystem}.json");
                    if (starSystemConfig == null) starSystemConfig = new StarSystemConfig(null);
                    else Logger.Log($"Loaded system config for {config.StarSystem}");

                    // Since we only load stuff the first time we can do this now
                    if (starSystemConfig.startHere)
                    {
                        _defaultStarSystem = config.StarSystem;
                        _currentStarSystem = config.StarSystem;
                    }

                    SystemDict.Add(config.StarSystem, new NewHorizonsSystem(config.StarSystem, starSystemConfig, mod));

                    BodyDict.Add(config.StarSystem, new List<NewHorizonsBody>());
                }

                body = new NewHorizonsBody(config, mod);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load {relativeDirectory}: {e.Message}, is your Json formatted correctly?");
            }

            return body;
        }

        #endregion Load

        #region Change star system
        public void ChangeCurrentStarSystem(string newStarSystem, bool warp = false)
        {
            if (_isChangingStarSystem) return;

            Logger.Log($"Warping to {newStarSystem}");
            if(warp && _shipWarpController) _shipWarpController.WarpOut();
            _currentStarSystem = newStarSystem;
            _isChangingStarSystem = true;
            IsWarping = warp;
            WearingSuit = PlayerState.IsWearingSuit();

            // We kill them so they don't move as much
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);

            if(newStarSystem == "EyeOfTheUniverse")
            {
                PlayerData.SaveWarpedToTheEye(60);
                LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, true, LoadManager.FadeType.ToBlack, 0.1f, true);
            }
            else
            {
                LoadManager.LoadSceneAsync(OWScene.SolarSystem, true, LoadManager.FadeType.ToBlack, 0.1f, true);
            }
        }

        void OnDeath(DeathType _)
        {
            // We reset the solar system on death (unless we just killed the player)
            if (!_isChangingStarSystem)
            {
                _currentStarSystem = _defaultStarSystem;
                IsWarping = false;
            }
        }
        #endregion Change star system
    }

    #region API
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

            var body = new NewHorizonsBody(planetConfig, mod ?? Main.Instance);

            if (!Main.BodyDict.ContainsKey(body.Config.StarSystem)) Main.BodyDict.Add(body.Config.StarSystem, new List<NewHorizonsBody>());
            Main.BodyDict[body.Config.StarSystem].Add(body);
        }

        public void LoadConfigs(IModBehaviour mod)
        {
            Main.Instance.LoadConfigs(mod);
        }

        public GameObject GetPlanet(string name)
        {
            return Main.BodyDict.Values.SelectMany(x => x)?.ToList()?.FirstOrDefault(x => x.Config.Name == name)?.Object;
        }

        public string GetCurrentStarSystem()
        {
            return Main.Instance.CurrentStarSystem;
        }
    }
    #endregion API
}
