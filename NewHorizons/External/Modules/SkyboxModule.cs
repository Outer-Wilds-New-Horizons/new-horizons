using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class SkyboxModule
    {
        /// <summary>
        /// Whether to destroy the star field around the player
        /// </summary>
        public bool destroyStarField;

        /// <summary>
        /// Whether to use a cube for the skybox instead of a smooth sphere
        /// </summary>
        public bool useCube;

        /// <summary>
        /// Relative filepath to the texture to use for the skybox's positive X direction
        /// </summary>
        public string rightPath;

        /// <summary>
        /// Relative filepath to the texture to use for the skybox's negative X direction
        /// </summary>
        public string leftPath;

        /// <summary>
        /// Relative filepath to the texture to use for the skybox's positive Y direction
        /// </summary>
        public string topPath;

        /// <summary>
        /// Relative filepath to the texture to use for the skybox's negative Y direction
        /// </summary>
        public string bottomPath;

        /// <summary>
        /// Relative filepath to the texture to use for the skybox's positive Z direction
        /// </summary>
        public string frontPath;

        /// <summary>
        /// Relative filepath to the texture to use for the skybox's negative Z direction
        /// </summary>
        public string backPath;
    }
}
