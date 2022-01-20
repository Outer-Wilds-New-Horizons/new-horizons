using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Tools
{
    public static class OWCameraFix
    {
        public static void Apply()
        {
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<OWCamera>("Awake", typeof(OWCameraFix), nameof(OWCameraFix.OnOWCameraAwake));
        }

        private static void OnOWCameraAwake(OWCamera __instance)
        {
            var newDist = __instance.farClipPlane * 10f;
            newDist = Mathf.Clamp(newDist, 500000f, 10000000f);
            __instance.farClipPlane = newDist;
            __instance.farCameraDistance = newDist;
        }
    }
}
