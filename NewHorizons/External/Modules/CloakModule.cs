using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

        /// <summary>
        /// Name of an existing AudioClip in the game that will play when entering the cloaking field.
        /// </summary>
        public string audioClip;

        /// <summary>
        /// Relative filepath to the .wav file to use as the audio. Mutually exclusive with audioClip.
        /// </summary>
        public string audioFilePath;
    }
}