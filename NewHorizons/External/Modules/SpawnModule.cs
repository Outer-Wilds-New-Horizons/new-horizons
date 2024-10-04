using NewHorizons.External.SerializableData;
using NewHorizons.Handlers;
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

            /// <summary>
            /// Whether this planet's spawn point is the one the player/ship will initially spawn at, if multiple spawn points exist.
            /// Do not use at the same time as makeDefaultIfFactRevealed or makeDefaultIfPersistentCondition
            /// </summary>
            public bool isDefault;

            /// <summary>
            /// If the given ship log fact is revealed, this spawn point will be used
            /// Do not use at the same time as isDefault or makeDefaultIfPersistentCondition
            /// </summary>
            public string makeDefaultIfFactRevealed;

            /// <summary>
            /// If the given persistent condition is true, this spawn point will be used
            /// Do not use at the same time as isDefault or makeDefaultIfFactRevealed
            /// </summary>
            public string makeDefaultIfPersistentCondition;

            public bool IsDefault()
            {
                if (!string.IsNullOrEmpty(makeDefaultIfFactRevealed) && ShipLogHandler.KnowsFact(makeDefaultIfFactRevealed))
                {
                    return true;
                }
                if (!string.IsNullOrEmpty(makeDefaultIfPersistentCondition) && PlayerData.GetPersistentCondition(makeDefaultIfPersistentCondition))
                {
                    return true;
                }
                else
                {
                    return isDefault;
                }
            }
        }

        [JsonObject]
        public class PlayerSpawnPoint : SpawnPoint
        {
            /// <summary>
            /// If you spawn on a planet with no oxygen, you probably want to set this to true ;;)
            /// </summary>
            public bool startWithSuit;
        }

        [JsonObject]
        public class ShipSpawnPoint : SpawnPoint
        {
        
        }
    }
}