using HarmonyLib;
using NewHorizons.Components.EyeOfTheUniverse;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.MapPatches
{
    [HarmonyPatch(typeof(ReferenceFrame))]
    public static class ReferenceFramePatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ReferenceFrame.GetHUDDisplayName))]
        public static void ReferenceFrame_GetHUDDisplayName(ReferenceFrame __instance, ref string __result)
        {
            var ao = __instance.GetAstroObject();

            if (ao is NHAstroObject nhao)
            {
                if (nhao.HideDisplayName) return;

                if (nhao is EyeAstroObject || !nhao.isVanilla)
                {
                    if (nhao.TryGetTranslatedCustomName(out string translatedCustomName))
                    {
                        __result = translatedCustomName;
                    }
                }
            }
        }
    }
}
