using NewHorizons.OtherMods;
using UnityEngine;

namespace NewHorizons.Utility.OuterWilds
{
    public static class TimeLoopUtilities
    {
        public const float LOOP_DURATION_IN_SECONDS = TimeLoop.LOOP_DURATION_IN_MINUTES * 60;
        public static void SetLoopDuration(float minutes)
        {
            TimeLoop._loopDuration = minutes * 60f;

            // If slow time mod is on give them at least an hour
            // This won't slow down time based events like sand sizes but oh well
            if (OtherModUtil.IsEnabled("dnlwtsn.SlowTime"))
            {
                TimeLoop._loopDuration = Mathf.Max(TimeLoop._loopDuration, 60f * 60f);
            }
        }
        public static void SetSecondsElapsed(float secondsElapsed) => TimeLoop._timeOffset = secondsElapsed - Time.timeSinceLevelLoad;
        public static void SetMinutesRemaining(float minutes) => TimeLoop.SetSecondsRemaining(minutes * 60);
        public static float GetMinutesRemaining() => TimeLoop.GetSecondsRemaining() / 60f;
        public static float GetVanillaSecondsRemaining() => LOOP_DURATION_IN_SECONDS - TimeLoop.GetSecondsElapsed();
        public static float GetVanillaMinutesRemaining() => GetVanillaSecondsRemaining() / 60f;
        public static float GetVanillaFractionElapsed() => TimeLoop.GetSecondsElapsed() / LOOP_DURATION_IN_SECONDS;
        public static void SetSecondsRemainingIfLessThan(float seconds)
        {
            if (TimeLoop.GetSecondsRemaining() >= seconds) return;
            TimeLoop.SetSecondsRemaining(seconds);
        }
        public static void SetMinutesRemainingIfLessThan(float minutes) => SetSecondsRemainingIfLessThan(minutes * 60);
    }
}
