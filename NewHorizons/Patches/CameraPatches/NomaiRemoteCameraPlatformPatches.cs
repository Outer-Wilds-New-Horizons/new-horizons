using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.CameraPatches
{
    [HarmonyPatch(typeof(NomaiRemoteCameraPlatform))]
    public static class NomaiRemoteCameraPlatformPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NomaiRemoteCameraPlatform.IDToPlanetString))]
        public static bool NomaiRemoteCameraPlatform_IDToPlanetString(NomaiRemoteCameraPlatform.ID id, out string __result)
        {
            __result = id switch
            {
                NomaiRemoteCameraPlatform.ID.None => "None",
                NomaiRemoteCameraPlatform.ID.SunStation => UITextLibrary.GetString(UITextType.LocationSS),
                NomaiRemoteCameraPlatform.ID.HGT_TimeLoop => UITextLibrary.GetString(UITextType.LocationTT),
                NomaiRemoteCameraPlatform.ID.TH_Mine => UITextLibrary.GetString(UITextType.LocationTH),
                NomaiRemoteCameraPlatform.ID.THM_EyeLocator => UITextLibrary.GetString(UITextType.LocationTHMoon),
                NomaiRemoteCameraPlatform.ID.BH_Observatory or NomaiRemoteCameraPlatform.ID.BH_GravityCannon or NomaiRemoteCameraPlatform.ID.BH_QuantumFragment or NomaiRemoteCameraPlatform.ID.BH_BlackHoleForge or NomaiRemoteCameraPlatform.ID.BH_NorthPole => UITextLibrary.GetString(UITextType.LocationBH),
                NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland1 or NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland2 or NomaiRemoteCameraPlatform.ID.GD_StatueIsland => UITextLibrary.GetString(UITextType.LocationGD),
                NomaiRemoteCameraPlatform.ID.GD_ProbeCannonSunkenModule => UITextLibrary.GetString(UITextType.LocationOPC_Module3),
                NomaiRemoteCameraPlatform.ID.GD_ProbeCannonDamagedModule => UITextLibrary.GetString(UITextType.LocationOPC_Module2),
                NomaiRemoteCameraPlatform.ID.GD_ProbeCannonIntactModule => UITextLibrary.GetString(UITextType.LocationOPC_Module1),
                NomaiRemoteCameraPlatform.ID.VM_Interior => UITextLibrary.GetString(UITextType.LocationBHMoon),
                NomaiRemoteCameraPlatform.ID.HGT_TLE => UITextLibrary.GetString(UITextType.LocationCT),
                _ => RemoteHandler.GetPlatformIDName(id),
            };
            return false;
        }
    }
}
