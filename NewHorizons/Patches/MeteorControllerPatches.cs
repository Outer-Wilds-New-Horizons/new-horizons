using HarmonyLib;

namespace NewHorizons.Patches
{
    [HarmonyPatch(typeof(MeteorController))]
    public static class MeteorControllerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(MeteorController.Suspend), new System.Type[0])]
        public static void MeteorController_Suspend(MeteorController __instance)
        {
            // Meteors launch inactive because of prefab. So let's fix that.
            // how tf does this work
            __instance.gameObject.SetActive(true);
        }
    }
}
