using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Utility
{
    public class Patches
    {
        public static void Apply()
        {
            Main.helper.HarmonyHelper.AddPrefix<ReferenceFrame>("GetHUDDisplayName", typeof(Patches), nameof(Patches.GetHUDDisplayName));
        }

        public static bool GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();
            if (ao.GetAstroObjectName() == AstroObject.Name.CustomString)
            {
                __result = ao.GetCustomName();
                return false;
            }
            return true;
        }
    }
}
