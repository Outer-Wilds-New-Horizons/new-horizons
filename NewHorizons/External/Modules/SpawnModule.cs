#region

using NewHorizons.Utility;
using Newtonsoft.Json;

#endregion

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class SpawnModule
    {
        /// <summary>
        /// If you want the player to spawn on the new body, set a value for this. Press `P` in game with Debug mode on to have
        /// the game log the position you're looking at to find a good value for this.
        /// </summary>
        public MVector3 playerSpawnPoint;

        /// <summary>
        /// Euler angles by which the player will be oriented.
        /// </summary>
        public MVector3 playerSpawnRotation;

        /// <summary>
        /// Required for the system to be accessible by warp drive.
        /// </summary>
        public MVector3 shipSpawnPoint;

        /// <summary>
        /// Euler angles by which the ship will be oriented.
        /// </summary>
        public MVector3 shipSpawnRotation;

        /// <summary>
        /// If you spawn on a planet with no oxygen, you probably want to set this to true ;;)
        /// </summary>
        public bool startWithSuit;
    }
}