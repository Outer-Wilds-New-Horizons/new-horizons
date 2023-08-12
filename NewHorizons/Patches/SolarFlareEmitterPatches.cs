using HarmonyLib;
using System.Linq;

namespace NewHorizons.Patches
{
    [HarmonyPatch(typeof(SolarFlareEmitter))]
    public static class SolarFlareEmitterPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SolarFlareEmitter.Awake))]
        public static void SolarFlareEmitter_Awake(SolarFlareEmitter __instance)
        {
            // Because in StarBuilder we use inactive game objects instead of real prefabs these objects all get created inactive
            foreach (var flare in __instance._streamers.Concat(__instance._loops).Concat(__instance._domes))
            {
                flare.gameObject.SetActive(true);
                flare.enabled = true;
            }
        }
    }
}
