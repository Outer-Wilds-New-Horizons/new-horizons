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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NomaiRemoteCameraStreaming), nameof(NomaiRemoteCameraStreaming.NomaiRemoteCameraPlatformIDToSceneName))]
        public static bool NomaiRemoteCameraStreaming_NomaiRemoteCameraPlatformIDToSceneName(NomaiRemoteCameraPlatform.ID id, out string __result)
        {
            switch (id)
            {
                case NomaiRemoteCameraPlatform.ID.SunStation:
                    __result = "SolarSystem";
                    break;
                case NomaiRemoteCameraPlatform.ID.HGT_TimeLoop:
                case NomaiRemoteCameraPlatform.ID.HGT_TLE:
                    __result = "HourglassTwins";
                    break;
                case NomaiRemoteCameraPlatform.ID.TH_Mine:
                case NomaiRemoteCameraPlatform.ID.THM_EyeLocator:
                    __result = "TimberHearth";
                    break;
                case NomaiRemoteCameraPlatform.ID.BH_Observatory:
                case NomaiRemoteCameraPlatform.ID.BH_GravityCannon:
                case NomaiRemoteCameraPlatform.ID.BH_QuantumFragment:
                case NomaiRemoteCameraPlatform.ID.BH_BlackHoleForge:
                case NomaiRemoteCameraPlatform.ID.BH_NorthPole:
                case NomaiRemoteCameraPlatform.ID.VM_Interior:
                    __result = "BrittleHollow";
                    break;
                case NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland1:
                case NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland2:
                case NomaiRemoteCameraPlatform.ID.GD_StatueIsland:
                case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonSunkenModule:
                case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonDamagedModule:
                case NomaiRemoteCameraPlatform.ID.GD_ProbeCannonIntactModule:
                    __result = "GiantsDeep";
                    break;
                case NomaiRemoteCameraPlatform.ID.None:
                    __result = "";
                    break;
                default:
                    var key = RemoteHandler.GetPlatformIDKey(id);
                    switch (key.Substring(0, key.IndexOf("_")))
                    {
                        case "SS":
                            __result = "SolarSystem";
                            break;
                        case "HGT":
                        case "CT":
                        case "TT":
                            __result = "HourglassTwins";
                            break;
                        case "CO":
                            __result = "Comet";
                            break;
                        case "QM":
                            __result = "QuantumMoon";
                            break;
                        case "GD":
                            __result = "GiantsDeep";
                            break;
                        case "BH":
                        case "VM":
                            __result = "BrittleHollow";
                            break;
                        case "TH":
                        case "THM":
                            __result = "TimberHearth";
                            break;
                        case "DB":
                            __result = "DarkBramble";
                            break;
                        case "WH":
                            __result = "WhiteHole";
                            break;
                        case "RW":
                            __result = "RingWorld";
                            break;
                        case "DW":
                            __result = "DreamWorld";
                            break;
                        default:
                            __result = key;
                            break;
                    }
                    break;
            }
            return false;
        }
    }
}
