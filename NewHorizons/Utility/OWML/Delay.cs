using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Utility.OWML
{
    public static class Delay
    {
        #region OnSceneUnloaded
        static Delay() => SceneManager.sceneUnloaded += OnSceneUnloaded;

        private static void OnSceneUnloaded(Scene _) => Main.Instance.StopAllCoroutines();
        #endregion

        #region public methods
        public static void StartCoroutine(IEnumerator coroutine) => Main.Instance.StartCoroutine(coroutine);

        public static void RunWhen(Func<bool> predicate, Action action) => StartCoroutine(RunWhenCoroutine(action, predicate));

        public static void FireInNUpdates(Action action, int n) => StartCoroutine(FireInNUpdatesCoroutine(action, n));

        public static void FireOnNextUpdate(Action action) => FireInNUpdates(action, 1);

        public static void RunWhenOrInNUpdates(Action action, Func<bool> predicate, int n) => Delay.StartCoroutine(RunWhenOrInNUpdatesCoroutine(action, predicate, n));
        public static void RunWhenAndInNUpdates(Action action, Func<bool> predicate, int n) => Delay.StartCoroutine(RunWhenAndInNUpdatesCoroutine(action, predicate, n));
        #endregion

        #region Coroutines
        private static IEnumerator RunWhenCoroutine(Action action, Func<bool> predicate)
        {
            while (!predicate.Invoke())
            {
                yield return new WaitForFixedUpdate();
            }

            action.Invoke();
        }

        private static IEnumerator FireInNUpdatesCoroutine(Action action, int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            action?.Invoke();
        }

        private static IEnumerator RunWhenOrInNUpdatesCoroutine(Action action, Func<bool> predicate, int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return new WaitForFixedUpdate();
            }
            while (!predicate.Invoke())
            {
                yield return new WaitForFixedUpdate();
            }

            action.Invoke();
        }

        private static IEnumerator RunWhenAndInNUpdatesCoroutine(Action action, Func<bool> predicate, int n)
        {
            while (!predicate.Invoke())
            {
                yield return new WaitForFixedUpdate();
            }
            for (int i = 0; i < n; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            action.Invoke();
        }
        #endregion
    }
}
