using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class ProbeRulesetBuilder
    {
        public static ProbeRuleset Make(GameObject planetGO, Sector sector, VolumesModule.RulesetModule.ProbeRulesetInfo info)
        {
            var volume = VolumeBuilder.Make<ProbeRuleset>(planetGO, sector, info);

            volume._overrideProbeSpeed = info.overrideProbeSpeed;
            volume._probeSpeedOverride = info.probeSpeed;
            volume._overrideLanternRange = info.overrideLanternRange;
            volume._lanternRange = info.lanternRange;
            volume._ignoreAnchor = info.ignoreAnchor;

            return volume;
        }
    }
}
