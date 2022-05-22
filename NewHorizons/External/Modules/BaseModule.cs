using NewHorizons.Utility;
using System.ComponentModel;

namespace NewHorizons.External.Modules
{
    public class BaseModule
    {
        public bool HasMapMarker { get; set; }
        public float AmbientLight { get; set; }
        public float SurfaceGravity { get; set; }

        [DefaultValue("linear")] 
        public string GravityFallOff { get; set; } = "linear";

        public float SurfaceSize { get; set; }
        public float SphereOfInfluence { get; set; }
        public float GroundSize { get; set; }
        public bool HasCometTail { get; set; }
        public MVector3 CometTailRotation { get; set; }

        [DefaultValue(true)] 
        public bool HasReferenceFrame { get; set; } = true;

        public bool CenterOfSolarSystem { get; set; } 
        public float CloakRadius { get; set; } 
        public bool InvulnerableToSun { get; set; }

        [DefaultValue(true)] 
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
