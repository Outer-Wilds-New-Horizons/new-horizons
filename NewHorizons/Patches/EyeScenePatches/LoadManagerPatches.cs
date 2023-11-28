using HarmonyLib;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(LoadManager))]
    public static class LoadManagerPatches
    {
        private static void OnLoadScene(OWScene scene)
        {
            if (scene == OWScene.SolarSystem && !Main.Instance.IsWarpingBackToEye)
            {
                PlayerData.SaveEyeCompletion();

                // Switch to default just in case another mod warps back.
                if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse") Main.Instance.CurrentStarSystem = Main.Instance.DefaultStarSystem;
            }
            // Switch to eye just in case another mod warps there.
            else if (scene == OWScene.EyeOfTheUniverse) Main.Instance.CurrentStarSystem = "EyeOfTheUniverse";
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LoadManager.LoadSceneImmediate))]
        public static void LoadManager_LoadSceneImmediate(OWScene scene) => OnLoadScene(scene);

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LoadManager.StartAsyncSceneLoad))]
        public static void LoadManager_StartAsyncSceneLoad(OWScene scene) => OnLoadScene(scene);
    }
}
