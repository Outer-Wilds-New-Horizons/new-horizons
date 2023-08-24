using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class DayNightAudioVolumeInfo : PriorityVolumeInfo
    {
        /// <summary>
        /// The audio to use during the day. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list. Leave empty for no daytime audio.
        /// </summary>
        public string dayAudio;

        /// <summary>
        /// The audio to use during the day. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list. Leave empty for no nightime audio.
        /// </summary>
        public string nightAudio;

        /// <summary>
        /// The name of the astro object used to determine if it is day or night.
        /// </summary>
        public string sun;

        /// <summary>
        /// Angle in degrees defining daytime. Inside this window it will be day and outside it will be night.
        /// </summary>
        [Range(0f, 360f)]
        [DefaultValue(180f)]
        public float dayWindow = 180f;

        /// <summary>
        /// The loudness of the audio
        /// </summary>
        [Range(0f, 1f)]
        [DefaultValue(1f)]
        public float volume = 1f;

        /// <summary>
        /// The audio track of this audio volume.
        /// Most of the time you'll use environment (the default) for sound effects and music for music. 
        /// </summary>
        [DefaultValue("environment")] public NHAudioMixerTrackName track = NHAudioMixerTrackName.Environment;
    }
}
