using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class InteractionVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The prompt to display when the volume is interacted with.
        /// </summary>
        public string prompt;

        /// <summary>
        /// The range at which the volume can be interacted with.
        /// </summary>
        [DefaultValue(2f)] public float range = 2f;

        /// <summary>
        /// The max view angle (in degrees) the player can see the volume with to interact with it. This will effectively be a cone extending from the volume's center forwards (along the Z axis) based on the volume's rotation.
        /// If not specified, no view angle restriction will be applied.
        /// </summary>
        public float? maxViewAngle;

        /// <summary>
        /// Whether the volume can be interacted with while in the ship.
        /// </summary>
        public bool usableInShip;

        /// <summary>
        /// Whether the volume can be interacted with multiple times.
        /// </summary>
        public bool reusable;

        /// <summary>
        /// The name of the dialogue condition or persistent condition to set when the volume is interacted with.
        /// </summary>
        public string condition;

        /// <summary>
        /// If true, the condition will persist across all future loops until unset.
        /// </summary>
        public bool persistent;

        /// <summary>
        /// A sound to play when the volume is interacted with. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string audio;

        /// <summary>
        /// A path to an animator component where an animation will be triggered when the volume is interacted with. 
        /// </summary>
        public string pathToAnimator;

        /// <summary>
        /// The name of an animation trigger to set on the animator when the volume is interacted with.
        /// </summary>
        public string animationTrigger;
    }
}
