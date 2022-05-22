using System.ComponentModel;

namespace NewHorizons.External.Configs
{
    public class StarSystemConfig
    {
        public string subtitle;

        [DefaultValue(true)] public bool canEnterViaWarpDrive = true;
        [DefaultValue(true)] public bool enableTimeLoop = true;
        [DefaultValue(true)] public bool destroyStockPlanets = true;

        public bool startHere;
        public string factRequiredForWarp;
        public bool mapRestricted;
        public NomaiCoordinates coords;
        public SkyboxConfig skybox;

        public class NomaiCoordinates
        {
            public int[] x;
            public int[] y;
            public int[] z;
        }

        public class SkyboxConfig
        {
            public string assetBundle = null;
            public string path = null;
            public bool destroyStarField = false;
        }
    }
}
