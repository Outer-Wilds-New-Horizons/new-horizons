using HarmonyLib;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Patches.PlayerPatches
{
    [HarmonyPatch(typeof(PlayerSpawner))]
    public static class PlayerSpawnerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerSpawner.SpawnPlayer))]
        public static bool PlayerSpawner_SpawnPlayer(PlayerSpawner __instance)
        {
            if (Main.Instance.IsWarpingFromVessel || Main.Instance.DidWarpFromVessel)
            {
                Logger.LogWarning("Abort player spawn. Vessel will handle it.");
                return false;
            }
            else
            {
                Logger.LogVerbose("Player spawning");
                __instance.SetInitialSpawnPoint(Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint);
                return true;
            }
        }
    }
}
