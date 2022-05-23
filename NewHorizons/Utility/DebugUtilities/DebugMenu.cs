#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Handlers;
using Newtonsoft.Json;
using OWML.Common;
using OWML.Common.Menus;
using UnityEngine;

#endregion

namespace NewHorizons.Utility.DebugUtilities
{
    //
    //
    //  TODO: split this into two separate classes "DebugMenu" and "DebugPropPlacerMenu"
    //
    //

    [RequireComponent(typeof(DebugRaycaster))]
    [RequireComponent(typeof(DebugPropPlacer))]
    internal class DebugMenu : MonoBehaviour
    {
        private static IModButton pauseMenuButton;

        private GUIStyle _editorMenuStyle;
        private readonly Vector2 EditorMenuSize = new Vector2(600, 900);
        private bool menuOpen;
        private static bool openMenuOnPause;
        private static bool staticInitialized;

        private DebugPropPlacer _dpp;
        private DebugRaycaster _drc;

        // menu params
        private Vector2 recentPropsScrollPosition = Vector2.zero;
        private readonly HashSet<string> favoriteProps = new HashSet<string>();

        public static readonly char
            separatorCharacter =
                '☧'; // since no chars are illegal in game object names, I picked one that's extremely unlikely to be used to be a separator

        private static readonly string favoritePropsPlayerPrefKey = "FavoriteProps";

        private static IModBehaviour loadedMod;
        private readonly Dictionary<string, PlanetConfig> loadedConfigFiles = new Dictionary<string, PlanetConfig>();
        private bool saveButtonUnlocked;
        private Vector2 recentModListScrollPosition = Vector2.zero;

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        private void Awake()
        {
            _dpp = this.GetRequiredComponent<DebugPropPlacer>();
            _drc = this.GetRequiredComponent<DebugRaycaster>();
            LoadFavoriteProps();
        }

        private void Start()
        {
            if (!staticInitialized)
            {
                staticInitialized = true;

                Main.Instance.ModHelper.Menus.PauseMenu.OnInit += PauseMenuInitHook;
                Main.Instance.ModHelper.Menus.PauseMenu.OnClosed += CloseMenu;
                Main.Instance.ModHelper.Menus.PauseMenu.OnOpened += RestoreMenuOpennessState;

                PauseMenuInitHook();

                Main.Instance.OnChangeStarSystem.AddListener(s => SaveLoadedConfigsForRecentSystem());
            }
            else
            {
                InitMenu();
            }

            if (loadedMod != null) LoadMod(loadedMod);
        }

        private void PauseMenuInitHook()
        {
            pauseMenuButton = Main.Instance.ModHelper.Menus.PauseMenu.OptionsButton.Duplicate(TranslationHandler
                .GetTranslation("Toggle Prop Placer Menu", TranslationHandler.TextType.UI).ToUpper());
            InitMenu();
        }

        public static void UpdatePauseMenuButton()
        {
            if (pauseMenuButton != null)
            {
                if (Main.Debug) pauseMenuButton.Show();
                else pauseMenuButton.Hide();
            }
        }

        private void RestoreMenuOpennessState()
        {
            menuOpen = openMenuOnPause;
        }

        private void ToggleMenu()
        {
            menuOpen = !menuOpen;
            openMenuOnPause = !openMenuOnPause;
        }

        private void CloseMenu()
        {
            menuOpen = false;
        }

        private void LoadFavoriteProps()
        {
            var favoritePropsPlayerPref = PlayerPrefs.GetString(favoritePropsPlayerPrefKey);

            if (favoritePropsPlayerPref == null || favoritePropsPlayerPref == "") return;

            var favoritePropPaths = favoritePropsPlayerPref.Split(separatorCharacter);
            foreach (var favoriteProp in favoritePropPaths)
            {
                DebugPropPlacer.RecentlyPlacedProps.Add(favoriteProp);
                favoriteProps.Add(favoriteProp);
            }
        }

