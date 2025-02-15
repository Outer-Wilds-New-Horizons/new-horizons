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
        public static void FadeOut(float length) => Delay.StartCoroutine(FadeOutCoroutine(length));

        private static IEnumerator FadeOutCoroutine(float length)
        {
            LoadManager.s_instance._fadeCanvas.enabled = true;
            float startTime = Time.unscaledTime;
            float endTime = Time.unscaledTime + length;

            while (Time.unscaledTime < endTime)
            {
                var t = Mathf.Clamp01((Time.unscaledTime - startTime) / length);
                LoadManager.s_instance._fadeImage.color = Color.Lerp(Color.clear, Color.black, t);
                AudioListener.volume = 1f - t;
                yield return new WaitForEndOfFrame();
            }

            LoadManager.s_instance._fadeImage.color = Color.black;
            AudioListener.volume = 0;
            yield return new WaitForEndOfFrame();
        }

        public static void FadeThen(float length, Action action) => Delay.StartCoroutine(FadeThenCoroutine(length, action));

        private static IEnumerator FadeThenCoroutine(float length, Action action)
        {
            yield return FadeOutCoroutine(length);

            action?.Invoke();
        }
    }
}
