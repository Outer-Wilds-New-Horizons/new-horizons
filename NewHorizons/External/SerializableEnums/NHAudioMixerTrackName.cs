using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace NewHorizons.External.SerializableEnums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NHAudioMixerTrackName
    {
        [EnumMember(Value = @"undefined")] Undefined = 0,
        [EnumMember(Value = @"menu")] Menu = 1,
        [EnumMember(Value = @"music")] Music = 2,
        [EnumMember(Value = @"environment")] Environment = 4,
        [EnumMember(Value = @"environmentUnfiltered")] Environment_Unfiltered = 5,
        [EnumMember(Value = @"endTimesSfx")] EndTimes_SFX = 8,
        [EnumMember(Value = @"signal")] Signal = 16,
        [EnumMember(Value = @"death")] Death = 32,
        [EnumMember(Value = @"player")] Player = 64,
        [EnumMember(Value = @"playerExternal")] Player_External = 65,
        [EnumMember(Value = @"ship")] Ship = 128,
        [EnumMember(Value = @"map")] Map = 256,
        [EnumMember(Value = @"endTimesMusic")] EndTimes_Music = 512,
        [EnumMember(Value = @"muffleWhileRafting")] MuffleWhileRafting = 1024,
        [EnumMember(Value = @"muffleIndoors")] MuffleIndoors = 2048,
        [EnumMember(Value = @"slideReelMusic")] SlideReelMusic = 4096,
    }
}
