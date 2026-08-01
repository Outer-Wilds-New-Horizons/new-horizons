using HarmonyLib;
using NewHorizons.Components;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch(typeof(SurfaceManager))]
    public class SurfaceManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SurfaceManager.GetHitSurfaceType))]
        public static bool SurfaceManager_GetHitSurfaceType(SurfaceManager __instance, RaycastHit hitInfo, ref SurfaceType __result)
        {
            if (hitInfo.collider.TryGetComponent(out SurfaceTypeLookup surfaceTypeLookup) && surfaceTypeLookup.surfaceTypes != null && surfaceTypeLookup.surfaceTypes.Length > 0)
            {
                int hitSubmesh = __instance.GetHitSubmesh(hitInfo);
                if (hitSubmesh >= 0 && hitSubmesh < surfaceTypeLookup.surfaceTypes.Length)
                {
                    __result = surfaceTypeLookup.surfaceTypes[hitSubmesh];
                    return false;
                }
            }
            return true;
        }
    }
}
