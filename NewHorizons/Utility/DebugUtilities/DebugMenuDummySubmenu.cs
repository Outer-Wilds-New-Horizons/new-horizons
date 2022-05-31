using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Utility.DebugUtilities
{
    class DebugMenuDummySubmenu : DebugSubmenu
    {
        internal override void OnAwake(DebugMenu menu)
        {
        }

        internal override void OnGUI(DebugMenu menu)
        {
        }

        internal override string SubmenuName()
        {
            return "Blank";
        }
    }
}
