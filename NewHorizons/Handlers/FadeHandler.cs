using NewHorizons.Utility.OWML;
using System;
using System.Collections;
using UnityEngine;

namespace NewHorizons.Handlers
{
    /// <summary>
    /// copied from LoadManager.
    /// exists so we can do things after the fade without patching. 
    /// </summary>
    public static class FadeHandler
    {
        public static void FadeOut(float length) => Delay.StartCoroutine(FadeOutCoroutine(length, true));

        public static void FadeOut(float length, bool fadeSound) => Delay.StartCoroutine(FadeOutCoroutine(length, fadeSound));

        public static void FadeIn(float length) => Delay.StartCoroutine(FadeInCoroutine(length));

        private static IEnumerator FadeOutCoroutine(float length, bool fadeSound)
        {
            // Make sure its not already faded
            if (!LoadManager.s_instance._fadeCanvas.enabled)
            {
                LoadManager.s_instance._fadeCanvas.enabled = true;
                float startTime = Time.unscaledTime;
                float endTime = Time.unscaledTime + length;

                while (Time.unscaledTime < endTime)
                {
                    var t = Mathf.Clamp01((Time.unscaledTime - startTime) / length);
                    LoadManager.s_instance._fadeImage.color = Color.Lerp(Color.clear, Color.black, t);
                    if (fadeSound)
                    {
                        AudioListener.volume = 1f - t;
                    }
                    yield return new WaitForEndOfFrame();
                }

                LoadManager.s_instance._fadeImage.color = Color.black;
                if (fadeSound)
                {
                    AudioListener.volume = 0;
                }
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForSeconds(length);
            }
        }

        private static IEnumerator FadeInCoroutine(float length)
        {
            float startTime = Time.unscaledTime;
            float endTime = Time.unscaledTime + length;

            while (Time.unscaledTime < endTime)
            {
                var t = Mathf.Clamp01((Time.unscaledTime - startTime) / length);
                LoadManager.s_instance._fadeImage.color = Color.Lerp(Color.black, Color.clear, t);
                AudioListener.volume = t;
                yield return new WaitForEndOfFrame();
            }

            AudioListener.volume = 1;
            LoadManager.s_instance._fadeCanvas.enabled = false;
            LoadManager.s_instance._fadeImage.color = Color.clear;

            yield return new WaitForEndOfFrame();
        }

        public static void FadeThen(float length, Action action) => Delay.StartCoroutine(FadeThenCoroutine(length, action));

        private static IEnumerator FadeThenCoroutine(float length, Action action)
        {
            yield return FadeOutCoroutine(length, true);

            action?.Invoke();
        }
    }
}
