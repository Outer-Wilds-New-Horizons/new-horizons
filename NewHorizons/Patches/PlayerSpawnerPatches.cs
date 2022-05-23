using HarmonyLib;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class PlayerSpawnerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerSpawner), nameof(PlayerSpawner.SpawnPlayer))]
        public static void PlayerSpawner_SpawnPlayer(PlayerSpawner __instance)
        {
            Logger.Log("Player spawning");
            __instance.SetInitialSpawnPoint(Main.SystemDict[Main.Instance.CurrentStarSystem].SpawnPoint);
        }
    }
}
