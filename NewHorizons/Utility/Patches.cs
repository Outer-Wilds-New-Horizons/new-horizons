using NewHorizons.Builder.Props;
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
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<SunSurfaceAudioController>("Update", typeof(Patches), nameof(Patches.OnSunSurfaceAudioControllerUpdate));
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix<AudioSignal>("SignalNameToString", typeof(Patches), nameof(Patches.OnAudioSignalSignalNameToString));

            var playerDataKnowsSignal = typeof(PlayerData).GetMethod("KnowsSignal");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataKnowsSignal, typeof(Patches), nameof(Patches.OnPlayerDataKnowsSignal));
            var playerDataLearnSignal = typeof(PlayerData).GetMethod("LearnSignal");
            Main.Instance.ModHelper.HarmonyHelper.AddPrefix(playerDataLearnSignal, typeof(Patches), nameof(Patches.OnPlayerDataLearnSignal));

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
            ____maxPanDistance = Main.FurthestOrbit * 1.5f;
            ____maxZoomDistance *= 6f;
            ____minPitchAngle = -90f;
            ____zoomSpeed *= 4f;
        }

        public static void OnOWCameraAwake(OWCamera __instance)
        {
            __instance.farClipPlane = Main.FurthestOrbit * 3f;
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

        public static bool OnSunSurfaceAudioControllerUpdate(SunSurfaceAudioController __instance)
        {
            if (__instance._sunController != null) return true;

            var surfaceRadius = __instance.transform.parent.parent.localScale.magnitude;
            float value = Mathf.Max(0f, Vector3.Distance(Locator.GetPlayerCamera().transform.position, __instance.transform.position) - surfaceRadius);
            float num = Mathf.InverseLerp(1600f, 100f, value);
            __instance._audioSource.SetLocalVolume(num * num * __instance._fade);
            return false;
        }

        public static bool OnAudioSignalSignalNameToString(SignalName __0, ref string __result)
        {
            switch(__0)
            {
                case SignalName.WhiteHole_SS_Receiver:
                    __result = "Sun Station Receiver";
                    return false;
                case SignalName.WhiteHole_CT_Receiver:
                    __result = "Ember Twin Receiver";
                    return false;
                case SignalName.WhiteHole_CT_Experiment:
                    __result = "White Hole Receiver";
                    return false;
                case SignalName.WhiteHole_TT_Receiver:
                    __result = "Ash Twin Receiver";
                    return false;
                case SignalName.WhiteHole_TT_TimeLoopCore:
                    __result = "Ash Twin Project";
                    return false;
                case SignalName.WhiteHole_TH_Receiver:
                    __result = "Timber Hearth Receiver";
                    return false;
                case SignalName.WhiteHole_BH_NorthPoleReceiver:
                    __result = "North Pole Receiver";
                    return false;
                case SignalName.WhiteHole_BH_ForgeReceiver:
                    __result = "Black Hole Forge Receiver";
                    return false;
                case SignalName.WhiteHole_GD_Receiver:
                    __result = "Giant's Deep Receiver";
                    return false;
                default:
                    var customSignalName = SignalBuilder.GetCustomSignalName(__0);
                    if (customSignalName == null) return true;
                    else
                    {
                        __result = customSignalName;
                        return false;
                    }
            }
        }

        public static bool OnPlayerDataKnowsSignal(SignalName __0, ref bool __result)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(__0);
            if (customSignalName != null)
            {
                __result = SignalBuilder.KnownSignals.Contains(customSignalName);
                return false;
            }
            return true;
        }

        public static bool OnPlayerDataLearnSignal(SignalName __0)
        {
            var customSignalName = SignalBuilder.GetCustomSignalName(__0);
            if (customSignalName != null)
            {
                if(!SignalBuilder.KnownSignals.Contains(customSignalName)) SignalBuilder.KnownSignals.Add(customSignalName);
                return false;
            }
            return true;
        }
    }
}
