using NewHorizons.External;
using NewHorizons.External.Configs;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NewHorizons.Utility
{
    [RequireComponent(typeof(DebugRaycaster))]
    [RequireComponent(typeof(DebugPropPlacer))]
    class DebugMenu : MonoBehaviour
    {
        GUIStyle _editorMenuStyle;
        Vector2 EditorMenuSize = new Vector2(600, 900);
        bool menuOpen = false;

        DebugPropPlacer _dpp;
        DebugRaycaster _drc;

        // menu params
        private Vector2 recentPropsScrollPosition = Vector2.zero;
        private HashSet<string> favoriteProps = new HashSet<string>();
        public static readonly char separatorCharacter = '☧'; // since no chars are illegal in game object names, I picked one that's extremely unlikely to be used to be a separator
        private string favoritePropsPlayerPrefKey = "FavoriteProps";

        //private string workingModName = "";
        private IModBehaviour loadedMod = null;
        private Dictionary<string, PlanetConfig> loadedConfigFiles = new Dictionary<string, PlanetConfig>();
        private bool saveButtonUnlocked = false;
        private bool propsHaveBeenLoaded = false;
        private Vector2 recentModListScrollPosition = Vector2.zero;

        private void Awake()
        {  
            _dpp = this.GetRequiredComponent<DebugPropPlacer>();
            _drc = this.GetRequiredComponent<DebugRaycaster>();

            LoadFavoriteProps();
        }

        private void LoadFavoriteProps()
        {
            string favoritePropsPlayerPref = PlayerPrefs.GetString(favoritePropsPlayerPrefKey);

            if (favoritePropsPlayerPref == null || favoritePropsPlayerPref == "") return;

            var favoritePropPaths = favoritePropsPlayerPref.Split(separatorCharacter);
            foreach (string favoriteProp in favoritePropPaths)
            {
                _dpp.RecentlyPlacedProps.Add(favoriteProp);
                this.favoriteProps.Add(favoriteProp);
            }
        }

        private void Update()
        {
            if (!Main.Debug) return;

            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
            {
                menuOpen = !menuOpen;
                if (menuOpen) InitMenu();
            }
        }
        
        private void OnGUI()
        {
            if (!menuOpen) return;
            if (!Main.Debug) return;

            Vector2 menuPosition =  new Vector2(10, 40);
            
            //TODO: add gui for stuff https://github.com/Bwc9876/OW-SaveEditor/blob/master/SaveEditor/SaveEditor.cs
            // https://docs.unity3d.com/ScriptReference/GUI.TextField.html

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
            foreach (string propPath in _dpp.RecentlyPlacedProps)
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
                        loadedMod = mod;

                        propsHaveBeenLoaded = true;
                        var folder = loadedMod.ModHelper.Manifest.ModFolderPath;
                
                        if (System.IO.Directory.Exists(folder + "planets"))
                        {
                            foreach (var file in System.IO.Directory.GetFiles(folder + @"planets\", "*.json", System.IO.SearchOption.AllDirectories))
                            {
                                Logger.Log("READING FROM CONFIG @ " + file);
                                var relativeDirectory = file.Replace(folder, "");
                                var bodyConfig = loadedMod.ModHelper.Storage.Load<PlanetConfig>(relativeDirectory);
                                loadedConfigFiles[file] = bodyConfig;
                                _dpp.FindAndRegisterPropsFromConfig(bodyConfig);
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
            } 
            else
            {
                GUILayout.Label(loadedMod.ModHelper.Manifest.UniqueName);
            }
            // workingModName = GUILayout.TextField(workingModName);
            
            

            //GUI.enabled = !propsHaveBeenLoaded && loadedMod != null;
            //if (GUILayout.Button("Load Detail Props from Configs", GUILayout.ExpandWidth(false)))
            //{
            //    propsHaveBeenLoaded = true;
            //    var folder = loadedMod.ModHelper.Manifest.ModFolderPath;
                
            //    if (System.IO.Directory.Exists(folder + "planets"))
            //    {
            //        foreach (var file in System.IO.Directory.GetFiles(folder + @"planets\", "*.json", System.IO.SearchOption.AllDirectories))
            //        {
            //            Logger.Log("READING FROM CONFIG @ " + file);
            //            var relativeDirectory = file.Replace(folder, "");
            //            var bodyConfig = loadedMod.ModHelper.Storage.Load<PlanetConfig>(relativeDirectory);
            //            loadedConfigFiles[file] = bodyConfig;
            //            _dpp.FindAndRegisterPropsFromConfig(bodyConfig);
            //        }
            //    }
            //}
            //GUI.enabled = true;

            GUILayout.Space(5);

            { 
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(saveButtonUnlocked ? " O " : " | ", GUILayout.ExpandWidth(false)))
                {
                    saveButtonUnlocked = !saveButtonUnlocked;
                }
                GUI.enabled = saveButtonUnlocked;
                if (GUILayout.Button("Update your mod's configs"))
                {
                    UpdateLoadedConfigs();
                    
                    Logger.Log($"(count) Saving {loadedConfigFiles.Keys.Count} files");
                    foreach (var filePath in loadedConfigFiles.Keys)
                    {
                        var relativePath = filePath.Replace(loadedMod.ModHelper.Manifest.ModFolderPath, "");
                        Logger.Log("Saving... " + relativePath + " to " + filePath);
                        loadedMod.ModHelper.Storage.Save(loadedConfigFiles[filePath], relativePath);
                    }
                    saveButtonUnlocked = false;
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Print your mod's updated configs"))
            {
                UpdateLoadedConfigs();
                
                foreach (var filePath in loadedConfigFiles.Keys)
                {
                    Logger.Log("The updated copy of " + filePath);
                    Logger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(loadedConfigFiles[filePath], Newtonsoft.Json.Formatting.Indented));
                }
                //_dpp.PrintConfigs();
            }

            // TODO: field to provide name of mod to load configs from, plus button to load those into the PropPlaecr (make sure not to load more than once, once the button has been pushed, disable it)
            // TODO: add a warning that the button cannot be pushed more than once

            // TODO: put a text field here to print all the configs in
            // TODO: put a button here to save configs to file

            GUILayout.EndArea();
        }

        private void UpdateLoadedConfigs()
        {
            // for each keyvalue in _dpp.GetPropsConfigByBody()
            // find the matching entry loadedConfigFiles
            // entry matches if the value of AstroOBjectLocator.FindBody(key) matches

            var newDetails = _dpp.GetPropsConfigByBody(true);
            
        
            //var allConfigsForMod = Main.Instance.BodyDict[Main.CurrentStarSystem].Where(x => x.Mod == mod).Select(x => x.Config)

            //var allConfigs = Main.BodyDict.Values.SelectMany(x => x).Where(x => x.Mod == loadedMod).Select(x => x.Config);
        
            Logger.Log("updating configs");

            Logger.Log("New Details Counts by planet: " + string.Join(", ", newDetails.Keys.Select(x => x + $" ({newDetails[x].Length})")));

            // TODO: looks like placing the first prop on a given planet in a given session clears out all existing props on that planet
            Dictionary<string, string> planetToConfigPath = new Dictionary<string, string>();

            // Get all configs
            foreach (var filePath in loadedConfigFiles.Keys)
            {
                Logger.Log("potentially updating copy of config at " + filePath);

                if (loadedConfigFiles[filePath].Name == null || AstroObjectLocator.GetAstroObject(loadedConfigFiles[filePath].Name) == null) { Logger.Log("Failed to update copy of config at " + filePath); continue; }      

                var bodyName = loadedConfigFiles[filePath].Name;
                var astroObjectName = AstroObjectLocator.GetAstroObject(bodyName).name;
                if (astroObjectName.EndsWith("_Body")) astroObjectName = astroObjectName.Substring(0, astroObjectName.Length-"_Body".Length);
                var systemName = loadedConfigFiles[filePath].StarSystem;
                var composedName = systemName + separatorCharacter + astroObjectName;

                planetToConfigPath[composedName] = filePath;

                Logger.Log("made composed name from copy of config file for " + composedName + " " + newDetails.ContainsKey(composedName));

                if (!newDetails.ContainsKey(composedName)) continue;

                
                if (loadedConfigFiles[filePath].Props == null)
                {
                    loadedConfigFiles[filePath].Props = new External.PropModule();
                }
                
                loadedConfigFiles[filePath].Props.Details = newDetails[composedName];

                Logger.Log("successfully updated copy of config file for " + composedName);
            }

            // find all new planets that do not yet have config paths
            var planetsThatDoNotHaveConfigFiles = newDetails.Keys.Where(x => !planetToConfigPath.ContainsKey(x)).ToList();
            foreach (var planetAndSystem in planetsThatDoNotHaveConfigFiles)
            {
                Logger.Log("Fabricating new config file for " + planetAndSystem);
                
                var filepath = "planets/" + planetAndSystem + ".json";
                PlanetConfig c = new PlanetConfig(null);
                c.StarSystem = planetAndSystem.Split(separatorCharacter)[0];
                c.Name = planetAndSystem.Split(separatorCharacter)[1];
                c.Props = new PropModule();
                c.Props.Details = newDetails[planetAndSystem];

                // loadedConfigFiles[filepath] = c;
            }
        }

        private void InitMenu()
        {
            if (_editorMenuStyle != null) return;
            
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
