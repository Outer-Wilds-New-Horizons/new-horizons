using NewHorizons.External.Modules.Volumes;
using UnityEngine;

namespace NewHorizons.Builder.Volumes.Rulesets
{
    public static class ProbeRulesetBuilder
    {
        public static ProbeRuleset Make(GameObject planetGO, Sector sector, RulesetModule.ProbeRulesetInfo info)
        {
            var volume = VolumeBuilder.Make<ProbeRuleset>(planetGO, ref sector, info);

            volume._overrideProbeSpeed = info.overrideProbeSpeed;
            volume._probeSpeedOverride = info.probeSpeed;
            volume._overrideLanternRange = info.overrideLanternRange;
            volume._lanternRange = info.lanternRange;
            volume._ignoreAnchor = info.ignoreAnchor;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
