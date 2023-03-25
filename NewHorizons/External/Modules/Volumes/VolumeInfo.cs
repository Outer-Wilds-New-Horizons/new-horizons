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
