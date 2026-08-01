using HarmonyLib;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NewHorizons.Patches;

[HarmonyPatch]
internal static class TitleScenePatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(TitleScreenAnimation), nameof(TitleScreenAnimation.Awake))]
    public static void TitleScreenAnimation_Awake(TitleScreenAnimation __instance)
    {
        if (TitleSceneHandler.reloaded)
        {
            TitleSceneHandler.reloaded = false;

            // Skip Splash on title screen reload
            TitleScreenAnimation titleScreenAnimation = __instance;
            titleScreenAnimation._fadeDuration = 0;
            titleScreenAnimation._gamepadSplash = false;
            titleScreenAnimation._introPan = false;

            TitleAnimationController titleAnimationController = GameObject.FindObjectOfType<TitleAnimationController>();
            titleAnimationController._logoFadeDelay = 0.001f;
            titleAnimationController._logoFadeDuration = 0.001f;
            titleAnimationController._optionsFadeDelay = 0.001f;
            titleAnimationController._optionsFadeDuration = 0.001f;
            titleAnimationController._optionsFadeSpacing = 0.001f;
            titleAnimationController.FadeInTitleLogo();

            // Reopen profile
            if (TitleSceneHandler.reopenProfile)
            {
                TitleSceneHandler.reopenProfile = false;
                Delay.FireOnNextUpdate(TitleSceneHandler.ReopenProfileMenu);
            }
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TitleScreenAnimation), nameof(TitleScreenAnimation.FadeInMusic))]
    public static bool TitleScreenAnimation_FadeInMusic(TitleScreenAnimation __instance)
    {
        if (!__instance._musicSource.isPlaying)
        {
            float fadeTime = TitleSceneHandler.CurrentTitleScreenConfig?.musicFadeInTime ?? 8f;
            if (fadeTime <= 0f)
            {
                __instance._musicSource.SetLocalVolume(1f);
                __instance._musicSource.Play();
                __instance._musicSource.UpdateSourceVolume();
            }
            else
                __instance._musicSource.FadeIn(fadeDuration: fadeTime, fadeFromNothing: true);
        }
        return false;
    }
}
