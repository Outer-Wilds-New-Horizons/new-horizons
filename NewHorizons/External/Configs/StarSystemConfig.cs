namespace NewHorizons.External.Configs
{
    public class StarSystemConfig
    {
        public bool canEnterViaWarpDrive = true;
        public bool startHere = false;
        public bool destroyStockPlanets = true;
        public string factRequiredForWarp;
        public bool enableTimeLoop = true;
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
