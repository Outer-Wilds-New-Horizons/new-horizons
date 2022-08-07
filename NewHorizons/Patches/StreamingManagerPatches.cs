using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public static class StreamingManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StreamingManager), nameof(StreamingManager.UnloadStreamingAssets))]
        public static bool StreamingManager_UnloadStreamingAssets(string assetBundleName)
        {
            // Only let it unload stuff that isn't being used
            return !StreamingHandler.IsBundleInUse(assetBundleName);
        } 
    }
}
