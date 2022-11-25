using NewHorizons.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        /// Add audio volumes to this planet.
        /// </summary>
        public AudioVolumeInfo[] audioVolumes;

        /// <summary>
        /// Add destruction volumes to this planet.
        /// </summary>
        public DestructionVolumeInfo[] destructionVolumes;

        /// <summary>
        /// Add fluid volumes to this planet.
        /// </summary>
        public FluidVolumeInfo[] fluidVolumes;

        /// <summary>
        /// Add hazard volumes to this planet.
        /// </summary>
        public HazardVolumeInfo[] hazardVolumes;

        /// <summary>
        /// Add interference volumes to this planet.
        /// </summary>
        public VolumeInfo[] interferenceVolumes;

        /// <summary>
        /// Add insulating volumes to this planet. These will stop electricty hazard volumes from affecting you (just like the jellyfish).
        /// </summary>
        public VolumeInfo[] insulatingVolumes;

        /// <summary>
        /// Add map restriction volumes to this planet.
        /// </summary>
        public VolumeInfo[] mapRestrictionVolumes;

        /// <summary>
        /// Add notification volumes to this planet.
        /// </summary>
        public NotificationVolumeInfo[] notificationVolumes;

        /// <summary>
        /// Add oxygen volumes to this planet.
        /// </summary>
        public OxygenVolumeInfo[] oxygenVolumes;

        /// <summary>
        /// Add probe-specific volumes to this planet.
        /// </summary>
        public ProbeModule probe;

        /// <summary>
        /// Add triggers that reveal parts of the ship log on this planet.
        /// </summary>
        public RevealVolumeInfo[] revealVolumes;

        /// <summary>
        /// Add reverb volumes to this planet. Great for echoes in caves.
        /// </summary>
        public VolumeInfo[] reverbVolumes;

        /// <summary>
        /// Add zero-gravity volumes to this planet. 
        /// Good for surrounding planets which are using a static position to stop the player being pulled away.
        /// </summary>
        public PriorityVolumeInfo[] zeroGravityVolumes;

        [JsonObject]
        public class VolumeInfo
        {
            /// <summary>
            /// The location of this volume. Optional (will default to 0,0,0).
            /// </summary>
            public MVector3 position;

            /// <summary>
            /// The radius of this volume.
            /// </summary>
            [DefaultValue(1f)]
            public float radius = 1f;

            /// <summary>
            /// The relative path from the planet to the parent of this object. Optional (will default to the root sector).
            /// </summary>
            public string parentPath;

            /// <summary>
            /// An optional rename of this volume.
            /// </summary>
            public string rename;
        }

        [JsonObject]
        public class PriorityVolumeInfo : VolumeInfo
        {
            /// <summary>
            /// The layer of this volume.
            /// </summary>
            [DefaultValue(0)]
            public int layer = 0;

            /// <summary>
            /// The priority for this volume's effects to be applied. 
            /// Ex, a player in a gravity volume with priority 0, and zero-gravity volume with priority 1, will feel zero gravity.
            /// </summary>
            [DefaultValue(1)]
            public int priority = 1;
        }

        [JsonObject]
        public class RevealVolumeInfo : VolumeInfo
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public enum RevealVolumeType
            {
                [EnumMember(Value = @"enter")] Enter = 0,

                [EnumMember(Value = @"observe")] Observe = 1,

                [EnumMember(Value = @"snapshot")] Snapshot = 2
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum EnterType
            {
                [EnumMember(Value = @"both")] Both = 0,

                [EnumMember(Value = @"player")] Player = 1,

                [EnumMember(Value = @"probe")] Probe = 2
            }

            /// <summary>
            /// The max view angle (in degrees) the player can see the volume with to unlock the fact (`observe` only)
            /// </summary>
            [DefaultValue(180f)]
            public float maxAngle = 180f; // Observe Only

            /// <summary>
            /// The max distance the user can be away from the volume to reveal the fact (`snapshot` and `observe` only)
            /// </summary>
            [DefaultValue(-1f)]
            public float maxDistance = -1f; // Snapshot & Observe Only

            /// <summary>
            /// What needs to be done to the volume to unlock the facts
            /// </summary>
            [DefaultValue("enter")] public RevealVolumeType revealOn = RevealVolumeType.Enter;

            /// <summary>
            /// What can enter the volume to unlock the facts (`enter` only)
            /// </summary>
            [DefaultValue("both")] public EnterType revealFor = EnterType.Both;

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
        public class AudioVolumeInfo : VolumeInfo
        {
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

            /// <summary>
            /// The loudness of the audio
            /// </summary>
            [Range(0f, 1f)]
            [DefaultValue(1f)]
            public float volume = 1f;
        }

        [JsonObject]
        public class NotificationVolumeInfo : VolumeInfo
        {
            /// <summary>
            /// What the notification will show for.
            /// </summary>
            [DefaultValue("all")] public NotificationTarget target = NotificationTarget.All;

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

        [JsonObject]
        public class HazardVolumeInfo : VolumeInfo
        {
            /// <summary>
            /// The type of hazard for this volume.
            /// </summary>
            [DefaultValue("general")] public HazardType type = HazardType.GENERAL;

            /// <summary>
            /// The amount of damage you will take per second while inside this volume.
            /// </summary>
            [DefaultValue(10f)] public float damagePerSecond = 10f;

            /// <summary>
            /// The type of damage you will take when you first touch this volume.
            /// </summary>
            [DefaultValue("impact")] public InstantDamageType firstContactDamageType = InstantDamageType.Impact;

            /// <summary>
            /// The amount of damage you will take when you first touch this volume.
            /// </summary>
            public float firstContactDamage;

            [JsonConverter(typeof(StringEnumConverter))]
            public enum HazardType
            {
                [EnumMember(Value = @"none")] NONE = 0,
                [EnumMember(Value = @"general")] GENERAL = 1,
                [EnumMember(Value = @"ghostMatter")] DARKMATTER = 2,
                [EnumMember(Value = @"heat")] HEAT = 4,
                [EnumMember(Value = @"fire")] FIRE = 8,
                [EnumMember(Value = @"sandfall")] SANDFALL = 16,
                [EnumMember(Value = @"electricity")] ELECTRICITY = 32,
                [EnumMember(Value = @"rapids")] RAPIDS = 64
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public enum InstantDamageType
            {
                [EnumMember(Value = @"impact")] Impact,
                [EnumMember(Value = @"puncture")] Puncture,
                [EnumMember(Value = @"electrical")] Electrical
            }
        }

        [JsonObject]
        public class VanishVolumeInfo : VolumeInfo
        {
            /// <summary>
            /// Whether the bodies will shrink when they enter this volume or just disappear instantly.
            /// </summary>
            [DefaultValue(true)] public bool shrinkBodies = true;

            /// <summary>
            /// Whether this volume only affects the player and ship.
            /// </summary>
            public bool onlyAffectsPlayerAndShip;
        }

        [JsonObject]
        public class DestructionVolumeInfo : VanishVolumeInfo
        {
            /// <summary>
            /// The type of death the player will have if they enter this volume.
            /// </summary>
            [DefaultValue("default")] public DeathType deathType = DeathType.Default;

            [JsonConverter(typeof(StringEnumConverter))]
            public enum DeathType
            {
                [EnumMember(Value = @"default")] Default,
                [EnumMember(Value = @"impact")] Impact,
                [EnumMember(Value = @"asphyxiation")] Asphyxiation,
                [EnumMember(Value = @"energy")] Energy,
                [EnumMember(Value = @"supernova")] Supernova,
                [EnumMember(Value = @"digestion")] Digestion,
                [EnumMember(Value = @"bigBang")] BigBang,
                [EnumMember(Value = @"crushed")] Crushed,
                [EnumMember(Value = @"meditation")] Meditation,
                [EnumMember(Value = @"timeLoop")] TimeLoop,
                [EnumMember(Value = @"lava")] Lava,
                [EnumMember(Value = @"blackHole")] BlackHole,
                [EnumMember(Value = @"dream")] Dream,
                [EnumMember(Value = @"dreamExplosion")] DreamExplosion,
                [EnumMember(Value = @"crushedByElevator")] CrushedByElevator
            }
        }

        [JsonObject]
        public class OxygenVolumeInfo : VolumeInfo
        {
            /// <summary>
            /// Does this volume contain trees? This will change the notification from "Oxygen tank refilled" to "Trees detected, oxygen tank refilled".
            /// </summary>
            public bool treeVolume;

            /// <summary>
            /// Whether to play the oxygen tank refill sound or just fill quietly.
            /// </summary>
            [DefaultValue(true)] public bool playRefillAudio = true;
        }

        [JsonObject]
        public class FluidVolumeInfo : PriorityVolumeInfo
        {
            /// <summary>
            /// Density of the fluid. The higher the density, the harder it is to go through this fluid.
            /// </summary>
            [DefaultValue(1.2f)] public float density = 1.2f;

            /// <summary>
            /// The type of fluid for this volume.
            /// </summary>
            public FluidType type;

            /// <summary>
            /// Should the player and rafts align to this fluid.
            /// </summary>
            [DefaultValue(true)] public bool alignmentFluid = true;

            /// <summary>
            /// Should the ship align to the fluid by rolling.
            /// </summary>
            public bool allowShipAutoroll;

            /// <summary>
            /// Disable this fluid volume immediately?
            /// </summary>
            public bool disableOnStart;

            [JsonConverter(typeof(StringEnumConverter))]
            public enum FluidType
            {
                [EnumMember(Value = @"none")] NONE = 0,
                [EnumMember(Value = @"air")] AIR,
                [EnumMember(Value = @"water")] WATER,
                [EnumMember(Value = @"cloud")] CLOUD,
                [EnumMember(Value = @"sand")] SAND,
                [EnumMember(Value = @"plasma")] PLASMA,
                [EnumMember(Value = @"fog")] FOG
            }
        }

        [JsonObject]
        public class ProbeModule
        {
            /// <summary>
            /// Add probe destruction volumes to this planet. These will delete your probe.
            /// </summary>
            public VolumeInfo[] destructionVolumes;

            /// <summary>
            /// Add probe safety volumes to this planet. These will stop the probe destruction volumes from working.
            /// </summary>
            public VolumeInfo[] safetyVolumes;
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
