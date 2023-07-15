using HarmonyLib;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;

namespace NewHorizons.Patches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerSpawner))]
    public static class PlayerSpawnerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerSpawner.SpawnPlayer))]
        public static bool PlayerSpawner_SpawnPlayer(PlayerSpawner __instance)
        {
            Main.Instance.PlayerSpawned = true;

            if (Main.Instance.IsWarpingFromVessel || Main.Instance.DidWarpFromVessel || Main.Instance.IsWarpingFromShip)
            {
                NHLogger.LogWarning("Abort player spawn. Vessel/Ship will handle it.");
                return false;
            }

            return true;
        }
    }
}
