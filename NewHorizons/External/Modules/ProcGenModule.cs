using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class ProcGenModule
    {
        /// <summary>
        /// Scale height of the proc gen.
        /// </summary>
        [Range(0, double.MaxValue)] public float scale;

        /// <summary>
        /// Ground color, only applied if no texture or material is chosen.
        /// </summary>
        public MColor color;

        /// <summary>
        /// Can pick a preset material with a texture from the base game. Does not work with color or any textures.
        /// </summary>
        public Material material;

        /// <summary>
        /// Can use a custom texture. Does not work with material or color.
        /// </summary>
        public string texture;

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

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Material
        {
            [EnumMember(Value = @"default")] Default = 0,

            [EnumMember(Value = @"ice")] Ice = 1,

            [EnumMember(Value = @"quantum")] Quantum = 2,

            [EnumMember(Value = @"rock")] Rock = 3
        }
    }
}