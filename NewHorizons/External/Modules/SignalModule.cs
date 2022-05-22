using NewHorizons.Utility;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class SignalModule 
    {
        /// <summary>
        /// List of signals to add (Why did xen do it like this)
        /// </summary>
        public SignalInfo[] Signals;

        [JsonObject]
        public class SignalInfo
        {
            /// <summary>
            /// Position of the signal's source
            /// </summary>
            public MVector3 Position;
            
            /// <summary>
            /// The frequency ID of the signal. The built-in game values are `Default`, `Traveler`, `Quantum`, `EscapePod`, `Statue`, `WarpCore`, `HideAndSeek`, and `Radio`. You can also put a custom value.
            /// </summary>
            public string Frequency;
            
            /// <summary>
            /// The unique ID of the signal.
            /// </summary>
            public string Name;
            
            /// <summary>
            /// Name of an existing AudioClip in the game that will player over the signal.
            /// </summary>
            public string AudioClip;
            
            /// <summary>
            /// Relative filepath to the .wav file to use as the audio. Mutually exclusive with audioClip.
            /// </summary>
            public string AudioFilePath;
            
            /// <summary>
            /// A ship log fact to reveal when the signal is identified.
            /// </summary>
            [DefaultValue("")]
            public string Reveals = "";
            
            /// <summary>
            /// Radius of the sphere giving off the signal.
            /// </summary>
            [DefaultValue(1f)]
            public float SourceRadius = 1f;
            
            /// <summary>
            /// How close the player must get to the signal to detect it. This is when you get the "Unknown Signal Detected" notification.
            /// </summary>
            public float DetectionRadius;
            
            /// <summary>
            /// How close the player must get to the signal to identify it. This is when you learn its name.
            /// </summary>
            [DefaultValue(10f)]
            public float IdentificationRadius = 10f;
            
            /// <summary>
            /// `false` if the player can hear the signal without equipping the signal-scope.
            /// </summary>
            [DefaultValue(true)]
            public bool OnlyAudibleToScope = true;
            
            /// <summary>
            /// Only set to `true` if you are putting this signal inside a cloaking field.
            /// </summary>
            public bool InsideCloak;
        }
    }
}
