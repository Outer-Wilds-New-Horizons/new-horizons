using NewHorizons.External.Modules.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes.Rulesets
{
    public static class ThrustRulesetBuilder
    {
        public static ThrustRuleset Make(GameObject planetGO, Sector sector, RulesetModule.ThrustRulesetInfo info)
        {
            var volume = VolumeBuilder.Make<ThrustRuleset>(planetGO, sector, info);

            volume._thrustLimit = info.thrustLimit;
            volume._nerfJetpackBooster = info.nerfJetpackBooster;
            volume._nerfDuration = info.nerfDuration;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
