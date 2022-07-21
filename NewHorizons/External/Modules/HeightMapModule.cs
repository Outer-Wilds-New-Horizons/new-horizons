using NewHorizons.Utility;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class HeightMapModule
    {
        /// <summary>
        /// Relative filepath to the texture used for the terrain height.
        /// </summary>
        public string heightMap;

        /// <summary>
        /// The highest points on your planet will be at this height.
        /// </summary>
        [Range(0f, double.MaxValue)] public float maxHeight;

        /// <summary>
        /// The lowest points on your planet will be at this height.
        /// </summary>
        [Range(0f, double.MaxValue)] public float minHeight;

        /// <summary>
        /// The scale of the terrain.
        /// </summary>
        public MVector3 stretch;

        /// <summary>
        /// Relative filepath to the texture used for the terrain.
        /// </summary>
        public string textureMap;

        /// <summary>
        /// Resolution of the heightmap.
        /// Higher values means more detail but also more memory/cpu/gpu usage.
        /// This value will be 1:1 with the heightmap texture width, but only at the equator.
        /// </summary>
        [Range(1 * 4, 500 * 4)]
        [DefaultValue(51 * 4)]
        public int resolution = 51 * 4;
    }
}