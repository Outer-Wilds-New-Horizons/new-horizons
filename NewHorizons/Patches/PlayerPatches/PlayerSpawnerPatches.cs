using HarmonyLib;
using NewHorizons.Utility;
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
            else if (Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint != null)
            {
                Logger.LogVerbose($"Player spawning at {Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint.transform.GetPath()}");
                __instance.SetInitialSpawnPoint(Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint);
            } else if (Main.Instance.CurrentStarSystem != "SolarSystem" && Main.Instance.CurrentStarSystem != "EyeOfTheUniverse")
            {
                Logger.LogWarning("No player spawn point set.");
            }
            return true;
        }
    }
}
