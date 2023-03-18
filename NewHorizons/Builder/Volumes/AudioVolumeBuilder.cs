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
            var go = new GameObject("AudioVolume");
            go.SetActive(false);

            go.transform.parent = sector?.transform ?? planetGO.transform;

            if (!string.IsNullOrEmpty(info.rename))
            {
                go.name = info.rename;
            }

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = planetGO.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    go.transform.parent = newParent;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? Vector3.zero);
            if (info.isRelativeToParent) go.transform.localPosition = pos;
            else go.transform.position = planetGO.transform.TransformPoint(pos);
            go.layer = LayerUtilities.AdvancedEffectVolume;

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
