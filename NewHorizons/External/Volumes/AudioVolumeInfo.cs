using NewHorizons.External.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NewHorizons.External.Volumes
{
    [JsonObject]
    public class AudioVolumeInfo : PriorityVolumeInfo
    {
        /// <summary>
        /// The audio to use. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string audio;

        [DefaultValue("random")] public ClipSelectionType clipSelection = ClipSelectionType.RANDOM;

        /// <summary>
        /// The audio track of this audio volume
        /// </summary>
        [DefaultValue("environment")] public AudioMixerTrackName track = AudioMixerTrackName.Environment;

        /// <summary>
        /// Whether to loop this audio while in this audio volume or just play it once
        /// </summary>
        [DefaultValue(true)] public bool loop = true;

        /// <summary>
        /// The loudness of the audio
        /// </summary>
        [Range(0f, 1f)]
        [DefaultValue(1f)]
        public float volume = 1f;

        /// <summary>
        /// How long it will take to fade this sound in and out when entering/exiting this volume.
        /// </summary>
        [DefaultValue(2f)]
        public float fadeSeconds = 2f;

        /// <summary>
        /// Play the sound instantly without any fading.
        /// </summary>
        public bool noFadeFromBeginning;

        /// <summary>
        /// Randomize what time the audio starts at.
        /// </summary>
        public bool randomizePlayhead;

        /// <summary>
        /// Pause the music when exiting the volume.
        /// </summary>
        public bool pauseOnFadeOut;


        [JsonConverter(typeof(StringEnumConverter))]
        public enum ClipSelectionType
        {
            [EnumMember(Value = @"random")] RANDOM,
            [EnumMember(Value = @"sequential")] SEQUENTIAL,
            [EnumMember(Value = @"manual")] MANUAL
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum AudioMixerTrackName
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
}
