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
        /// Can pick a preset material with a texture from the base game. Does not work with color.
        /// </summary>
        public Material material;

        /// <summary>
        /// Can use a custom texture. Does not work with material or color.
        /// </summary>
        public string texturePath;

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