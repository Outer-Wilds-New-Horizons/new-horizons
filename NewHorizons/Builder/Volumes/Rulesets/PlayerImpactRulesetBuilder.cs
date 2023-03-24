using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes.Rulesets
{
    public static class PlayerImpactRulesetBuilder
    {
        public static PlayerImpactRuleset Make(GameObject planetGO, Sector sector, VolumesModule.RulesetModule.PlayerImpactRulesetInfo info)
        {
            var volume = VolumeBuilder.Make<PlayerImpactRuleset>(planetGO, sector, info);

            volume.minImpactSpeed = info.minImpactSpeed;
            volume.maxImpactSpeed = info.maxImpactSpeed;

            return volume;
        }
    }
}
