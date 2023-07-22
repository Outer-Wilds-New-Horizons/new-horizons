using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Audio
{
    [JsonObject]
    public abstract class BaseAudioInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// The audio to use. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string audio;

        /// <summary>
        /// At this distance the sound is at its loudest.
        /// </summary>
        public float minDistance;

        /// <summary>
        /// The sound will drop off by this distance (Note: for signals, this only effects when it is heard aloud and not via the signalscope).
        /// </summary>
        [DefaultValue(5f)] public float maxDistance = 30f;

        /// <summary>
        /// How loud the sound will play
        /// </summary>
        [DefaultValue(0.5f)] public float volume = 0.5f;
    }
}
