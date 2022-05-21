using Newtonsoft.Json;

namespace NewHorizons.External.Configs
{
    /// <summary>
    /// Configuration for a specific star system
    /// </summary>
    [JsonObject]
    public class StarSystemConfig
    {
        /// <summary>
        /// Whether this system can be warped to via the warp drive
        /// </summary>
        public bool canEnterViaWarpDrive = true;
        
        /// <summary>
        /// Set to `true` if you want to spawn here after dying, not Timber Hearth. You can still warp back to the main star system.
        /// </summary>
        public bool startHere;
        
        /// <summary>
        /// Do you want a clean slate for this star system? Or will it be a modified version of the original.
        /// </summary>
        public bool destroyStockPlanets = true;
        
        /// <summary>
        /// Set to the FactID that must be revealed before it can be warped to. Don't set `CanEnterViaWarpDrive` to `false` if you're using this, that would make no sense.
        /// </summary>
        public string factRequiredForWarp;
        
        /// <summary>
        /// Should the player be sent back in time after 22 minutes?
        /// </summary>
        public bool enableTimeLoop = true;
        
        /// <summary>
        /// Should the player be unable to use their map in this system?
        /// </summary>
        public bool mapRestricted;
        
        /// <summary>
        /// [DEPRECATED] Not implemented
        /// </summary>
        public NomaiCoordinates coords;
        
        /// <summary>
        /// Customize the skybox for this system
        /// </summary>
        public SkyboxConfig skybox;

        public class NomaiCoordinates
        {
            public int[] x;
            public int[] y;
            public int[] z;
        }

        [JsonObject]
        public class SkyboxConfig
        {
            /// <summary>
            /// Path to the Unity asset bundle to load the skybox material from
            /// </summary>
            public string assetBundle;
            
            /// <summary>
            /// Path to the material within the asset bundle specified by `assetBundle` to use for the skybox
            /// </summary>
            public string path;
            
            /// <summary>
            /// Whether to destroy the star field around the player
            /// </summary>
            public bool destroyStarField;
        }
    }
}
