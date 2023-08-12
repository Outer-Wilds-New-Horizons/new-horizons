using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.TranslatorText
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NomaiTextType
    {
        [EnumMember(Value = @"wall")] Wall,

        [EnumMember(Value = @"scroll")] Scroll,

        [EnumMember(Value = @"whiteboard")] Whiteboard,

        [EnumMember(Value = @"computer")] Computer,

        [EnumMember(Value = @"preCrashComputer")] PreCrashComputer,

        [EnumMember(Value = @"recorder")] Recorder,

        [EnumMember(Value = @"preCrashRecorder")] PreCrashRecorder,

        [EnumMember(Value = @"cairnBH")] CairnBrittleHollow,

        [EnumMember(Value = @"cairnTH")] CairnTimberHearth,

        [EnumMember(Value = @"cairnCT")] CairnEmberTwin,

        [EnumMember(Value = @"trailmarker")] Trailmarker,

        #region Obsolete

        [Obsolete("Please use CairnBrittleHollow instead")][EnumMember(Value = @"cairn")] Cairn,

        [Obsolete("Please use CairnTimberHearth instead")][EnumMember(Value = @"cairnVariant")] CairnVariant,

        #endregion
    }
}
