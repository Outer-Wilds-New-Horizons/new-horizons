using UnityEngine;

namespace NewHorizons.Utility
{
    public static class TimeLoopUtilities
    {
        public const float LOOP_DURATION_IN_SECONDS = TimeLoop.LOOP_DURATION_IN_MINUTES * 60;
        public static void SetLoopDuration(float minutes) => TimeLoop._loopDuration = minutes * 60f;
        public static void SetSecondsElapsed(float secondsElapsed) => TimeLoop._timeOffset = secondsElapsed - Time.timeSinceLevelLoad;
        public static float GetMinutesRemaining() => TimeLoop.GetSecondsRemaining() / 60f;
        public static float GetVanillaSecondsRemaining() => LOOP_DURATION_IN_SECONDS - TimeLoop.GetSecondsElapsed();
        public static float GetVanillaMinutesRemaining() => GetVanillaSecondsRemaining() / 60f;
        public static float GetVanillaFractionElapsed() => TimeLoop.GetSecondsElapsed() / LOOP_DURATION_IN_SECONDS;
    }
}
