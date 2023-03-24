using NewHorizons.Builder.Props;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OWUtilities;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Volumes
{
    public static class AudioVolumeBuilder
    {
        public static AudioVolume Make(GameObject planetGO, Sector sector, VolumesModule.AudioVolumeInfo info, IModBehaviour mod)
        {
            var go = GeneralPropBuilder.MakeNew("AudioVolume", planetGO, sector, info);
            go.layer = Layer.AdvancedEffectVolume;

            var audioSource = go.AddComponent<AudioSource>();

            var owAudioSource = go.AddComponent<OWAudioSource>();
            owAudioSource._audioSource = audioSource;
            owAudioSource.loop = info.loop;
            owAudioSource.SetMaxVolume(info.volume);
            owAudioSource.SetClipSelectionType(EnumUtils.Parse<OWAudioSource.ClipSelectionOnPlay>(info.clipSelection.ToString()));
            owAudioSource.SetTrack(EnumUtils.Parse<OWAudioMixer.TrackName>(info.track.ToString()));
            AudioUtilities.SetAudioClip(owAudioSource, info.audio, mod);

            var audioVolume = go.AddComponent<AudioVolume>();
            audioVolume._layer = info.layer;
            audioVolume.SetPriority(info.priority);
            audioVolume._fadeSeconds = info.fadeSeconds;
            audioVolume._noFadeFromBeginning = info.noFadeFromBeginning;
            audioVolume._randomizePlayhead = info.randomizePlayhead;
            audioVolume._pauseOnFadeOut = info.pauseOnFadeOut;

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;
            audioVolume._triggerVolumeOverride = owTriggerVolume;

            go.SetActive(true);

            return audioVolume;
        }
    }
}
