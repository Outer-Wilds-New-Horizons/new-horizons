using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class ConditionTriggerVolumeBuilder
    {
        public static ConditionTriggerVolume Make(GameObject planetGO, Sector sector, ConditionTriggerVolumeInfo info)
        {
            var volume = VolumeBuilder.Make<ConditionTriggerVolume>(planetGO, sector, info);

            volume.Condition = info.condition;
            volume.Persistent = info.persistent;
            volume.Reversible = info.reversible;
            volume.Player = info.player;
            volume.Probe = info.probe;
            volume.Ship = info.ship;

            volume.gameObject.SetActive(true);

            return volume;
        }
    }
}
