using NewHorizons.Builder.Props;
using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class DayNightAudioVolumeBuilder
    {
        public static NHDayNightAudioVolume Make(GameObject planetGO, Sector sector, DayNightAudioVolumeInfo info, IModBehaviour mod)
        {
            var go = GeneralPropBuilder.MakeNew("DayNightAudioVolume", planetGO, sector, info);
            go.layer = Layer.AdvancedEffectVolume;

            var audioVolume = go.AddComponent<NHDayNightAudioVolume>();
            audioVolume.sunName = info.sun;
            audioVolume.dayWindow = info.dayWindow;
            audioVolume.dayAudio = info.dayAudio;
            audioVolume.nightAudio = info.nightAudio;
            audioVolume.modBehaviour = mod;
            audioVolume.volume = info.volume;
            audioVolume.SetTrack(info.track.ConvertToOW());

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;

            go.SetActive(true);

            return audioVolume;
        }
    }
}
