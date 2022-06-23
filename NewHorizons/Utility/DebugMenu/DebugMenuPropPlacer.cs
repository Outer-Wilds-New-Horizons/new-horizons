using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility.DebugUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility.DebugMenu
{
    class DebugMenuPropPlacer : DebugSubmenu
    {
        private HashSet<string> favoriteProps = new HashSet<string>();
        private List<string> favoritePropsList = new List<string>();
        private List<string> propsLoadedFromConfig = new List<string>();
        public static readonly char separatorCharacter = '☧'; // since no chars are illegal in game object names, I picked one that's extremely unlikely to be used to be a separator
        private static readonly string favoritePropsPlayerPrefKey = "FavoriteProps";

        internal DebugPropPlacer _dpp;
        internal DebugRaycaster _drc;

        // menu params
        private Vector2 favoritePropsScrollPosition = Vector2.zero;
        private bool favoritePropsCollapsed = false;
        private Vector2 recentPropsScrollPosition = Vector2.zero;
        private bool recentPropsCollapsed = false;
        private Vector2 configPropsScrollPosition = Vector2.zero;
        private bool configPropsCollapsed = true;

        internal override string SubmenuName()
        {
            return "Prop Placer";
        }

        internal override void OnInit(DebugMenu menu)
        {
            _dpp = menu.GetComponent<DebugPropPlacer>();
            _drc = menu.GetComponent<DebugRaycaster>();
        }

        internal override void OnAwake(DebugMenu menu)
        {
            _dpp = menu.GetComponent<DebugPropPlacer>();
            _drc = menu.GetComponent<DebugRaycaster>();
            LoadFavoriteProps();
        }

        internal override void OnBeginLoadMod(DebugMenu debugMenu)
        {
            
        }

        internal override void GainActive()
        {
            DebugPropPlacer.active = true;
        }

        internal override void LoseActive()
        {
            DebugPropPlacer.active = false;
        }
        
        internal override void LoadConfigFile(DebugMenu menu, PlanetConfig config)
        {
            _dpp.FindAndRegisterPropsFromConfig(config, propsLoadedFromConfig);
        }

        private void LoadFavoriteProps()
        {
            string favoritePropsPlayerPref = PlayerPrefs.GetString(favoritePropsPlayerPrefKey);

            if (favoritePropsPlayerPref == null || favoritePropsPlayerPref == "") return;

            var favoritePropPaths = favoritePropsPlayerPref.Split(separatorCharacter);
            foreach (string favoriteProp in favoritePropPaths)
            {
                // DebugPropPlacer.RecentlyPlacedProps.Add(favoriteProp);
                this.favoriteProps.Add(favoriteProp);
            }

            favoritePropsList = favoriteProps.ToList();
        }

        internal override void OnGUI(DebugMenu menu)
        {
            //
            // DebugPropPlacer
            // 
            GUILayout.Label("Currently placing: ");
            _dpp.SetCurrentObject(GUILayout.TextArea(_dpp.currentObject));

            GUILayout.Space(5);
            
            var favoriteMenu = PropsList(menu, favoritePropsList, favoritePropsCollapsed, favoritePropsScrollPosition, "Favorite Props");
            favoritePropsCollapsed = favoriteMenu.collapsed;
            favoritePropsScrollPosition = favoriteMenu.scroll;
            GUILayout.Space(5);

            var recentMenu = PropsList(menu, DebugPropPlacer.RecentlyPlacedProps, recentPropsCollapsed, recentPropsScrollPosition, "Recently Placed Props");
            recentPropsCollapsed= recentMenu.collapsed;
            recentPropsScrollPosition = recentMenu.scroll;
            GUILayout.Space(5);

            var configMenu = PropsList(menu, propsLoadedFromConfig, configPropsCollapsed, configPropsScrollPosition, "Props Loaded From Configs");
            configPropsCollapsed = configMenu.collapsed;
            configPropsScrollPosition = configMenu.scroll;
            GUILayout.Space(5);

            
        }

        private struct PropsListReturn { public bool collapsed; public Vector2 scroll; } 
        private PropsListReturn PropsList(DebugMenu menu, IEnumerable<string> props, bool collapsed, Vector2 scroll, string title)
        {
            GUILayout.BeginVertical(menu._editorMenuStyle);
                        
                var arrow = collapsed ? " > " : " v ";
                if (GUILayout.Button(arrow + title, menu._submenuStyle)) collapsed = !collapsed;
                if (collapsed) return new PropsListReturn() { collapsed=collapsed, scroll=scroll };

                scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(menu.EditorMenuSize.x), GUILayout.Height(500));
                foreach (string propPath in props)
                {
                    GUILayout.BeginHorizontal();

                    var propPathElements = propPath[propPath.Length-1] == '/'
                        ? propPath.Substring(0, propPath.Length-1).Split('/')
                        : propPath.Split('/');
                    string propName = propPathElements[propPathElements.Length - 1];

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
                        PlayerPrefs.SetString(favoritePropsPlayerPrefKey, string.Join(separatorCharacter + "", favoritePropsArray));
                    }

                    if (GUILayout.Button(propName, GUILayout.ExpandWidth(true)))
                    {
                        _dpp.SetCurrentObject(propPath);
                    }

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            GUILayout.EndVertical();

            return new PropsListReturn() { collapsed=collapsed, scroll=scroll };
        }

        internal override void PreSave(DebugMenu menu)
        {
            UpdateLoadedConfigsForRecentSystem(menu);
        }

        private void UpdateLoadedConfigsForRecentSystem(DebugMenu menu)
        {
            var newDetails = _dpp.GetPropsConfigByBody();

            Logger.Log("Updating config files. New Details Counts by planet: " + string.Join(", ", newDetails.Keys.Select(x => x + $" ({newDetails[x].Length})")));

            Dictionary<string, string> planetToConfigPath = new Dictionary<string, string>();

            // Get all configs
            foreach (var filePath in menu.loadedConfigFiles.Keys)
            {
                Logger.Log("potentially updating copy of config at " + filePath);

                if (menu.loadedConfigFiles[filePath].starSystem != Main.Instance.CurrentStarSystem) return;
                if (menu.loadedConfigFiles[filePath].name == null || AstroObjectLocator.GetAstroObject(menu.loadedConfigFiles[filePath].name) == null) { Logger.Log("Failed to update copy of config at " + filePath); continue; }

                var astroObjectName = DebugPropPlacer.GetAstroObjectName(menu.loadedConfigFiles[filePath].name);
                planetToConfigPath[astroObjectName] = filePath;

                if (!newDetails.ContainsKey(astroObjectName)) continue;

                if (menu.loadedConfigFiles[filePath].Props == null) menu.loadedConfigFiles[filePath].Props = new External.Modules.PropModule();
                menu.loadedConfigFiles[filePath].Props.details = newDetails[astroObjectName];

                Logger.Log("successfully updated copy of config file for " + astroObjectName);
            }

            // find all new planets that do not yet have config paths
            var planetsThatDoNotHaveConfigFiles = newDetails.Keys.Where(x => !planetToConfigPath.ContainsKey(x)).ToList();
            foreach (var astroObjectName in planetsThatDoNotHaveConfigFiles)
            {
                Logger.Log("Fabricating new config file for " + astroObjectName);

                var filepath = "planets/" + Main.Instance.CurrentStarSystem + "/" + astroObjectName + ".json";
                PlanetConfig c = new PlanetConfig();
                c.starSystem = Main.Instance.CurrentStarSystem;
                c.name = astroObjectName;
                c.Props = new PropModule();
                c.Props.details = newDetails[astroObjectName];

                menu.loadedConfigFiles[filepath] = c;
            }
        }
    }
}
