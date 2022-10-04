using HarmonyLib;
using NewHorizons.OtherMods.CommonCameraUtility;
using UnityEngine;

namespace NewHorizons.Patches.CameraPatches
{
    [HarmonyPatch]
    public static class NomaiRemoteCameraPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NomaiRemoteCamera), nameof(NomaiRemoteCamera.Awake))]
        public static void NomaiRemoteCamera_Awake(NomaiRemoteCamera __instance)
        {
            // These layers were left on because it doesnt come up in base game (Dreamworld is inactive, player is far away)
            __instance._camera.mainCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("DreamSimulation"));
            __instance._camera.mainCamera.cullingMask &= ~(1 <<LayerMask.NameToLayer("UI"));
            __instance._camera.mainCamera.cullingMask &= ~(1 <<LayerMask.NameToLayer("HeadsUpDisplay"));
            __instance._camera.mainCamera.cullingMask &= ~(1 <<LayerMask.NameToLayer("HelmetUVPass"));
        }
    }
}
