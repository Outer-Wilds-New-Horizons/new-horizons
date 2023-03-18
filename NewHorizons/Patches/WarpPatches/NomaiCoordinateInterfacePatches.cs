using HarmonyLib;
using NewHorizons.Utility;

namespace NewHorizons.Patches.WarpPatches
{
    [HarmonyPatch(typeof(NomaiCoordinateInterface))]
    public static class NomaiCoordinateInterfacePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NomaiCoordinateInterface.SetPillarRaised), new System.Type[] { typeof(bool) })]
        public static bool NomaiCoordinateInterface_SetPillarRaised(NomaiCoordinateInterface __instance, bool raised)
        {
            if (raised)
                return !(!__instance._powered || __instance.CheckEyeCoordinates() && Main.Instance.CurrentStarSystem != "EyeOfTheUniverse" || __instance.CheckAllCoordinates(out string targetSystem) && Main.Instance.CurrentStarSystem != targetSystem);
            return true;
        }
    }
}
