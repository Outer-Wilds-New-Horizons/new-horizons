using HarmonyLib;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.SerializableData;
using NewHorizons.Utility.OWML;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Utility.DebugTools.Menu
{
    class DebugMenuShipLogs : DebugSubmenu
    {
        string entryPositionsText = "";

        internal override void GainActive()
        {
        
        }
        internal override void LoseActive()
        {
            
        }

        internal override void LoadConfigFile(DebugMenu menu, PlanetConfig config)
        {
        
        }


        internal override void OnAwake(DebugMenu menu)
        {
        }

        internal override void OnBeginLoadMod(DebugMenu debugMenu)
        {
        }

        private string GetEntryPositionsJSON() =>
            Resources
                .FindObjectsOfTypeAll<ShipLogEntryCard>()
                .Join(
                    entry => JsonConvert.SerializeObject(new ShipLogModule.EntryPositionInfo
                    {
                        id = entry.name,
                        position = new MVector2(entry.transform.localPosition.x, entry.transform.localPosition.y)
                    }, DebugMenu.jsonSettings),
                    ",\n"
                );

        internal override void OnGUI(DebugMenu menu)
        {
            if (GUILayout.Button("Print Ship Log Positions"))
            {
                entryPositionsText = GetEntryPositionsJSON();
            }

            GUILayout.TextArea(entryPositionsText);
        }

        internal override void OnInit(DebugMenu menu)
        {
        
        }

        internal override void PreSave(DebugMenu menu)
        {
        
        }

        internal override string SubmenuName()
        {
            return "Ship Log";
        }

        internal override void PrintNewConfigSection(DebugMenu menu)
        {
            NHLogger.Log(GetEntryPositionsJSON());
        }
    }
}
