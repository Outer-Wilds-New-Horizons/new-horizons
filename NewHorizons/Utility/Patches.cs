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
            // Prefixes
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<ReferenceFrame>("GetHUDDisplayName", typeof(Patches), nameof(Patches.GetHUDDisplayName));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<PlayerState>("CheckShipOutsideSolarSystem", typeof(Patches), nameof(Patches.CheckShipOutersideSolarSystem));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SunLightParamUpdater>("LateUpdate", typeof(Patches), nameof(Patches.OnSunLightParamUpdaterLateUpdate));

            // Postfixes
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<EllipticOrbitLine>("Start", typeof(Patches), nameof(Patches.OnEllipticOrbitLineStart));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<MapController>("Awake", typeof(Patches), nameof(Patches.OnMapControllerAwake));
            Main.Instance.ModHelper.HarmonyHelper.AddPostfix<OWCamera>("Awake", typeof(Patches), nameof(Patches.OnOWCameraAwake));
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

        public static void OnMapControllerAwake(MapController __instance, ref float ____maxPanDistance, ref float ____maxZoomDistance, ref float ____minPitchAngle, ref float ____zoomSpeed)
        {
            ____maxPanDistance *= 4f;
            ____maxZoomDistance *= 6f;
            ____minPitchAngle = -90f;
            ____zoomSpeed *= 4f;
        }

        public static void OnOWCameraAwake(OWCamera __instance)
        {
            __instance.farClipPlane *= 4f;
        }

        public static bool OnSunLightParamUpdaterLateUpdate(SunLightParamUpdater __instance)
        {
            if (__instance.sunLight)
            {
                Vector3 position = __instance.transform.position;
                float w = 2000f;
                if (__instance._sunController != null)
                {
                    w = (__instance._sunController.HasSupernovaStarted() ? __instance._sunController.GetSupernovaRadius() : __instance._sunController.GetSurfaceRadius());
                }
                float range = __instance.sunLight.range;
                Color color = (__instance._sunLightController != null) ? __instance._sunLightController.sunColor : __instance.sunLight.color;
                float w2 = (__instance._sunLightController != null) ? __instance._sunLightController.sunIntensity : __instance.sunLight.intensity;
                Shader.SetGlobalVector(__instance._propID_SunPosition, new Vector4(position.x, position.y, position.z, w));
                Shader.SetGlobalVector(__instance._propID_OWSunPositionRange, new Vector4(position.x, position.y, position.z, 1f / (range * range)));
                Shader.SetGlobalVector(__instance._propID_OWSunColorIntensity, new Vector4(color.r, color.g, color.b, w2));
            }

            return false;
        }
    }
}
