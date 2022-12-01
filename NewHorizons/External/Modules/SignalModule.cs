using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NewHorizons.Utility;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class SignalModule
    {
        /// <summary>
        /// List of signals to add (Why did xen do it like this)
        /// </summary>
        public SignalInfo[] signals;

        [JsonObject]
        public class SignalInfo
        {
            [Obsolete("audioClip is deprecated, please use audio instead")]
            public string audioClip;

            [Obsolete("audioFilePath is deprecated, please use audio instead")]
            public string audioFilePath;

            /// <summary>
            /// The audio to use. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string audio;

            /// <summary>
            /// How close the player must get to the signal to detect it. This is when you get the "Unknown Signal Detected"
            /// notification.
            /// </summary>
            [Range(0f, double.MaxValue)] public float detectionRadius;

            /// <summary>
            /// The frequency ID of the signal. The built-in game values are `Default`, `Traveler`, `Quantum`, `EscapePod`,
            /// `Statue`, `WarpCore`, `HideAndSeek`, and `Radio`. You can also put a custom value.
            /// </summary>
            public string frequency;

            /// <summary>
            /// How close the player must get to the signal to identify it. This is when you learn its name.
            /// </summary>
            [DefaultValue(10f)] [Range(0f, double.MaxValue)]
            public float identificationRadius = 10f;

            /// <summary>
            /// Only set to `true` if you are putting this signal inside a cloaking field.
            /// </summary>
            public bool insideCloak;

            /// <summary>
            /// The unique ID of the signal.
            /// </summary>
            public string name;

            /// <summary>
            /// `false` if the player can hear the signal without equipping the signal-scope.
            /// </summary>
            [DefaultValue(true)] public bool onlyAudibleToScope = true;

            /// <summary>
            /// Position of the signal's source
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// A ship log fact to reveal when the signal is identified.
            /// </summary>
            [DefaultValue("")] public string reveals = "";

            /// <summary>
            /// Radius of the sphere giving off the signal.
            /// </summary>
            [DefaultValue(1f)] public float sourceRadius = 1f;

            /// <summary>
            /// The relative path from the planet to the parent of this signal. Optional (will default to the root sector).
            /// </summary>
            public string parentPath;

            /// <summary>
            /// Whether the positional and rotational coordinates are relative to parent instead of the root planet object.
            /// </summary>
            public bool isRelativeToParent;
        }
    }
}