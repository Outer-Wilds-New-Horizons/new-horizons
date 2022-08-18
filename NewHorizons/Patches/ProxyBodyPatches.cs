using HarmonyLib;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class ProxyBodyPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyBody), nameof(ProxyBody.Awake))]
        public static void ProxyBody_Awake(ProxyBody __instance)
        {
            // Mobius rly used the wrong event name
            GlobalMessenger.AddListener("EnterMapView", __instance.OnEnterMapView);
            GlobalMessenger.AddListener("ExitMapView", __instance.OnExitMapView);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyBody), nameof(ProxyBody.OnDestroy))]
        public static void ProxyBody_OnDestroy(ProxyBody __instance)
        {
            GlobalMessenger.RemoveListener("EnterMapView", __instance.OnEnterMapView);
            GlobalMessenger.RemoveListener("ExitMapView", __instance.OnExitMapView);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyBody), nameof(ProxyBody.OnEnterMapView))]
        public static void ProxyBody_OnEnterMapView(ProxyBody __instance)
        {
            // Set this to false before the method sets the rendering to false so it matches
            __instance._outOfRange = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyBody), nameof(ProxyBody.IsObjectInSupernova))]
        public static bool ProxyBody_IsObjectInSupernova(ProxyBody __instance, ref bool __result)
        {
            __result = SupernovaEffectHandler.InPointInsideAnySupernova(__instance._realObjectTransform.position);
            return false;
        }
        
        // Mobius why doesn't ProxyOrbiter inherit from ProxyBody
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyOrbiter), nameof(ProxyOrbiter.Awake))]
        public static void ProxyOrbiter_Awake(ProxyOrbiter __instance)
        {
            // Mobius rly used the wrong event name
            GlobalMessenger.AddListener("EnterMapView", __instance.OnEnterMapView);
            GlobalMessenger.AddListener("ExitMapView", __instance.OnExitMapView);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyOrbiter), nameof(ProxyOrbiter.OnDestroy))]
        public static void ProxyOrbiter_OnDestroy(ProxyOrbiter __instance)
        {
            GlobalMessenger.RemoveListener("EnterMapView", __instance.OnEnterMapView);
            GlobalMessenger.RemoveListener("ExitMapView", __instance.OnExitMapView);
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(ProxyPlanet), nameof(ProxyPlanet.Initialize))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ProxyPlanet_Initialize(ProxyPlanet instance) { }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyBrittleHollow), nameof(ProxyBrittleHollow.Initialize))]
        public static bool ProxyBrittleHollow_Initialize(ProxyBrittleHollow __instance)
        {
            try
            {
                ProxyPlanet_Initialize(__instance);
                __instance._moon.SetOriginalBodies(Locator.GetAstroObject(AstroObject.Name.VolcanicMoon).transform, Locator.GetAstroObject(AstroObject.Name.BrittleHollow).transform);
                if (!__instance._fragmentsResolved) __instance.ResolveFragments();
                __instance._blackHoleMaterial = new Material(__instance._blackHoleRenderer.sharedMaterial);
                __instance._blackHoleRenderer.sharedMaterial = __instance._blackHoleMaterial;
            }
            catch (NullReferenceException ex)
            {
                __instance.PrintInitializeFailMessage(ex);
                UnityEngine.Object.Destroy(__instance._moon.gameObject);
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyTimberHearth), nameof(ProxyTimberHearth.Initialize))]
        public static bool ProxyTimberHearth_Initialize(ProxyTimberHearth __instance)
        {
            try
            {
                ProxyPlanet_Initialize(__instance);
                __instance._moon.SetOriginalBodies(Locator.GetAstroObject(AstroObject.Name.TimberMoon).transform, Locator.GetAstroObject(AstroObject.Name.TimberHearth).transform);
            }
            catch (NullReferenceException ex)
            {
                __instance.PrintInitializeFailMessage(ex);
                UnityEngine.Object.Destroy(__instance._moon.gameObject);
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProxyAshTwin), nameof(ProxyAshTwin.Initialize))]
        public static bool ProxyAshTwin_Initialize(ProxyAshTwin __instance)
        {
            try
            {
                ProxyPlanet_Initialize(__instance);
                __instance._realSandTransform = Locator.GetAstroObject(AstroObject.Name.TowerTwin).GetSandLevelController().transform;
                SandFunnelController sandFunnelController = SearchUtilities.Find("SandFunnel_Body", false)?.GetComponent<SandFunnelController>();
                if (sandFunnelController != null)
                {
                    __instance._realSandColumnRoot = sandFunnelController.scaleRoot;
                    __instance._realSandColumnRenderObject = sandFunnelController.sandGeoObjects[0];
                }
            }
            catch (NullReferenceException ex)
            {
                __instance.PrintInitializeFailMessage(ex);
                UnityEngine.Object.Destroy(__instance.gameObject);
            }
            return false;
        }
    }
}
