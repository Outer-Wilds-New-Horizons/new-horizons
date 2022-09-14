using HarmonyLib;

namespace NewHorizons.Patches.CameraPatches
{
    [HarmonyPatch]
    public static class OWCameraPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OWCamera), nameof(OWCamera.Awake))]
        public static void OnOWCameraAwake(OWCamera __instance)
        {
            if (Main.SystemDict.TryGetValue(Main.Instance.CurrentStarSystem, out var system) && system?.Config?.farClipPlaneOverride != 0f)
            {
                __instance.farClipPlane = system.Config.farClipPlaneOverride;
                __instance.farCameraDistance = system.Config.farClipPlaneOverride;
                __instance.mainCamera.farClipPlane = system.Config.farClipPlaneOverride;
            }
        }
    }
}
