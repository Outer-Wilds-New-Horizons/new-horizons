using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.StreamingPatches
{
    [HarmonyPatch(typeof(StreamingManager))]
    public static class StreamingManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StreamingManager.UnloadStreamingAssets))]
        public static bool StreamingManager_UnloadStreamingAssets(string assetBundleName)
        {
            // Only let it unload stuff that isn't being used
            return !StreamingHandler.IsBundleInUse(assetBundleName);
        }
    }
}
