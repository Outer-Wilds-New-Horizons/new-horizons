using NewHorizons.Utility;
namespace NewHorizons.External.Modules
{
    public class BaseModule
    {
        public bool HasMapMarker { get; set; }
        public float AmbientLight { get; set; }
        public float SurfaceGravity { get; set; }
        public string GravityFallOff { get; set; } = "linear";
        public float SurfaceSize { get; set; }
        public float SphereOfInfluence { get; set; }
        public float GroundSize { get; set; }
        public bool HasCometTail { get; set; }
        public MVector3 CometTailRotation { get; set; }
        public bool HasReferenceFrame { get; set; } = true;
        public bool CenterOfSolarSystem { get; set; } = false;
        public float CloakRadius { get; set; } = 0f;
        public bool InvulnerableToSun { get; set; }
        public bool ShowMinimap { get; set; } = true;

        #region Obsolete
        [System.Obsolete("IsSatellite is deprecated, please use ShowMinimap instead")] public bool IsSatellite { get; set; }
        [System.Obsolete("BlackHoleSize is deprecated, please use SingularityModule instead")] public float BlackHoleSize { get; set; }
        [System.Obsolete("LavaSize is deprecated, please use LavaModule instead")] public float LavaSize { get; set; }
        [System.Obsolete("WaterTint is deprecated, please use WaterModule instead")] public float WaterSize { get; set; }
        [System.Obsolete("WaterTint is deprecated, please use WaterModule instead")] public MColor WaterTint { get; set; }
        [System.Obsolete("HasAmbientLight is deprecated, please use AmbientLight instead")] public bool HasAmbientLight { get; set; }
        #endregion Obsolete
    }
}
