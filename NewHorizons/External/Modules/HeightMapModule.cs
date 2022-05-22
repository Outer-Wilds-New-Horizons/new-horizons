using NewHorizons.Utility;

namespace NewHorizons.External.Modules
{
    public class HeightMapModule
    {
        /// <summary>
        /// Relative filepath to the texture used for the terrain height.
        /// </summary>
        public string heightMap;

        /// <summary>
        /// The highest points on your planet will be at this height.
        /// </summary>
        public float maxHeight;

        /// <summary>
        /// The lowest points on your planet will be at this height.
        /// </summary>
        public float minHeight;

        /// <summary>
        /// The scale of the terrain.
        /// </summary>
        public MVector3 stretch;

        /// <summary>
        /// Relative filepath to the texture used for the terrain.
        /// </summary>
        public string textureMap;
    }
}