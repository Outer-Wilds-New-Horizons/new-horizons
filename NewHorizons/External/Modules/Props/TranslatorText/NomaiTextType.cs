using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.TranslatorText
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NomaiTextType
    {
        [EnumMember(Value = @"wall")] Wall = 0,

        [EnumMember(Value = @"scroll")] Scroll = 1,

        [EnumMember(Value = @"computer")] Computer = 2,

        [EnumMember(Value = @"cairn")] Cairn = 3,

        [EnumMember(Value = @"recorder")] Recorder = 4,

        [EnumMember(Value = @"preCrashRecorder")] PreCrashRecorder = 5,

        [EnumMember(Value = @"preCrashComputer")] PreCrashComputer = 6,

        [EnumMember(Value = @"trailmarker")] Trailmarker = 7,

        [EnumMember(Value = @"cairnVariant")] CairnVariant = 8,

        [EnumMember(Value = @"whiteboard")] Whiteboard = 9,
    }
}
