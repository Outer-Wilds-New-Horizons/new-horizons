using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class AudioVolumeBuilder
    {
        public static AudioVolume Make(GameObject planetGO, Sector sector, AudioVolumeInfo info, IModBehaviour mod)
        {
            var go = GeneralPropBuilder.MakeNew("AudioVolume", planetGO, ref sector, info);
            go.layer = Layer.AdvancedEffectVolume;

            var audioSource = go.AddComponent<AudioSource>();

            var owAudioSource = go.AddComponent<OWAudioSource>();
            owAudioSource._audioSource = audioSource;
            owAudioSource.loop = info.loop;
            owAudioSource.SetMaxVolume(info.volume);
            owAudioSource.SetClipSelectionType(info.clipSelection.ConvertToOW());
            owAudioSource.SetTrack(info.track.ConvertToOW());
            AudioUtilities.SetAudioClip(owAudioSource, info.audio, mod);

            var audioVolume = PriorityVolumeBuilder.MakeExisting<AudioVolume>(go, planetGO, ref sector, info);

            audioVolume._layer = info.layer;
            audioVolume.SetPriority(info.priority);
            audioVolume._fadeSeconds = info.fadeSeconds;
            audioVolume._noFadeFromBeginning = info.noFadeFromBeginning;
            audioVolume._randomizePlayhead = info.randomizePlayhead;
            audioVolume._pauseOnFadeOut = info.pauseOnFadeOut;

            var owTriggerVolume = go.GetComponent<OWTriggerVolume>();
            audioVolume._triggerVolumeOverride = owTriggerVolume;

            go.SetActive(true);

            return audioVolume;
        }
    }
}
