using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using OWML.Common;
using OWML.Common.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NewHorizons.Utility
{

    //
    //
    //  TODO: split this into two separate classes "DebugMenu" and "DebugPropPlacerMenu"
    //
    //

    [RequireComponent(typeof(DebugRaycaster))]
    [RequireComponent(typeof(DebugPropPlacer))]
    class DebugMenu : MonoBehaviour
    {
        private static IModButton pauseMenuButton;

        GUIStyle _editorMenuStyle;
        Vector2 EditorMenuSize = new Vector2(600, 900);
        bool menuOpen = false;
        static bool openMenuOnPause;
        static bool staticInitialized;

        DebugPropPlacer _dpp;
        DebugRaycaster _drc;

        // menu params
        private Vector2 recentPropsScrollPosition = Vector2.zero;
        private HashSet<string> favoriteProps = new HashSet<string>();
        public static readonly char separatorCharacter = '☧'; // since no chars are illegal in game object names, I picked one that's extremely unlikely to be used to be a separator
        private static readonly string favoritePropsPlayerPrefKey = "FavoriteProps";

        //private string workingModName = "";
        private static IModBehaviour loadedMod = null;
        private Dictionary<string, PlanetConfig> loadedConfigFiles = new Dictionary<string, PlanetConfig>();
        private bool saveButtonUnlocked = false;
        private Vector2 recentModListScrollPosition = Vector2.zero;


        private void Awake()
        {  
            _dpp = this.GetRequiredComponent<DebugPropPlacer>();
            _drc = this.GetRequiredComponent<DebugRaycaster>();
            LoadFavoriteProps();
        }
        private void Start() 
        { 
            if (!Main.Debug) return;
            if (!staticInitialized)
            {
                staticInitialized = true;

                Main.Instance.ModHelper.Menus.PauseMenu.OnInit += PauseMenuInitHook;
                Main.Instance.ModHelper.Menus.PauseMenu.OnClosed += CloseMenu;
                Main.Instance.ModHelper.Menus.PauseMenu.OnOpened += RestoreMenuOpennessState;
            
                PauseMenuInitHook();

                Main.Instance.OnChangeStarSystem.AddListener((string s) => SaveLoadedConfigsForRecentSystem());
            } 
            else
            {
                InitMenu();
            }

            if (loadedMod != null)
            {
                LoadMod(loadedMod);
            }
        }

        private void PauseMenuInitHook()
        {
            pauseMenuButton = Main.Instance.ModHelper.Menus.PauseMenu.OptionsButton.Duplicate("Toggle Prop Placer Menu".ToUpper());
            InitMenu();
        }
        private void RestoreMenuOpennessState() { menuOpen = openMenuOnPause; }
        private void ToggleMenu() { menuOpen = !menuOpen; openMenuOnPause = !openMenuOnPause; }

        private void CloseMenu() { menuOpen = false; }

        private void LoadFavoriteProps()
        {
            string favoritePropsPlayerPref = PlayerPrefs.GetString(favoritePropsPlayerPrefKey);

            if (favoritePropsPlayerPref == null || favoritePropsPlayerPref == "") return;

            var favoritePropPaths = favoritePropsPlayerPref.Split(separatorCharacter);
            foreach (string favoriteProp in favoritePropPaths)
            {
                DebugPropPlacer.RecentlyPlacedProps.Add(favoriteProp);
                this.favoriteProps.Add(favoriteProp);
            }
        }
        
        private void OnGUI()
        {
            if (!menuOpen) return;
            if (!Main.Debug) return;

            Vector2 menuPosition =  new Vector2(10, 40);

            GUILayout.BeginArea(new Rect(menuPosition.x, menuPosition.y, EditorMenuSize.x, EditorMenuSize.y), _editorMenuStyle);

            //
            // DebugPropPlacer
            // 
            GUILayout.Label("Recently placed objects");
            _dpp.SetCurrentObject(GUILayout.TextArea(_dpp.currentObject));
            
            GUILayout.Space(5);

            // List of recently placed objects
            GUILayout.Label("Recently placed objects");
            recentPropsScrollPosition  = GUILayout.BeginScrollView(recentPropsScrollPosition, GUILayout.Width(EditorMenuSize.x), GUILayout.Height(100));
            foreach (string propPath in DebugPropPlacer.RecentlyPlacedProps)
            {
                GUILayout.BeginHorizontal();

                var propPathElements = propPath.Split('/');
                string propName = propPathElements[propPathElements.Length-1];

                string favoriteButtonIcon = favoriteProps.Contains(propPath) ? "★" : "☆";
                if (GUILayout.Button(favoriteButtonIcon, GUILayout.ExpandWidth(false)))
                {
                    if (favoriteProps.Contains(propPath))
                    {
                        favoriteProps.Remove(propPath);
                    }
                    else
                    {
                        favoriteProps.Add(propPath);
                    }

                    string[] favoritePropsArray = favoriteProps.ToArray<string>();
                    PlayerPrefs.SetString(favoritePropsPlayerPrefKey, string.Join(separatorCharacter+"", favoritePropsArray));
                }
                
                if (GUILayout.Button(propName))
                {
                    _dpp.SetCurrentObject(propPath);
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            
            GUILayout.Space(5);

            // continue working on existing mod

            GUILayout.Label("Name of your mod");
            if (loadedMod == null)
            {
                recentModListScrollPosition  = GUILayout.BeginScrollView(recentModListScrollPosition, GUILayout.Width(EditorMenuSize.x), GUILayout.Height(100));

                foreach (var mod in Main.MountedAddons)
                {
                    if (GUILayout.Button(mod.ModHelper.Manifest.UniqueName))
                    {
                        LoadMod(mod);
                    }
                }

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
            }

            GUILayout.EndArea();
        }

        private void LoadMod(IModBehaviour mod)
        {
            loadedMod = mod;
            DebugPropPlacer.active = true;

            var folder = loadedMod.ModHelper.Manifest.ModFolderPath;
                
            List<NewHorizonsBody> bodiesForThisMod = Main.BodyDict.Values.SelectMany(x => x).Where(x => x.Mod == loadedMod).ToList();
            foreach (NewHorizonsBody body in bodiesForThisMod)
            {
                if (body.RelativePath == null)
                {
                    Logger.Log("Error loading config for " + body.Config.Name + " in " + body.Config.StarSystem);
                }

                loadedConfigFiles[folder + body.RelativePath] = (body.Config as PlanetConfig);
                _dpp.FindAndRegisterPropsFromConfig(body.Config);
            }
        }

        private void SaveLoadedConfigsForRecentSystem()
        {
            UpdateLoadedConfigsForRecentSystem();
                    
            string backupFolderName = "configBackups\\" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "\\";
            Logger.Log($"Potentially saving {loadedConfigFiles.Keys.Count} files");

            foreach (var filePath in loadedConfigFiles.Keys)
            {
                Logger.Log("Possibly Saving... " + loadedConfigFiles[filePath].Name + " @ " + filePath);
                if (loadedConfigFiles[filePath].StarSystem != Main.Instance.CurrentStarSystem) continue;

                var relativePath = filePath.Replace(loadedMod.ModHelper.Manifest.ModFolderPath, "");
                
                try 
                { 
                    Logger.Log("Saving... " + relativePath + " to " + filePath);
                    var directoryName = System.IO.Path.GetDirectoryName(loadedMod.ModHelper.Manifest.ModFolderPath + relativePath);
                    System.IO.Directory.CreateDirectory(directoryName);
                    
                    loadedMod.ModHelper.Storage.Save(loadedConfigFiles[filePath], relativePath); 
                } 
                catch (Exception e) { Logger.LogError("Failed to save file " + backupFolderName+relativePath); Logger.LogError(e.Message + "\n" + e.StackTrace); }

                try 
                { 
                    var directoryName = System.IO.Path.GetDirectoryName(Main.Instance.ModHelper.Manifest.ModFolderPath + backupFolderName + relativePath);
                    System.IO.Directory.CreateDirectory(directoryName);
                    
                    Main.Instance.ModHelper.Storage.Save(loadedConfigFiles[filePath], backupFolderName+relativePath); 
                } 
                catch (Exception e) { Logger.LogError("Failed to save backup file " + backupFolderName+relativePath); Logger.LogError(e.Message + "\n" + e.StackTrace); }
            }
        }

        private void UpdateLoadedConfigsForRecentSystem()
        {
            var newDetails = _dpp.GetPropsConfigByBody();
        
            Logger.Log("Updating config files. New Details Counts by planet: " + string.Join(", ", newDetails.Keys.Select(x => x + $" ({newDetails[x].Length})")));

            Dictionary<string, string> planetToConfigPath = new Dictionary<string, string>();

            // Get all configs
            foreach (var filePath in loadedConfigFiles.Keys)
            {
                Logger.Log("potentially updating copy of config at " + filePath);

                if (loadedConfigFiles[filePath].StarSystem != Main.Instance.CurrentStarSystem) return;
                if (loadedConfigFiles[filePath].Name == null || AstroObjectLocator.GetAstroObject(loadedConfigFiles[filePath].Name) == null) { Logger.Log("Failed to update copy of config at " + filePath); continue; }      

                var astroObjectName = DebugPropPlacer.GetAstroObjectName(loadedConfigFiles[filePath].Name);
                planetToConfigPath[astroObjectName] = filePath;

                if (!newDetails.ContainsKey(astroObjectName)) continue;

                if (loadedConfigFiles[filePath].Props == null) loadedConfigFiles[filePath].Props = new External.Modules.PropModule();
                loadedConfigFiles[filePath].Props.Details = newDetails[astroObjectName];

                Logger.Log("successfully updated copy of config file for " + astroObjectName);
            }

            // find all new planets that do not yet have config paths
            var planetsThatDoNotHaveConfigFiles = newDetails.Keys.Where(x => !planetToConfigPath.ContainsKey(x)).ToList();
            foreach (var astroObjectName in planetsThatDoNotHaveConfigFiles)
            {
                Logger.Log("Fabricating new config file for " + astroObjectName);
                
                var filepath = "planets/" + Main.Instance.CurrentStarSystem + "/" + astroObjectName + ".json";
                PlanetConfig c = new PlanetConfig();
                c.StarSystem = Main.Instance.CurrentStarSystem;
                c.Name = astroObjectName;
                c.Props = new PropModule();
                c.Props.Details = newDetails[astroObjectName];

                loadedConfigFiles[filepath] = c;
            }
        }

        private void InitMenu()
        {
            if (_editorMenuStyle != null) return;
            
            // TODO: figure out how to clear this event list so that we don't pile up useless instances of the DebugMenu that can't get garbage collected
            pauseMenuButton.OnClick += ToggleMenu;

            _dpp = this.GetRequiredComponent<DebugPropPlacer>();
            _drc = this.GetRequiredComponent<DebugRaycaster>();


            Texture2D bgTexture = MakeTexture((int)EditorMenuSize.x, (int)EditorMenuSize.y, Color.black);

            _editorMenuStyle = new GUIStyle
            {
                normal =
                {
                    background = bgTexture
                }
            };
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width*height];
 
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
 
            Texture2D newTexture = new Texture2D(width, height);
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return newTexture;
        }
    }
}
