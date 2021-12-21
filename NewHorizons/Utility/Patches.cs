using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class Patches
    {
        public static void Apply()
        {
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ReferenceFrame>("GetHUDDisplayName", typeof(Patches), nameof(Patches.GetHUDDisplayName));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<PlayerState>("CheckShipOutsideSolarSystem", typeof(Patches), nameof(Patches.CheckShipOutersideSolarSystem));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<EllipticOrbitLine>("Start", typeof(Patches), nameof(Patches.OnEllipticOrbitLineStart));
        }

        public static bool GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();
            if (ao != null && ao.GetAstroObjectName() == AstroObject.Name.CustomString)
            {
                __result = ao.GetCustomName();
                return false;
            }
            return true;
        }

        public static bool CheckShipOutersideSolarSystem(PlayerState __instance, bool __result)
        {
            __result = false;
            return false;
        }

        public static void OnEllipticOrbitLineStart(EllipticOrbitLine __instance, ref Vector3 ____upAxisDir, AstroObject ____astroObject)
        {
            if (____astroObject.GetAstroObjectName() == AstroObject.Name.Comet) return;

            // For some reason other planets do this idk
            ____upAxisDir *= -1f;    
        }
    }
}
