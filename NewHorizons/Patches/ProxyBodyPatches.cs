using HarmonyLib;
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
    }
}
