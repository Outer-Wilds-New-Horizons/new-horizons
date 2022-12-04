using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility.DebugMenu
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

        private string GetEntryPositionsJSON() 
        {
            return string.Join(",\n",
                Resources
                    .FindObjectsOfTypeAll<ShipLogEntryCard>()
                    .Select(go => JsonConvert.SerializeObject(new ShipLogModule.EntryPositionInfo
                    {
                        id = go.name,
                        position = new MVector2(go.transform.localPosition.x, go.transform.localPosition.y)
                    }, DebugMenu.jsonSettings))
            );
        }

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
            Logger.Log(GetEntryPositionsJSON());
        }
    }
}
