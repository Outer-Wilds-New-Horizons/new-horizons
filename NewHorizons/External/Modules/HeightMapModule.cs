using NewHorizons.External.SerializableData;
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
        /// Resolution of the heightmap.
        /// Higher values means more detail but also more memory/cpu/gpu usage.
        /// This value will be 1:1 with the heightmap texture width, but only at the equator.
        /// </summary>
        [Range(1 * 4, 500 * 4)]
        [DefaultValue(51 * 4)]
        public int resolution = 51 * 4;

        /// <summary>
        /// Relative filepath to the texture used for the terrain colors.
        /// </summary>
        public string textureMap;

        /// <summary>
        /// Relative filepath to the texture used for the terrain's smoothness and metallic, which are controlled by the texture's alpha and red channels respectively. Optional.
        /// Typically black with variable transparency, when metallic isn't wanted.
        /// </summary>
        public string smoothnessMap;

        /// <summary>
        /// How "glossy" the surface is, where 0 is diffuse, and 1 is like a mirror.
        /// Multiplies with the alpha of the smoothness map if using one.
        /// </summary>
        [Range(0f, 1f)]
        [DefaultValue(0f)]
        public float smoothness = 0f;

        /// <summary>
        /// How metallic the surface is, from 0 to 1.
        /// Multiplies with the red of the smoothness map if using one.
        /// </summary>
        [Range(0f, 1f)]
        [DefaultValue(0f)]
        public float metallic = 0f;

        /// <summary>
        /// Relative filepath to the texture used for the normal (aka bump) map. Optional.
        /// </summary>
        public string normalMap;

        /// <summary>
        /// Strength of the normal map. Usually 0-1, but can go above, or negative to invert the map.
        /// </summary>
        [DefaultValue(1f)]
        public float normalStrength = 1f;

        /// <summary>
        /// Relative filepath to the texture used for emission. Optional.
        /// </summary>
        public string emissionMap;

        /// <summary>
        /// Color multiplier of the emission texture. Defaults to white.
        /// </summary>
        public MColor emissionColor;

        /// <summary>
        /// Relative filepath to the texture used for blending up to 5 tiles together, using the red, green, blue, and alpha channels, plus a lack of all 4 for a fifth "base" tile.
        /// Optional, even if using tiles (defaults to white, therefore either base or all other channels will be active).
        /// </summary>
        public string tileBlendMap;

        /// <summary>
        /// An optional set of textures that can tile and combine with the main maps. This tile will appear when all other tile channels are absent in the blend map, or when no other tiles are defined.
        /// Note that tiles will not be active from afar, so it is recommended to make the main textures control the general appearance, and make the tiles handle up close details.
        /// </summary>
        public HeightMapTileInfo baseTile;

        /// <summary>
        /// An optional set of textures that can tile and combine with the main maps. The distribution of this tile is controlled by red channel of the blend map.
        /// Note that tiles will not be active from afar, so it is recommended to make the main maps control the general appearance more than the tiles.
        /// </summary>
        public HeightMapTileInfo redTile;

        /// <summary>
        /// An optional set of textures that can tile and combine with the main maps. The distribution of this tile is controlled by green channel of the blend map.
        /// Note that tiles will not be active from afar, so it is recommended to make the main maps control the general appearance more than the tiles.
        /// </summary>
        public HeightMapTileInfo greenTile;

        /// <summary>
        /// An optional set of textures that can tile and combine with the main maps. The distribution of this tile is controlled by blue channel of the blend map.
        /// Note that tiles will not be active from afar, so it is recommended to make the main maps control the general appearance more than the tiles.
        /// </summary>
        public HeightMapTileInfo blueTile;

        /// <summary>
        /// An optional set of textures that can tile and combine with the main maps. The distribution of this tile is controlled by alpha channel of the blend map.
        /// Note that tiles will not be active from afar, so it is recommended to make the main maps control the general appearance more than the tiles.
        /// </summary>
        public HeightMapTileInfo alphaTile;

        [JsonObject]
        public class HeightMapTileInfo
        {
            /// <summary>
            /// The size, in meters, of each tile.
            /// </summary>
            [Range(0f, double.MaxValue)]
            [DefaultValue(1f)]
            public float size = 1f;

            /// <summary>
            /// Relative filepath to a color texture. Optional.
            /// Note that this tile texture will be multiplied with the main texture map. This means that white will multiply by 2, black by 0, and grey by 1.
            /// Thus, a texture that stays near (128, 128, 128) will blend nicely with the main texture map below.
            /// Colors other than greyscale can be used, but they might multiply strangely.
            /// </summary>
            public string textureTile;

            /// <summary>
            /// Relative filepath to a texture for smoothness and metallic, which are controlled by the texture's alpha and red channels respectively. Optional.
            /// Note that this tile texture will be multiplied with the main smoothness map and/or values. This means that black/red will multiply by 2, transparent by 0, and half transparent by 1.
            /// Thus, a texture that stays near half alpha/red will blend nicely with the main smoothness map below.
            /// </summary>
            public string smoothnessTile;

            /// <summary>
            /// Relative filepath to a normal (aka bump) texture. Optional.
            /// Blends additively with the main normal map.
            /// </summary>
            public string normalTile;

            /// <summary>
            /// Strength of the tile normal. Usually 0-1, but can go above, or negative to invert the map.
            /// </summary>
            [DefaultValue(1f)]
            public float normalStrength = 1f;
        }
    }
}