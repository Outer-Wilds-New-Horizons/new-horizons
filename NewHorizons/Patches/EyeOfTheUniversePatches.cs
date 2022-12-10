using HarmonyLib;
using UnityEngine;
namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class EyeOfTheUniversePatches
    {
        // Funny eye of the universe stuff

        private static void OnLoadScene(OWScene scene)
        {
            if (scene == OWScene.SolarSystem && !Main.Instance.IsWarpingBackToEye)
            {
                PlayerData.SaveEyeCompletion();

                // Switch to default just in case another mod warps back.
                if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse") Main.Instance._currentStarSystem = Main.Instance.DefaultStarSystem;
            }
            // Switch to eye just in case another mod warps there.
            else if (scene == OWScene.EyeOfTheUniverse) Main.Instance._currentStarSystem = "EyeOfTheUniverse";
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LoadManager), nameof(LoadManager.LoadSceneImmediate))]
        public static void LoadManager_LoadSceneImmediate(OWScene scene) => OnLoadScene(scene);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LoadManager), nameof(LoadManager.StartAsyncSceneLoad))]
        public static void LoadManager_StartAsyncSceneLoad(OWScene scene) => OnLoadScene(scene);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SubmitActionLoadScene), nameof(SubmitActionLoadScene.ConfirmSubmit))]
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EyeVortexTrigger), nameof(EyeVortexTrigger.OnEnterVortex))]
        public static void EyeVortexTrigger_OnEnterVortex(EyeVortexTrigger __instance, GameObject hitObj)
        {
            if (!hitObj.CompareTag("PlayerDetector")) return;
            __instance._tunnelObject.SetActive(true);
        }

    }
}
