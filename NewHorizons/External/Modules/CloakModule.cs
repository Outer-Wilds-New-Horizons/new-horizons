using System;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class CloakModule
    {
        /// <summary>
        /// Radius of the cloaking field around the planet. It's a bit finicky so experiment with different values. If you
        /// don't want a cloak, leave this as 0.
        /// </summary>
        public float radius;

        [Obsolete("audioClip is deprecated, please use audio instead")]
        public string audioClip;

        [Obsolete("audioFilePath is deprecated, please use audio instead")]
        public string audioFilePath;

        /// <summary>
        /// The audio that will play when entering the cloaking field. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string audio;
    }
}