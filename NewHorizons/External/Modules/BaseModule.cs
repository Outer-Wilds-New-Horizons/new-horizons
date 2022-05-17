using NewHorizons.Utility;
namespace NewHorizons.External.Modules
{
    public class BaseModule : Module
    {
        public bool HasMapMarker { get; set; }
        public bool HasAmbientLight { get; set; }
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


        // Obsolete
        public bool IsSatellite { get; set; }
        public float BlackHoleSize { get; set; } 
        public float LavaSize { get; set; } 
        public float WaterSize { get; set; } 
        public MColor WaterTint { get; set; } 
    }
}
