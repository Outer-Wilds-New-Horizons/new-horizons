using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
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

            go.transform.position = planetGO.transform.TransformPoint(info.position != null ? (Vector3)info.position : Vector3.zero);
            go.layer = LayerMask.NameToLayer("AdvancedEffectVolume");

            var audioSource = go.AddComponent<AudioSource>();

            var owAudioSource = go.AddComponent<OWAudioSource>();
            owAudioSource._audioSource = audioSource;
            owAudioSource.loop = info.loop;
            owAudioSource.SetTrack((OWAudioMixer.TrackName)Enum.Parse(typeof(OWAudioMixer.TrackName), Enum.GetName(typeof(AudioMixerTrackName), info.track)));
            AudioUtilities.SetAudioClip(owAudioSource, info.audio, mod);

            var audioVolume = go.AddComponent<AudioVolume>();

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
