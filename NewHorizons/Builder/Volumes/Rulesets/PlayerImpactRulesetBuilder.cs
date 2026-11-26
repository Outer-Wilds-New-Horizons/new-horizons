using NewHorizons.External.Modules.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes.Rulesets
{
    public static class PlayerImpactRulesetBuilder
    {
        public static PlayerImpactRuleset Make(GameObject planetGO, Sector sector, RulesetModule.PlayerImpactRulesetInfo info)
        {
            var volume = VolumeBuilder.Make<PlayerImpactRuleset>(planetGO, ref sector, info);

            volume.minImpactSpeed = info.minImpactSpeed;
            volume.maxImpactSpeed = info.maxImpactSpeed;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
