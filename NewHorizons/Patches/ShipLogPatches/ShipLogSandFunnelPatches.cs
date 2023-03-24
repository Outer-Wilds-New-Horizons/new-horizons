using HarmonyLib;

namespace NewHorizons.Patches.ShipLogPatches
{
    [HarmonyPatch(typeof(ShipLogSandFunnel))]
    public static class ShipLogSandFunnelPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogSandFunnel.UpdateState))]
        public static bool ShipLogSandFunnel_UpdateState()
        {
            return Main.Instance.CurrentStarSystem == "SolarSystem";
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ShipLogSandFunnel.Awake))]
        public static bool ShipLogSandFunnel_Awake()
        {
            return Main.Instance.CurrentStarSystem == "SolarSystem";
        }
    }
}
