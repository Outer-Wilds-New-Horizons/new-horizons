using System;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class CloakModule
    {
        /// <summary>
        /// Radius of the cloaking field around the planet. For the Stranger this is 3000
        /// </summary>
        public float radius;

        /// <summary>
        /// Not sure what this is. For the Stranger it is 2000. Optional (will default to be proportional to the cloak radius).
        /// </summary>
        public float? cloakScaleDist;

        /// Not sure what this is. For the Stranger it is 900. Optional (will default to be proportional to the cloak radius).
        public float? innerCloakRadius;

        /// Not sure what this is. For the Stranger it is 800. Optional (will default to be proportional to the cloak radius).
        public float? nearCloakRadius;

        /// Not sure what this is. For the Stranger it is 500. Optional (will default to be proportional to the cloak radius).
        public float? farCloakRadius;

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