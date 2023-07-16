using System;
using System.Collections;
using UnityEngine;

namespace NewHorizons.Utility.OWML
{
    public static class Delay
    {
        public static void RunWhen(Func<bool> predicate, Action action) => Main.Instance.ModHelper.Events.Unity.RunWhen(predicate, action);
        public static void FireInNUpdates(Action action, int n) => Main.Instance.ModHelper.Events.Unity.FireInNUpdates(action, n);
        public static void FireOnNextUpdate(Action action) => Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(action);
        public static void RunWhenAndInNUpdates(Action action, Func<bool> predicate, int n) => Main.Instance.StartCoroutine(RunWhenOrInNUpdatesCoroutine(action, predicate, n));

        private static IEnumerator RunWhenOrInNUpdatesCoroutine(Action action, Func<bool> predicate, int n)
        {
            for (int i = 0; i < n; i++)
            {
                yield return new WaitForEndOfFrame();
            }
            while (!predicate.Invoke())
            {
                yield return new WaitForEndOfFrame();
            }

            action.Invoke();
        }
    }
}
