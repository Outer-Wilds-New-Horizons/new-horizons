using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Props.Quantum
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Obsolete]
    public enum QuantumGroupType
    {
        [EnumMember(Value = @"sockets")] Sockets = 0,

        [EnumMember(Value = @"states")] States = 1,

        FailedValidation = 10
    }
}
