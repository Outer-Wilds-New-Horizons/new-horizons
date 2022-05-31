using NewHorizons.External.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Utility.DebugMenu
{
    class DebugMenuDummySubmenu : DebugSubmenu
    {
        internal override void LoadConfigFile(DebugMenu menu, PlanetConfig config)
        {
        
        }

        internal override void OnAwake(DebugMenu menu)
        {
        }

        internal override void OnBeginLoadMod(DebugMenu debugMenu)
        {
        }

        internal override void OnGUI(DebugMenu menu)
        {
        }

        internal override void OnInit(DebugMenu menu)
        {
        
        }

        internal override void PreSave(DebugMenu menu)
        {
        
        }

        internal override string SubmenuName()
        {
            return "Blank";
        }
    }
}
