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
        public MColor color;

        [Range(0, double.MaxValue)] public float scale;

        public Material material;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum Material
        {
            [EnumMember(Value = @"default")] Default = 0,

            [EnumMember(Value = @"ice")] Ice = 1,

            [EnumMember(Value = @"quantum")] Quantum = 2
        }
    }
}