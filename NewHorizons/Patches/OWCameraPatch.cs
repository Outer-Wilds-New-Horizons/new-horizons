#region

using HarmonyLib;

#endregion

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class OWCameraPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OWCamera), nameof(OWCamera.Awake))]
        public static void OnOWCameraAwake(OWCamera __instance)
        {
            // var oldDist = __instance.farClipPlane;
            // var newDist = __instance.farClipPlane * 10f;
            // if (__instance.useFarCamera) Mathf.Clamp(newDist, oldDist, 50000f);
            // else newDist = Mathf.Clamp(newDist, oldDist, 10000000f);
            // __instance.farClipPlane = newDist;
            // __instance.farCameraDistance = newDist;
            // __instance.mainCamera.farClipPlane = newDist;
        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(OWCamera), nameof(OWCamera.RebuildSkybox))]
        // public static bool OnOWCameraRebuildSkybox(OWCamera __instance)
        // {
        //     __instance._skyboxCommandBuffer = new CommandBuffer();
        //     __instance._skyboxCommandBuffer.name = "Skybox";
        //     var camera = __instance._useFarCamera && !SystemInfo.usesReversedZBuffer ? __instance._farCamera : __instance._mainCamera;
        //     CameraEvent evt = CameraEvent.BeforeSkybox;
        //     camera.AddCommandBuffer(evt, __instance._skyboxCommandBuffer);
        //     return false;
        // }
    }
}