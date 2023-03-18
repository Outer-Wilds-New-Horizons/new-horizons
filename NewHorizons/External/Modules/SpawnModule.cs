using NewHorizons.External.Props;
using NewHorizons.Utility;
using Newtonsoft.Json;
using System;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class SpawnModule
    {
        /// <summary>
        /// If you want the player to spawn on the new body, set a value for this.
        /// </summary>
        public PlayerSpawnPoint playerSpawn;

        /// <summary>
        /// Required for the system to be accessible by warp drive.
        /// </summary>
        public ShipSpawnPoint shipSpawn;

        [Obsolete("playerSpawnPoint is deprecated. Use playerSpawn.position instead")] public MVector3 playerSpawnPoint;
        [Obsolete("playerSpawnRotation is deprecated. Use playerSpawn.rotation instead")] public MVector3 playerSpawnRotation;
        [Obsolete("shipSpawnPoint is deprecated. Use shipSpawn.position instead")] public MVector3 shipSpawnPoint;
        [Obsolete("shipSpawnRotation is deprecated. Use shipSpawn.rotation instead")] public MVector3 shipSpawnRotation;
        [Obsolete("startWithSuit is deprecated. Use playerSpawn.startWithSuit instead")] public bool startWithSuit;

        [JsonObject]
        public class PlayerSpawnPoint : GeneralPropInfo {
            /// <summary>
            /// If you spawn on a planet with no oxygen, you probably want to set this to true ;;)
            /// </summary>
            public bool startWithSuit;
        }

        [JsonObject]
        public class ShipSpawnPoint : GeneralPropInfo {
        
        }
    }
}