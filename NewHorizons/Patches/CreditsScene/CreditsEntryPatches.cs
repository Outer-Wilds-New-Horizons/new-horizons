using HarmonyLib;
using NewHorizons.Utility;
using System;

namespace NewHorizons.Patches.CreditsScene
{
    [HarmonyPatch]
    public static class CreditsEntryPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CreditsEntry), nameof(CreditsEntry.SetContents))]
        public static bool CreditsEntry_SetContents(CreditsEntry __instance, string[] __0)
        {
            var columnTexts = __0;

            for (int i = 0; i < __instance._columns.Length; i++)
            {
                // Base method throws out of bounds exception sometimes (_columns length doesn't match columnTexts length)
                // Trim also NREs sometimes because of the TrimHelper class Mobius has idk
                try
                {
                    __instance._columns[i].text = columnTexts[i].Trim();
                }
                catch 
                {
                    // Error occurs when column 2 is empty
                    __instance._columns[i].text = " ";
                }
            }
            return false;
        }
    }
}
