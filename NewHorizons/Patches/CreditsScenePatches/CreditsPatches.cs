using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.CreditsScenePatches
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

