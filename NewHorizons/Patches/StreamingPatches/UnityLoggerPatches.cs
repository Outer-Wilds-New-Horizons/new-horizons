using HarmonyLib;
using OWML.Logging;

namespace NewHorizons.Patches.StreamingPatches
{
    [HarmonyPatch(typeof(UnityLogger))]
    public static class UnityLoggerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnLogMessageReceived")]
        public static bool UnityLogger_OnLogMessageReceived(string message)
        {
            // Filter out goofy error that doesn't actually break anything
            return !message.EndsWith(") is out of bounds (size=0)");
        }
    }
}
