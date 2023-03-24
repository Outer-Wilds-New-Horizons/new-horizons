using HarmonyLib;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(SubmitActionLoadScene))]
    public static class SubmitActionLoadScenePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SubmitActionLoadScene.ConfirmSubmit))]
        public static void SubmitActionLoadScene_ConfirmSubmit(SubmitActionLoadScene __instance)
        {
            // Title screen can warp you to eye and cause problems.
            if (__instance._sceneToLoad == SubmitActionLoadScene.LoadableScenes.EYE)
            {
                Utility.Logger.LogWarning("Warping to solar system and then back to eye");
                Main.Instance.IsWarpingBackToEye = true;
                __instance._sceneToLoad = SubmitActionLoadScene.LoadableScenes.GAME;
            }
        }
    }
}
