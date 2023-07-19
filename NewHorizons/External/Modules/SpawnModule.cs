using NewHorizons.External.SerializableData;
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
        public class SpawnPoint : GeneralPropInfo
        {
            /// <summary>
            /// Offsets the player/ship by this local vector when spawning. Used to prevent spawning in the floor. Optional: defaults to (0, 4, 0).
            /// </summary>
            public MVector3 offset;
        }

        [JsonObject]
        public class PlayerSpawnPoint : SpawnPoint
        {
            /// <summary>
            /// If you spawn on a planet with no oxygen, you probably want to set this to true ;;)
            /// </summary>
            public bool startWithSuit;
            /// <summary>
            /// Whether this planet's spawn point is the one the player will initially spawn at, if multiple spawn points exist.
            /// </summary>
            public bool isDefault;


        }

        [JsonObject]
        public class ShipSpawnPoint : SpawnPoint
        {
        
        }
    }
}