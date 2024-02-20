using Autodesk.Fbx;
using HarmonyLib;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;

namespace NewHorizons.Patches.EyeScenePatches
{
    [HarmonyPatch(typeof(SubmitActionLoadScene))]
    public static class SubmitActionLoadScenePatches
    {
        // To call the base method
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(SubmitActionConfirm), nameof(SubmitActionConfirm.ConfirmSubmit))]
        public static void SubmitActionConfirm_ConfirmSubmit(SubmitActionConfirm instance) { }

        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SubmitActionLoadScene.ConfirmSubmit))]
        public static bool SubmitActionLoadScene_ConfirmSubmit(SubmitActionLoadScene __instance)
        {
            if (__instance._receivedSubmitAction) return false;
            
            // Title screen can warp you to eye and cause problems.
            if (__instance._sceneToLoad == SubmitActionLoadScene.LoadableScenes.EYE)
            {
                NHLogger.LogWarning("Warping to solar system and then back to eye");
                Main.Instance.IsWarpingBackToEye = true;
                __instance._sceneToLoad = SubmitActionLoadScene.LoadableScenes.GAME;
            }

            // Don't bother going through this stuff if we don't have to
            if (AssetBundleUtilities.AreRequiredAssetsLoaded()) return true;
            
            // modified from patched function
            SubmitActionConfirm_ConfirmSubmit(__instance);            
            __instance._receivedSubmitAction = true;
            Locator.GetMenuInputModule().DisableInputs();

            Delay.RunWhen(() =>
            {
                // update text. just use 0%
                __instance.ResetStringBuilder();
                __instance._nowLoadingSB.Append(UITextLibrary.GetString(UITextType.LoadingMessage));
                __instance._nowLoadingSB.Append(0.ToString("P0"));
                __instance._loadingText.text = __instance._nowLoadingSB.ToString();

                return AssetBundleUtilities.AreRequiredAssetsLoaded();
            }, () =>
            {
                switch (__instance._sceneToLoad)
                {
                    case SubmitActionLoadScene.LoadableScenes.GAME:
                        LoadManager.LoadSceneAsync(OWScene.SolarSystem, false, LoadManager.FadeType.ToBlack, 1f, false);
                        __instance.ResetStringBuilder();
                        __instance._waitingOnStreaming = true;
                        break;
                    case SubmitActionLoadScene.LoadableScenes.EYE:
                        LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, true, LoadManager.FadeType.ToBlack, 1f, false);
                        __instance.ResetStringBuilder();
                        break;
                    case SubmitActionLoadScene.LoadableScenes.TITLE:
                        LoadManager.LoadScene(OWScene.TitleScreen, LoadManager.FadeType.ToBlack, 2f, true);
                        break;
                    case SubmitActionLoadScene.LoadableScenes.CREDITS:
                        LoadManager.LoadScene(OWScene.Credits_Fast, LoadManager.FadeType.ToBlack, 1f, false);
                        break;
                }
            });
            
            return false;
        }
    }
}
