using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.CreditsScene
{
    [HarmonyPatch(typeof(Credits))]
    public static class CreditsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Credits.Start))]
        public static void Credits_Start(Credits __instance)
        {
            CreditsHandler.AddCredits(__instance);
        }
    }
}

