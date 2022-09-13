using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class MeteorPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MeteorController), nameof(MeteorController.Suspend), new System.Type[0])]
        public static void MeteorController_Suspend(MeteorController __instance)
        {
            __instance.gameObject.SetActive(true);
        }
    }
}
