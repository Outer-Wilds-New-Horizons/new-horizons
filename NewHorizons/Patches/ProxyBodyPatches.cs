using HarmonyLib;
using NewHorizons.Components;
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
