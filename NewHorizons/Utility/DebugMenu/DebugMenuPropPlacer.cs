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
        private List<string> propsLoadedFromConfig = new List<string>();
        public static readonly char separatorCharacter = '☧'; // since no chars are illegal in game object names, I picked one that's extremely unlikely to be used to be a separator
        private static readonly string favoritePropsPlayerPrefKey = "FavoriteProps";

        internal DebugPropPlacer _dpp;
        internal DebugRaycaster _drc;

        // misc
        private GameObject mostRecentlyPlacedProp;
        private Vector3 mostRecentlyPlacedPropSphericalPos;

        // menu params
        private Vector2 recentPropsScrollPosition = Vector2.zero;
        private bool propsCollapsed = false;
        private Vector3 propPosDelta = new Vector3(0.1f, 0.1f, 0.1f);
        private Vector3 propSphericalPosDelta = new Vector3(0.1f, 0.1f, 0.1f);

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
                DebugPropPlacer.RecentlyPlacedProps.Add(favoriteProp);
                this.favoriteProps.Add(favoriteProp);
            }
        }

        internal override void OnGUI(DebugMenu menu)
        {
            //
            // DebugPropPlacer
            // 
            GUILayout.Label("Currently placing: ");
            _dpp.SetCurrentObject(GUILayout.TextArea(_dpp.currentObject));

            GUILayout.Space(5);
            GUILayout.Space(5);
            
            
            var arrow = propsCollapsed ? " > " : " v ";
            if (GUILayout.Button(arrow + "Recently placed objects", menu._tabBarStyle)) propsCollapsed = !propsCollapsed;
            if (!propsCollapsed) DrawPropsList(menu); 
            GUILayout.Space(5);
            
            if (_dpp.mostRecentlyPlacedPropGO != null)
            {
        
                var propPath = _dpp.mostRecentlyPlacedPropPath;
                var propPathElements = propPath[propPath.Length-1] == '/'
                    ? propPath.Substring(0, propPath.Length-1).Split('/')
                    : propPath.Split('/');
                string propName = propPathElements[propPathElements.Length - 1];
                GUILayout.Label($"Reposition {propName}: ");

                //Vector3 latestPropPosDelta = VectorInput(_dpp.mostRecentlyPlacedPropGO.transform.localPosition, propPosDelta, out propPosDelta, "x", "y", "z");
                //_dpp.mostRecentlyPlacedPropGO.transform.localPosition += latestPropPosDelta;
                //if (latestPropPosDelta != Vector3.zero) mostRecentlyPlacedPropSphericalPos = DeltaSphericalPosition(mostRecentlyPlacedProp, Vector3.zero);        

                GUILayout.Space(5);


                if (mostRecentlyPlacedProp != _dpp.mostRecentlyPlacedPropGO)
                {
                    mostRecentlyPlacedProp = _dpp.mostRecentlyPlacedPropGO;
                    mostRecentlyPlacedPropSphericalPos = DeltaSphericalPosition(mostRecentlyPlacedProp, Vector3.zero);
                }

                Vector3 latestPropSphericalPosDelta = VectorInput(mostRecentlyPlacedPropSphericalPos, propSphericalPosDelta, out propSphericalPosDelta, "lat   ", "lon   ", "height");
                if (latestPropSphericalPosDelta != Vector3.zero)
                {
                    Logger.Log("Prop pos delta "+latestPropSphericalPosDelta);
                    SetSphericalPosition(mostRecentlyPlacedProp, mostRecentlyPlacedPropSphericalPos+latestPropSphericalPosDelta);
                    mostRecentlyPlacedPropSphericalPos = mostRecentlyPlacedPropSphericalPos+latestPropSphericalPosDelta;
                }
            }
        }

        private Vector3 DeltaSphericalPosition(GameObject prop, Vector3 deltaSpherical)
        {
            Transform astroObject = prop.transform.parent.parent; 
            Vector3 originalLocalPos = astroObject.InverseTransformPoint(prop.transform.position); // parent is the sector, this gives localPos relative to the astroobject (what the DetailBuilder asks for)
            Vector3 sphericalPos = CoordinateUtilities.CartesianToSpherical(originalLocalPos);
            
            if (deltaSpherical == Vector3.zero) return sphericalPos;

            SetSphericalPosition(prop, sphericalPos+deltaSpherical);
            return sphericalPos+deltaSpherical;
        }

        private void SetSphericalPosition(GameObject prop, Vector3 newSpherical)
        {
            Transform astroObject = prop.transform.parent.parent; 
            Vector3 originalLocalPos = astroObject.InverseTransformPoint(prop.transform.position); // parent is the sector, this gives localPos relative to the astroobject
            Vector3 finalLocalPosition = CoordinateUtilities.SphericalToCartesian(newSpherical);

            Vector3 finalAbsolutePosition = astroObject.TransformPoint(finalLocalPosition);
            prop.transform.localPosition = prop.transform.parent.InverseTransformPoint(finalAbsolutePosition);
            prop.transform.rotation = prop.transform.rotation * Quaternion.FromToRotation(originalLocalPos.normalized, finalLocalPosition.normalized);

            //Logger.Log("Original local: " + originalLocalPos + "  original spherical: " + sphericalPos +  "   delta spherical: " + deltaSpherical  + "   new speherical: " + (sphericalPos+deltaSpherical) + " new local: " + finalLocalPosition);
            Logger.Log("Original local: " + originalLocalPos + "  new spherical: " + newSpherical + " new local: " + finalLocalPosition);
        }

        private Vector3 VectorInput(Vector3 input, Vector3 deltaControls, out Vector3 deltaControlsOut, string labelX, string labelY, string labelZ)
        {
            var dx = deltaControls.x;
            var dy = deltaControls.y;
            var dz = deltaControls.z;

            // x
            GUILayout.BeginHorizontal();
                GUILayout.Label(labelX+":     ", GUILayout.Width(50));
				var xString = input.x+"";
				var newXString = GUILayout.TextField(xString, GUILayout.Width(50));
				var parsedXString = input.x; try { parsedXString = float.Parse(newXString); } catch {}
                float deltaX = xString == newXString ? 0 : parsedXString - input.x;
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) deltaX += dx;
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) deltaX -= dx;
                dx = float.Parse(GUILayout.TextField(dx+"", GUILayout.Width(100)));
            GUILayout.EndHorizontal();
            // y
            GUILayout.BeginHorizontal();
                GUILayout.Label(labelY+":     ", GUILayout.Width(50));
				var yString = input.y+"";
				var newYString = GUILayout.TextField(yString, GUILayout.Width(50));
				var parsedYString = input.y; try { parsedYString = float.Parse(newYString); } catch {}
                float deltaY = yString == newYString ? 0 : parsedYString - input.y;
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) deltaY += dy;
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) deltaY -= dy;
                dy = float.Parse(GUILayout.TextField(dy+"", GUILayout.Width(100)));
            GUILayout.EndHorizontal();
            // z
            GUILayout.BeginHorizontal();
                GUILayout.Label(labelZ+":     ", GUILayout.Width(50));
				var zString = input.z+"";
				var newZString = GUILayout.TextField(zString, GUILayout.Width(50));
				var parsedZString = input.z; try { parsedZString = float.Parse(newZString); } catch {}
                float deltaZ = zString == newZString ? 0 : parsedZString - input.z;
                if (GUILayout.Button("+", GUILayout.ExpandWidth(false))) deltaZ += dz;
                if (GUILayout.Button("-", GUILayout.ExpandWidth(false))) deltaZ -= dz;
                dz = float.Parse(GUILayout.TextField(dz+"", GUILayout.Width(100)));
            GUILayout.EndHorizontal();

            deltaControlsOut = new Vector3(dx, dy, dz);
            return new Vector3(deltaX, deltaY, deltaZ);
        }

        private void DrawPropsList(DebugMenu menu)
        {
            // List of recently placed objects
            recentPropsScrollPosition = GUILayout.BeginScrollView(recentPropsScrollPosition, GUILayout.Width(menu.EditorMenuSize.x), GUILayout.Height(500));
            foreach (string propPath in DebugPropPlacer.RecentlyPlacedProps)
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

                if (GUILayout.Button(propName))
                {
                    _dpp.SetCurrentObject(propPath);
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();           
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
