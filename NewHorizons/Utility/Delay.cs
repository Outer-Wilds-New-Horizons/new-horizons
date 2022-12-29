using System;

namespace NewHorizons.Utility
{
    public static class Delay
    {
        public static void RunWhen(Func<bool> predicate, Action action) => Main.Instance.ModHelper.Events.Unity.RunWhen(predicate, action);
        public static void FireInNUpdates(Action action, int n) => Main.Instance.ModHelper.Events.Unity.FireInNUpdates(action, n);
        public static void FireOnNextUpdate(Action action) => Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(action);
    }
}
