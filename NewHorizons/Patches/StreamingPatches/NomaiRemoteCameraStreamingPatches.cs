using HarmonyLib;
using NewHorizons.Handlers;

namespace NewHorizons.Patches.StreamingPatches
{
    [HarmonyPatch(typeof(NomaiRemoteCameraStreaming))]
    public class NomaiRemoteCameraStreamingPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(NomaiRemoteCameraStreaming.NomaiRemoteCameraPlatformIDToSceneName))]
        public static bool NomaiRemoteCameraStreaming_NomaiRemoteCameraPlatformIDToSceneName(NomaiRemoteCameraPlatform.ID id, out string __result)
        {
            __result = id switch
            {
                NomaiRemoteCameraPlatform.ID.SunStation => "SolarSystem",
                NomaiRemoteCameraPlatform.ID.HGT_TimeLoop or NomaiRemoteCameraPlatform.ID.HGT_TLE => "HourglassTwins",
                NomaiRemoteCameraPlatform.ID.TH_Mine or NomaiRemoteCameraPlatform.ID.THM_EyeLocator => "TimberHearth",
                NomaiRemoteCameraPlatform.ID.BH_Observatory or NomaiRemoteCameraPlatform.ID.BH_GravityCannon or NomaiRemoteCameraPlatform.ID.BH_QuantumFragment or NomaiRemoteCameraPlatform.ID.BH_BlackHoleForge or NomaiRemoteCameraPlatform.ID.BH_NorthPole or NomaiRemoteCameraPlatform.ID.VM_Interior => "BrittleHollow",
                NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland1 or NomaiRemoteCameraPlatform.ID.GD_ConstructionYardIsland2 or NomaiRemoteCameraPlatform.ID.GD_StatueIsland or NomaiRemoteCameraPlatform.ID.GD_ProbeCannonSunkenModule or NomaiRemoteCameraPlatform.ID.GD_ProbeCannonDamagedModule or NomaiRemoteCameraPlatform.ID.GD_ProbeCannonIntactModule => "GiantsDeep",
                NomaiRemoteCameraPlatform.ID.None => "",
                _ => PlatformKeyToSceneName(id),
            };
            return false;
        }

        private static string PlatformKeyToSceneName(NomaiRemoteCameraPlatform.ID id)
        {
            var key = RemoteHandler.GetPlatformIDKey(id);
            var _ = key.IndexOf("_");
            return (_ == -1 ? key : key.Substring(0, _)) switch
            {
                "SS" => "SolarSystem",
                "HGT" or "CT" or "TT" => "HourglassTwins",
                "CO" => "Comet",
                "QM" => "QuantumMoon",
                "GD" => "GiantsDeep",
                "BH" or "VM" => "BrittleHollow",
                "TH" or "THM" => "TimberHearth",
                "DB" => "DarkBramble",
                "WH" => "WhiteHole",
                "RW" => "RingWorld",
                "DW" => "DreamWorld",
                _ => key,
            };
        }
    }
}
