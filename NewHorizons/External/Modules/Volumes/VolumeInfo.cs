using NewHorizons.External.Modules.Audio;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class VolumeInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// The radius of this volume.
        /// </summary>
        [DefaultValue(1f)] public float radius = 1f;
    }

    [JsonObject]
    public class AudioVolumeInfo : PriorityVolumeInfo
    {
        /// <summary>
        /// The audio to use. Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string audio;

        [DefaultValue("random")] public ClipSelectionType clipSelection = ClipSelectionType.RANDOM;

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
            [EnumMember(Value = @"rapids")] RAPIDS = 64,
            [EnumMember(Value = @"riverHeat")] RIVERHEAT = 128,
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

    [JsonObject]
    public class VisorEffectModule
    {
        /// <summary>
        /// Add visor frost effect volumes to this planet. This is the ghost matter effect.
        /// </summary>
        public FrostEffectVolumeInfo[] frostEffectVolumes;

        /// <summary>
        /// Add visor rain effect volumes to this planet. You can see this on Giant's Deep.
        /// </summary>
        public RainEffectVolumeInfo[] rainEffectVolumes;

        [JsonObject]
        public class FrostEffectVolumeInfo : PriorityVolumeInfo
        {
            /// <summary>
            /// The rate at which the frost effect will get stronger
            /// </summary>
            [DefaultValue(0.5f)]
            public float frostRate = 0.5f;

            /// <summary>
            /// The maximum strength of frost this volume can give
            /// </summary>
            [Range(0f, 1f)]
            [DefaultValue(0.91f)]
            public float maxFrost = 0.91f;
        }

        [JsonObject]
        public class RainEffectVolumeInfo : PriorityVolumeInfo
        {
            /// <summary>
            /// The rate at which the rain droplet effect will happen
            /// </summary>
            [DefaultValue(0.1f)]
            public float dropletRate = 10f;

            /// <summary>
            /// The rate at which the rain streak effect will happen
            /// </summary>
            [DefaultValue(1f)]
            public float streakRate = 1f;
        }
    }

    [JsonObject]
    public class RulesetModule
    {
        /// <summary>
        /// Add anti travel music rulesets to this planet.
        /// </summary>
        public VolumeInfo[] antiTravelMusicRulesets;
        /// <summary>
        /// Add player impact rulesets to this planet.
        /// </summary>
        public PlayerImpactRulesetInfo[] playerImpactRulesets;
        /// <summary>
        /// Add probe rulesets to this planet.
        /// </summary>
        public ProbeRulesetInfo[] probeRulesets;
        /// <summary>
        /// Add thrust rulesets to this planet.
        /// </summary>
        public ThrustRulesetInfo[] thrustRulesets;

        [JsonObject]
        public class PlayerImpactRulesetInfo : VolumeInfo
        {
            /// <summary>
            /// Minimum player impact speed. Player will take the minimum amount of damage if they impact something at this speed.
            /// </summary>
            [DefaultValue(20f)] public float minImpactSpeed = 20f;

            /// <summary>
            /// Maximum player impact speed. Players will die if they impact something at this speed.
            /// </summary>
            [DefaultValue(40f)] public float maxImpactSpeed = 40f;
        }

        [JsonObject]
        public class ProbeRulesetInfo : VolumeInfo
        {
            /// <summary>
            /// Should this ruleset override the probe's speed?
            /// </summary>
            public bool overrideProbeSpeed;

            /// <summary>
            /// The speed of the probe while in this ruleset volume.
            /// </summary>
            public float probeSpeed;

            /// <summary>
            /// Should this ruleset override the range of probe's light?
            /// </summary>
            public bool overrideLanternRange;

            /// <summary>
            /// The range of probe's light while in this ruleset volume.
            /// </summary>
            public float lanternRange;

            /// <summary>
            /// Stop the probe from attaching to anything while in this ruleset volume.
            /// </summary>
            public bool ignoreAnchor;
        }

        [JsonObject]
        public class ThrustRulesetInfo : VolumeInfo
        {
            /// <summary>
            /// Limit how fast you can fly with your ship while in this ruleset volume.
            /// </summary>
            [DefaultValue(float.PositiveInfinity)] public float thrustLimit = float.PositiveInfinity;

            /// <summary>
            /// Nerf the jetpack booster.
            /// </summary>
            public bool nerfJetpackBooster;

            /// <summary>
            /// How long the jetpack booster will be nerfed.
            /// </summary>
            [DefaultValue(0.5f)] public float nerfDuration = 0.5f;
        }
    }

    [JsonObject]
    public class SpeedTrapVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// The speed the volume will slow you down to when you enter it.
        /// </summary>
        [DefaultValue(10f)]
        public float speedLimit = 10f;

        /// <summary>
        /// How fast it will slow down the player to the speed limit.
        /// </summary>
        [DefaultValue(3f)]
        public float acceleration = 3f;
    }
}
