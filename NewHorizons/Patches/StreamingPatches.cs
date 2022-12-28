using HarmonyLib;
using NewHorizons.Handlers;
using OWML.Logging;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class StreamingPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StreamingManager), nameof(StreamingManager.UnloadStreamingAssets))]
        public static bool StreamingManager_UnloadStreamingAssets(string assetBundleName)
        {
            // Only let it unload stuff that isn't being used
            return !StreamingHandler.IsBundleInUse(assetBundleName);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnityLogger), "OnLogMessageReceived")]
        public static bool UnityLogger_OnLogMessageReceived(string message)
        {
            // Filter out goofy error that doesn't actually break anything
            return !message.Contains(" is out of bounds (size=0)");
        }
    }
}
