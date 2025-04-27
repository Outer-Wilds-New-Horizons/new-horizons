using HarmonyLib;
using NewHorizons.Handlers;
using System;

namespace NewHorizons.Patches.CreditsScenePatches
{
    [HarmonyPatch(typeof(Credits))]
    public static class CreditsPatches
    {
        public static event EventHandler CreditsBuilt; // Used in NHGameOverManager to patch credits music and scroll speed
        
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Credits.Start))]
        public static void Credits_Start(Credits __instance)
        {
            CreditsHandler.AddCredits(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Credits.BuildCredits))]
        public static void Credits_BuildCredits_Post(Credits __instance)
        {
            // Do things BuildCredits() normally does

            // Fire event once finished
            CreditsBuilt?.Invoke(__instance, new EventArgs());
        }
    }
}

