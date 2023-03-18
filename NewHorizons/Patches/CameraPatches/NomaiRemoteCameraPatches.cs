using HarmonyLib;
using NewHorizons.OtherMods.CommonCameraUtility;
using UnityEngine;

namespace NewHorizons.Patches.CameraPatches
{
    [HarmonyPatch(typeof(NomaiRemoteCamera))]
    public static class NomaiRemoteCameraPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(NomaiRemoteCamera.Awake))]
        public static void NomaiRemoteCamera_Awake(NomaiRemoteCamera __instance)
        {
            // Ensures that if the player is visible from the remote camera they look normal
            CommonCameraHandler.RegisterCustomCamera(__instance._camera);

            // These layers were left on because it doesnt come up in base game (Dreamworld is inactive, player is far away)
            __instance._camera.mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("DreamSimulation"));
            __instance._camera.mainCamera.cullingMask &= ~(1 <<LayerMask.NameToLayer("UI"));
            __instance._camera.mainCamera.cullingMask &= ~(1 <<LayerMask.NameToLayer("HeadsUpDisplay"));
            __instance._camera.mainCamera.cullingMask &= ~(1 <<LayerMask.NameToLayer("HelmetUVPass"));
        }
    }
}
