using NewHorizons.External.Modules.Volumes.VolumeInfos;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Volumes
{
    [JsonObject]
    public class RulesetModule
    {
        /// <summary>
        /// Add anti travel music rulesets to this planet.
        /// This means no space/traveling music while inside the ruleset/volume.
        /// Usually used on planets.
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

}
