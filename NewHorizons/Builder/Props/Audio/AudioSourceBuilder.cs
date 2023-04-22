using NewHorizons.External.Modules.Props.Audio;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Props.Audio
{
    public static class AudioSourceBuilder
    {
        public static GameObject Make(GameObject planetGO, Sector sector, AudioSourceInfo info, IModBehaviour mod)
        {
            var owAudioSource = GeneralAudioBuilder.Make(planetGO, sector, info, mod);

            owAudioSource.SetTrack(info.track.ConvertToOW());

            owAudioSource.gameObject.SetActive(true);

            return owAudioSource.gameObject;
        }
    }
}
