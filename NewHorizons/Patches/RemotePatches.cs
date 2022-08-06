using HarmonyLib;
using NewHorizons.Components;
using NewHorizons.Handlers;
using UnityEngine;

namespace NewHorizons.Patches
{
    [HarmonyPatch]
    public class RemotePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.IDToPlanetString))]
        public static bool NomaiRemoteCameraPlatform_IDToPlanetString(NomaiRemoteCameraPlatform.ID id, out string __result)
        {
            switch (id)
            {
                case NomaiRemoteCameraPlatform.ID.None:
                    __result = "None";
                    break;
                case NomaiRemoteCameraPlatform.ID.SunStation:
                    __result = UITextLibrary.GetString(UITextType.LocationSS);
                    break;
                case NomaiRemoteCameraPlatform.ID.HGT_TimeLoop:
                    __result = UITextLibrary.GetString(UITextType.LocationTT);
                    break;
                case NomaiRemoteCameraPlatform.ID.TH_Mine:
                    __result = UITextLibrary.GetString(UITextType.LocationTH);
                    break;
                case NomaiRemoteCameraPlatform.ID.THM_EyeLocator:
                    __result = UITextLibrary.GetString(UITextType.LocationTHMoon);
                    break;
                case NomaiRemoteCameraPlatform.ID.BH_Observatory:
                case NomaiRemoteCameraPlatform.ID.BH_GravityCannon:
                case NomaiRemoteCameraPlatform.ID.BH_QuantumFragment:
                case NomaiRemoteCameraPlatform.ID.BH_BlackHoleForge:
                case NomaiRemoteCameraPlatform.ID.BH_NorthPole:
                    __result = UITextLibrary.GetString(UITextType.LocationBH);
                    break;
                case NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland1:
                case NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland2:
                case NomaiRemoteCameraPlatform.ID.GD_StatueIsland:
                    __result = UITextLibrary.GetString(UITextType.LocationGD);
                    break;
                case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonSunkenModule:
                    __result = UITextLibrary.GetString(UITextType.LocationOPC_Module3);
                    break;
                case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonDamagedModule:
                    __result = UITextLibrary.GetString(UITextType.LocationOPC_Module2);
                    break;
                case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonIntactModule:
                    __result = UITextLibrary.GetString(UITextType.LocationOPC_Module1);
                    break;
                case NomaiRemoteCameraPlatform.ID.VM_Interior:
                    __result = UITextLibrary.GetString(UITextType.LocationBHMoon);
                    break;
                case NomaiRemoteCameraPlatform.ID.HGT_TLE:
                    __result = UITextLibrary.GetString(UITextType.LocationCT);
                    break;
                default:
                    __result = RemoteHandler.GetPlatformIDName(id);
                    break;
            }
            return false;
        }
    }
}
