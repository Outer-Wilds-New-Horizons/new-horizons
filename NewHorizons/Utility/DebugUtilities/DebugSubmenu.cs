using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Utility.DebugUtilities
{
    abstract class DebugSubmenu
    {
        internal abstract void OnAwake(DebugMenu menu);
        internal abstract void OnGUI(DebugMenu menu);

        internal abstract string SubmenuName();
    }
}
