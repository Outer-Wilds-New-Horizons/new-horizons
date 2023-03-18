using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class ThrustRulesetBuilder
    {
        public static ThrustRuleset Make(GameObject planetGO, Sector sector, RulesetModule.ThrustRulesetInfo info)
        {
            var volume = VolumeBuilder.Make<ThrustRuleset>(planetGO, sector, info);

            volume._thrustLimit = info.thrustLimit;
            volume._nerfJetpackBooster = info.nerfJetpackBooster;
            volume._nerfDuration = info.nerfDuration;

            return volume;
        }
    }
}
