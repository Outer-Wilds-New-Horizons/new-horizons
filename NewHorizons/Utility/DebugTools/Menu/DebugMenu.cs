using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using OWML.Common;
using OWML.Common.Menus;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility.DebugTools.Menu
{
    class DebugMenu : MonoBehaviour
    {
        private static SubmitAction pauseMenuButton;

        public GUIStyle _editorMenuStyle;
        public GUIStyle _tabBarStyle;
        public GUIStyle _submenuStyle;
        internal Vector2 EditorMenuSize = new Vector2(600, 900);
        bool menuOpen = false;
        static bool openMenuOnPause;

        // Menu params
        internal static IModBehaviour loadedMod = null;
        internal Dictionary<string, PlanetConfig> loadedConfigFiles = new Dictionary<string, PlanetConfig>();
        private bool saveButtonUnlocked = false;
        private Vector2 recentModListScrollPosition = Vector2.zero;

        // Submenus
        private List<DebugSubmenu> submenus;
        private int activeSubmenu = 0;

        private static DebugMenu _instance;
        
        internal static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Formatting = Formatting.Indented,
        };

        private void Awake()
        {
            submenus = new List<DebugSubmenu>()
            {
                new DebugMenuPropPlacer(),
                new DebugMenuShipLogs(),
            };

            submenus.ForEach((submenu) => submenu.OnAwake(this));
        }

        private void Start()
        {
            _instance = this;

            Main.Instance.ModHelper.MenuHelper.PauseMenuManager.PauseMenuOpened += OnOpenMenu;
            Main.Instance.ModHelper.MenuHelper.PauseMenuManager.PauseMenuClosed += OnCloseMenu;
            Main.Instance.OnChangeStarSystem.AddListener(OnChangeStarSystem);

            InitMenu();

            if (loadedMod != null)
            {
                LoadMod(loadedMod);
            }
        }

        public void OnDestroy()
        {
            Main.Instance.ModHelper.MenuHelper.PauseMenuManager.PauseMenuOpened -= OnOpenMenu;
            Main.Instance.ModHelper.MenuHelper.PauseMenuManager.PauseMenuClosed -= OnCloseMenu;
            Main.Instance.OnChangeStarSystem.RemoveListener(OnChangeStarSystem);
        }

        private void OnChangeStarSystem(string _)
        {
            if (saveButtonUnlocked)
            {
                SaveLoadedConfigsForRecentSystem();
                saveButtonUnlocked = false;
            }
        }

        public static void InitializePauseMenu(IPauseMenuManager pauseMenu)
        {
            pauseMenuButton = pauseMenu.MakeSimpleButton(TranslationHandler.GetTranslation("Toggle Dev Tools Menu", TranslationHandler.TextType.UI).ToUpperFixed(), 3, true);
            _instance?.InitMenu();
        }

        public static void UpdatePauseMenuButton()
        {
            pauseMenuButton?.SetButtonVisible(Main.Debug);
        }

        private void OnOpenMenu() { menuOpen = openMenuOnPause; }

        private void ToggleMenu() { menuOpen = !menuOpen; openMenuOnPause = !openMenuOnPause; }

        private void OnCloseMenu() { menuOpen = false; }

        private void OnGUI()
        {
            if (!menuOpen) return;
            if (!Main.Debug) return;

            Vector2 menuPosition = new Vector2(10, 40);

            GUILayout.BeginArea(new Rect(menuPosition.x, menuPosition.y, EditorMenuSize.x, EditorMenuSize.y), _editorMenuStyle);

            // Continue working on existing mod
            GUILayout.Label("Name of your mod");
            if (loadedMod == null)
            {
                recentModListScrollPosition = GUILayout.BeginScrollView(recentModListScrollPosition, GUILayout.Width(EditorMenuSize.x), GUILayout.Height(100));

                foreach (var mod in Main.MountedAddons)
                {
                    if (GUILayout.Button(mod.ModHelper.Manifest.UniqueName))
                    {
                        LoadMod(mod);
                        submenus[activeSubmenu].GainActive();
                    }
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label(loadedMod.ModHelper.Manifest.UniqueName);
            }

            GUILayout.Space(5);

            // Save your work
            if (loadedMod != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(saveButtonUnlocked ? " O " : " | ", GUILayout.ExpandWidth(false)))
                {
                    saveButtonUnlocked = !saveButtonUnlocked;
                }
                GUI.enabled = saveButtonUnlocked;
                if (GUILayout.Button("Update your mod's configs"))
                {
                    SaveLoadedConfigsForRecentSystem();
                    saveButtonUnlocked = false;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
        
                if (GUILayout.Button("Print config changes for your mod"))
                {
                    PrintLoadedConfigChangesForRecentSystem();
                    saveButtonUnlocked = false;
                }
            }

            GUILayout.Space(20);

            // Draw submenu stuff
            if (loadedMod != null)
            {
                GUILayout.BeginHorizontal(_tabBarStyle);
                GUILayout.Space(5);
                for (int i = 0; i < submenus.Count; i++)
                {
                    GUI.enabled = i != activeSubmenu;
                    var style = i == activeSubmenu ? _submenuStyle : _tabBarStyle;
                    if (GUILayout.Button("  " + submenus[i].SubmenuName() + "  ", style, GUILayout.ExpandWidth(false)))
                    {
                        GUI.enabled = true;
                        submenus[activeSubmenu].LoseActive();
                        activeSubmenu = i;
                        submenus[activeSubmenu].GainActive();

                    }
                    GUI.enabled = true;

                    // if (i < submenus.Count-1) GUILayout.Label("|", GUILayout.ExpandWidth(false));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(_submenuStyle);
                GUILayout.Space(10);
                submenus[activeSubmenu].OnGUI(this);
                GUILayout.EndVertical();
            }

            GUILayout.EndArea();
        }

        private void LoadMod(IModBehaviour mod)
        {
            loadedMod = mod;
            submenus.ForEach(submenu => submenu.OnBeginLoadMod(this));

            var folder = loadedMod.ModHelper.Manifest.ModFolderPath;

            var bodiesForThisMod = Main.BodyDict.Values.SelectMany(x => x).Where(x => x.Mod == loadedMod).ToList();
            foreach (var body in bodiesForThisMod)
            {
                if (body.RelativePath == null)
                {
                    NHLogger.LogWarning($"Error loading config for {body.Config.name} in {body.Config.starSystem}");
                    continue;
                }

                loadedConfigFiles[Path.Combine(folder, body.RelativePath)] = body.Config;
                submenus.ForEach(submenu => submenu.LoadConfigFile(this, body.Config));
            }
        }

        private void SaveLoadedConfigsForRecentSystem()
        {
            submenus.ForEach(submenu => submenu.PreSave(this));

            var backupFolderName = $"configBackups\\{DateTime.Now.ToString("yyyyMMddTHHmmss")}\\";

            NHLogger.Log($"Potentially saving {loadedConfigFiles.Keys.Count} files");

            foreach (var filePath in loadedConfigFiles.Keys)
            {
                NHLogger.LogVerbose($"Possibly Saving... {loadedConfigFiles[filePath].name} @ {filePath}");

                if (loadedConfigFiles[filePath].starSystem != Main.Instance.CurrentStarSystem) continue;

                var relativePath = filePath.Replace(loadedMod.ModHelper.Manifest.ModFolderPath, "");

                var json = loadedConfigFiles[filePath].ToSerializedJson();

                try
                {
                    var path = Path.Combine(loadedMod.ModHelper.Manifest.ModFolderPath, backupFolderName, relativePath);
                    NHLogger.LogVerbose($"Backing up... {relativePath} to {path}");
                    var oldPath = Path.Combine(loadedMod.ModHelper.Manifest.ModFolderPath, relativePath);
                    var directoryName = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directoryName);

                    if (File.Exists(oldPath))
                        File.WriteAllBytes(path, File.ReadAllBytes(oldPath));
                    else
                        File.WriteAllText(path, json);
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Failed to save backup file {backupFolderName}{relativePath}:\n{e}");
                }

                try
                {
                    NHLogger.Log($"Saving... {relativePath} to {filePath}");
                    var path = Path.Combine(loadedMod.ModHelper.Manifest.ModFolderPath, relativePath);
                    var directoryName = Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directoryName);

                    File.WriteAllText(path, json);
                }
                catch (Exception e)
                {
                    NHLogger.LogError($"Failed to save file {relativePath}:\n{e}");
                }
            }
        }

        private void PrintLoadedConfigChangesForRecentSystem()
        {
            foreach(DebugSubmenu menu in submenus)
            {
                menu.PrintNewConfigSection(this);
            }
        }

        private void InitMenu()
        {
            if (_editorMenuStyle != null) return;

            UpdatePauseMenuButton();

            // TODO: figure out how to clear this event list so that we don't pile up useless instances of the DebugMenu that can't get garbage collected
            pauseMenuButton.OnSubmitAction += ToggleMenu;

            submenus.ForEach(submenu => submenu.OnInit(this));

            _editorMenuStyle = new GUIStyle
            {
                normal =
                {
                    background = ImageUtilities.MakeSolidColorTexture(1, 1, Color.black)
                }
            };

            _tabBarStyle = new GUIStyle
            {
                normal =
                {
                    background = ImageUtilities.MakeSolidColorTexture(1, 1, new Color(0.2f, 0.2f, 0.2f, 1)),
                    textColor = Color.white
                },
                fontStyle = FontStyle.Bold,
                fontSize = 16
            };

            _submenuStyle = new GUIStyle
            {
                normal =
                {
                    background = ImageUtilities.MakeSolidColorTexture(1, 1, new Color(0.1f, 0.1f, 0.1f, 1)),
                    textColor = Color.white
                },
                fontStyle = FontStyle.Bold,
                fontSize = 16
            };
        }
    }
}
