using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class VolumesModule
    {
        /// <summary>
        /// Add audio volumes to this planet
        /// </summary>
        public AudioVolumeInfo[] audioVolumes;

        /// <summary>
        /// Add notification volumes to this planet
        /// </summary>
        public NotificationVolumeInfo[] notificationVolumes;

        /// <summary>
        /// Add triggers that reveal parts of the ship log on this planet
        /// </summary>
        public RevealVolumeInfo[] revealVolumes;

        [JsonObject]
        public class RevealVolumeInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum RevealVolumeType
            {
                [EnumMember(Value = @"enter")] Enter = 0,

                [EnumMember(Value = @"observe")] Observe = 1,

                [EnumMember(Value = @"snapshot")] Snapshot = 2
            }

            /// <summary>
            /// The max view angle (in degrees) the player can see the volume with to unlock the fact (`observe` only)
            /// </summary>
            public float maxAngle = 180f; // Observe Only

            /// <summary>
            /// The max distance the user can be away from the volume to reveal the fact (`snapshot` and `observe` only)
            /// </summary>
            public float maxDistance = -1f; // Snapshot & Observe Only

            /// <summary>
            /// The position to place this volume at
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The radius of this reveal volume
            /// </summary>
            public float radius = 1f;

            /// <summary>
            /// What needs to be done to the volume to unlock the facts
            /// </summary>
            [DefaultValue("enter")] public RevealVolumeType revealOn = RevealVolumeType.Enter;

            /// <summary>
            /// A list of facts to reveal
            /// </summary>
            public string[] reveals;

            /// <summary>
            /// An achievement to unlock. Optional.
            /// </summary>
            public string achievementID;
        }

        [JsonObject]
        public class AudioVolumeInfo
        {
            /// <summary>
            /// The location of this audio volume. Optional (will default to 0,0,0).
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The radius of this audio volume
            /// </summary>
            public float radius;

            /// <summary>
            /// The audio to use. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
            /// </summary>
            public string audio;

            /// <summary>
            /// The audio track of this audio volume
            /// </summary>
            [DefaultValue("environment")] public AudioMixerTrackName track = AudioMixerTrackName.Environment;

            /// <summary>
            /// Whether to loop this audio while in this audio volume or just play it once
            /// </summary>
            [DefaultValue(true)] public bool loop = true;
        }

        [JsonObject]
        public class NotificationVolumeInfo
        {
            /// <summary>
            /// What the notification will show for.
            /// </summary>
            [DefaultValue("all")] public NotificationTarget target = NotificationTarget.All;

            /// <summary>
            /// The location of this notification volume. Optional (will default to 0,0,0).
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The radius of this notification volume.
            /// </summary>
            public float radius;

            /// <summary>
            /// The notification that will play when you enter this volume.
            /// </summary>
            public NotificationInfo entryNotification;

            /// <summary>
            /// The notification that will play when you exit this volume.
            /// </summary>
            public NotificationInfo exitNotification;


            [JsonObject]
            public class NotificationInfo
            {
                /// <summary>
                /// The message that will be displayed.
                /// </summary>
                public string displayMessage;

                /// <summary>
                /// The duration this notification will be displayed.
                /// </summary>
                [DefaultValue(5f)] public float duration = 5f;
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum NotificationTarget
            {
                [EnumMember(Value = @"all")] All = 0,
                [EnumMember(Value = @"ship")] Ship = 1,
                [EnumMember(Value = @"player")] Player = 2,
            }
        }
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
