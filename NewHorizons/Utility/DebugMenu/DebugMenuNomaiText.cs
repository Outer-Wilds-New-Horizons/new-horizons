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

    /*
     * Strategy:
     * load all existing nomai text and allow the user to select them from a list
     * from there, allow them to edit the placement of the one selected
     */

    class DebugMenuNomaiText : DebugSubmenu
    {
        internal DebugRaycaster _drc;

        class NomaiTextTree
        {
            public GameObject text;
            public int variation;
            public float arcLengthLocationOnParent;

            public List<NomaiTextTree> children = new List<NomaiTextTree>();
        }

        List<NomaiTextTree> textTrees = new List<NomaiTextTree>();
        NomaiTextTree currentTree;


        internal override string SubmenuName()
        {
            return "Text Placer";
        }

        internal override void OnInit(DebugMenu menu)
        {
            _drc = menu.GetComponent<DebugRaycaster>();
        }

        internal override void OnAwake(DebugMenu menu)
        {
            _drc = menu.GetComponent<DebugRaycaster>();
        }

        internal override void OnBeginLoadMod(DebugMenu debugMenu)
        {
            DebugPropPlacer.active = true;
        }
        
        internal override void LoadConfigFile(DebugMenu menu, PlanetConfig config)
        {
            // TODO: populate textTrees
        }

        internal override void OnGUI(DebugMenu menu)
        {
            GUILayout.Space(5);

            GUILayout.Space(5);
        }

        void DrawSpiralControls(int indentationLevel, NomaiTextTree tree)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(5*indentationLevel);

            GUILayout.EndHorizontal();

            tree.children.ForEach(child => DrawSpiralControls(indentationLevel+1, child));
        }

        internal override void PreSave(DebugMenu menu)
        {
            UpdateLoadedConfigsForRecentSystem(menu);
        }

        private void UpdateLoadedConfigsForRecentSystem(DebugMenu menu)
        {
            //var newDetails = _dpp.GetPropsConfigByBody();

            //Logger.Log("Updating config files. New Details Counts by planet: " + string.Join(", ", newDetails.Keys.Select(x => x + $" ({newDetails[x].Length})")));

            //Dictionary<string, string> planetToConfigPath = new Dictionary<string, string>();

            //// Get all configs
            //foreach (var filePath in menu.loadedConfigFiles.Keys)
            //{
            //    Logger.Log("potentially updating copy of config at " + filePath);

            //    if (menu.loadedConfigFiles[filePath].starSystem != Main.Instance.CurrentStarSystem) return;
            //    if (menu.loadedConfigFiles[filePath].name == null || AstroObjectLocator.GetAstroObject(menu.loadedConfigFiles[filePath].name) == null) { Logger.Log("Failed to update copy of config at " + filePath); continue; }

            //    var astroObjectName = DebugPropPlacer.GetAstroObjectName(menu.loadedConfigFiles[filePath].name);
            //    planetToConfigPath[astroObjectName] = filePath;

            //    if (!newDetails.ContainsKey(astroObjectName)) continue;

            //    if (menu.loadedConfigFiles[filePath].Props == null) menu.loadedConfigFiles[filePath].Props = new External.Modules.PropModule();
            //    menu.loadedConfigFiles[filePath].Props.details = newDetails[astroObjectName];

            //    Logger.Log("successfully updated copy of config file for " + astroObjectName);
            //}

            //// find all new planets that do not yet have config paths
            //var planetsThatDoNotHaveConfigFiles = newDetails.Keys.Where(x => !planetToConfigPath.ContainsKey(x)).ToList();
            //foreach (var astroObjectName in planetsThatDoNotHaveConfigFiles)
            //{
            //    Logger.Log("Fabricating new config file for " + astroObjectName);

            //    var filepath = "planets/" + Main.Instance.CurrentStarSystem + "/" + astroObjectName + ".json";
            //    PlanetConfig c = new PlanetConfig();
            //    c.starSystem = Main.Instance.CurrentStarSystem;
            //    c.name = astroObjectName;
            //    c.Props = new PropModule();
            //    c.Props.details = newDetails[astroObjectName];

            //    menu.loadedConfigFiles[filepath] = c;
            //}
        }
    }
}
