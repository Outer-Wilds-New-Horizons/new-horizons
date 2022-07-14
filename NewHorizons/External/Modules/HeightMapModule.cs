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
        /// </summary>
        [Range(0, int.MaxValue)] [DefaultValue(51)]
        public int resolution = 51;
    }
}