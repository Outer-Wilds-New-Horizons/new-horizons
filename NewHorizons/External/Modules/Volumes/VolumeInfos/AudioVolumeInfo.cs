using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class AudioVolumeInfo : PriorityVolumeInfo
    {
        /// <summary>
        /// The audio to use. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string audio;

        [DefaultValue("random")] public NHClipSelectionType clipSelection = NHClipSelectionType.RANDOM;

        /// <summary>
        /// The audio track of this audio volume.
        /// Most of the time you'll use environment (the default) for sound effects and music for music. 
        /// </summary>
        [DefaultValue("environment")] public NHAudioMixerTrackName track = NHAudioMixerTrackName.Environment;

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
    }

}