        private void OnGUI()
        {
            if (!menuOpen) return;
            if (!Main.Debug) return;

            var menuPosition = new Vector2(10, 40);

            GUILayout.BeginArea(new Rect(menuPosition.x, menuPosition.y, EditorMenuSize.x, EditorMenuSize.y),
                _editorMenuStyle);

            //
            // DebugPropPlacer
            // 
            GUILayout.Label("Recently placed objects");
            _dpp.SetCurrentObject(GUILayout.TextArea(_dpp.currentObject));

            GUILayout.Space(5);

            // List of recently placed objects
            GUILayout.Label("Recently placed objects");
            recentPropsScrollPosition = GUILayout.BeginScrollView(recentPropsScrollPosition,
                GUILayout.Width(EditorMenuSize.x), GUILayout.Height(100));
            foreach (var propPath in DebugPropPlacer.RecentlyPlacedProps)
            {
                GUILayout.BeginHorizontal();

                var propPathElements = propPath[propPath.Length - 1] == '/'
                    ? propPath.Substring(0, propPath.Length - 1).Split('/')
                    : propPath.Split('/');
                var propName = propPathElements[propPathElements.Length - 1];

                var favoriteButtonIcon = favoriteProps.Contains(propPath) ? "★" : "☆";
                if (GUILayout.Button(favoriteButtonIcon, GUILayout.ExpandWidth(false)))
                {
                    if (favoriteProps.Contains(propPath))
                        favoriteProps.Remove(propPath);
                    else
                        favoriteProps.Add(propPath);

                    var favoritePropsArray = favoriteProps.ToArray();
                    PlayerPrefs.SetString(favoritePropsPlayerPrefKey,
                        string.Join(separatorCharacter + "", favoritePropsArray));
                }

                if (GUILayout.Button(propName)) _dpp.SetCurrentObject(propPath);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.Space(5);

            // continue working on existing mod

            GUILayout.Label("Name of your mod");
            if (loadedMod == null)
            {
                recentModListScrollPosition = GUILayout.BeginScrollView(recentModListScrollPosition,
                    GUILayout.Width(EditorMenuSize.x), GUILayout.Height(100));

                foreach (var mod in Main.MountedAddons)
                    if (GUILayout.Button(mod.ModHelper.Manifest.UniqueName))
                        LoadMod(mod);

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label(loadedMod.ModHelper.Manifest.UniqueName);
            }

            GUILayout.Space(5);

            // save your work

            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(saveButtonUnlocked ? " O " : " | ", GUILayout.ExpandWidth(false)))
                    saveButtonUnlocked = !saveButtonUnlocked;
                GUI.enabled = saveButtonUnlocked;
                if (GUILayout.Button("Update your mod's configs"))
                {
                    SaveLoadedConfigsForRecentSystem();
                    saveButtonUnlocked = false;
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private void LoadMod(IModBehaviour mod)
        {
            loadedMod = mod;
            DebugPropPlacer.active = true;

            var folder = loadedMod.ModHelper.Manifest.ModFolderPath;

            var bodiesForThisMod = Main.BodyDict.Values.SelectMany(x => x).Where(x => x.Mod == loadedMod).ToList();
            foreach (var body in bodiesForThisMod)
            {
                if (body.RelativePath == null)
                    Logger.Log("Error loading config for " + body.Config.name + " in " + body.Config.starSystem);

                loadedConfigFiles[folder + body.RelativePath] = body.Config;
                _dpp.FindAndRegisterPropsFromConfig(body.Config);
            }
        }

        private void SaveLoadedConfigsForRecentSystem()
        {
            UpdateLoadedConfigsForRecentSystem();

            var backupFolderName = "configBackups\\" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "\\";
            Logger.Log($"Potentially saving {loadedConfigFiles.Keys.Count} files");

            foreach (var filePath in loadedConfigFiles.Keys)
            {
                Logger.Log("Possibly Saving... " + loadedConfigFiles[filePath].name + " @ " + filePath);
                if (loadedConfigFiles[filePath].starSystem != Main.Instance.CurrentStarSystem) continue;

                var relativePath = filePath.Replace(loadedMod.ModHelper.Manifest.ModFolderPath, "");

                var json = JsonConvert.SerializeObject(loadedConfigFiles[filePath], jsonSettings);
                // Add the schema line
                json =
                    "{\n\t\"$schema\": \"https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/main/NewHorizons/Schemas/body_schema.json\"," +
                    json.Substring(1);

                try
                {
                    Logger.Log("Saving... " + relativePath + " to " + filePath);
                    var path = loadedMod.ModHelper.Manifest.ModFolderPath + relativePath;
                    var directoryName = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directoryName);

                    File.WriteAllText(path, json);
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to save file " + backupFolderName + relativePath);
                    Logger.LogError(e.Message + "\n" + e.StackTrace);
                }

                try
                {
                    var path = Main.Instance.ModHelper.Manifest.ModFolderPath + backupFolderName + relativePath;
                    var directoryName = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directoryName);

                    File.WriteAllText(path, json);
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to save backup file " + backupFolderName + relativePath);
                    Logger.LogError(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        private void UpdateLoadedConfigsForRecentSystem()
        {
            var newDetails = _dpp.GetPropsConfigByBody();

            Logger.Log("Updating config files. New Details Counts by planet: " +
                       string.Join(", ", newDetails.Keys.Select(x => x + $" ({newDetails[x].Length})")));

            var planetToConfigPath = new Dictionary<string, string>();

            // Get all configs
            foreach (var filePath in loadedConfigFiles.Keys)
            {
                Logger.Log("potentially updating copy of config at " + filePath);

                if (loadedConfigFiles[filePath].starSystem != Main.Instance.CurrentStarSystem) return;
                if (loadedConfigFiles[filePath].name == null ||
                    AstroObjectLocator.GetAstroObject(loadedConfigFiles[filePath].name) == null)
                {
                    Logger.Log("Failed to update copy of config at " + filePath);
                    continue;
                }

                var astroObjectName = DebugPropPlacer.GetAstroObjectName(loadedConfigFiles[filePath].name);
                planetToConfigPath[astroObjectName] = filePath;

                if (!newDetails.ContainsKey(astroObjectName)) continue;

                if (loadedConfigFiles[filePath].Props == null) loadedConfigFiles[filePath].Props = new PropModule();
                loadedConfigFiles[filePath].Props.details = newDetails[astroObjectName];

                Logger.Log("successfully updated copy of config file for " + astroObjectName);
            }

            // find all new planets that do not yet have config paths
            var planetsThatDoNotHaveConfigFiles =
                newDetails.Keys.Where(x => !planetToConfigPath.ContainsKey(x)).ToList();
            foreach (var astroObjectName in planetsThatDoNotHaveConfigFiles)
            {
                Logger.Log("Fabricating new config file for " + astroObjectName);

                var filepath = "planets/" + Main.Instance.CurrentStarSystem + "/" + astroObjectName + ".json";
                var c = new PlanetConfig();
                c.starSystem = Main.Instance.CurrentStarSystem;
                c.name = astroObjectName;
                c.Props = new PropModule();
                c.Props.details = newDetails[astroObjectName];

                loadedConfigFiles[filepath] = c;
            }
        }

        private void InitMenu()
        {
            if (_editorMenuStyle != null) return;

            UpdatePauseMenuButton();

            // TODO: figure out how to clear this event list so that we don't pile up useless instances of the DebugMenu that can't get garbage collected
            pauseMenuButton.OnClick += ToggleMenu;

            _dpp = this.GetRequiredComponent<DebugPropPlacer>();
            _drc = this.GetRequiredComponent<DebugRaycaster>();

            var bgTexture =
                ImageUtilities.MakeSolidColorTexture((int) EditorMenuSize.x, (int) EditorMenuSize.y, Color.black);

            _editorMenuStyle = new GUIStyle
            {
                normal =
                {
                    background = bgTexture
                }
            };
        }
    }
}