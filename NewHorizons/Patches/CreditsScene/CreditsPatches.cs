using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.CreditsScene
{
    [HarmonyPatch]
    public static class CreditsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Credits), nameof(Credits.Start))]
        public static void Credits_Start(Credits __instance)
        {
            CreditsHandler.AddCredits(__instance);
        }
    }
}

