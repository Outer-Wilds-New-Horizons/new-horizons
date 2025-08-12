using NewHorizons.External.Modules.Props.Audio;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Props.Audio
{
    public static class GeneralAudioBuilder
    {
        public static AnimationCurve CustomCurve => new(
            new Keyframe(0.0333f, 1f, -30.012f, -30.012f, 0.3333f, 0.3333f),
            new Keyframe(0.0667f, 0.5f, -7.503f, -7.503f, 0.3333f, 0.3333f),
            new Keyframe(0.1333f, 0.25f, -1.8758f, -1.8758f, 0.3333f, 0.3333f),
            new Keyframe(0.2667f, 0.125f, -0.4689f, -0.4689f, 0.3333f, 0.3333f),
            new Keyframe(0.5333f, 0.0625f, -0.1172f, -0.1172f, 0.3333f, 0.3333f),
            new Keyframe(1f, 0f, -0.0333f, -0.0333f, 0.3333f, 0.3333f)
    );

        public static OWAudioSource Make(GameObject planetGO, Sector sector, BaseAudioInfo info, IModBehaviour mod)
        {
            var signalGO = GeneralPropBuilder.MakeNew($"{(string.IsNullOrEmpty(info.rename) ? "AudioSource" : info.rename)}", planetGO, ref sector, info);
            signalGO.layer = Layer.AdvancedEffectVolume;

            var source = signalGO.AddComponent<AudioSource>();
            var owAudioSource = signalGO.AddComponent<OWAudioSource>();
            owAudioSource._audioSource = source;

            source.loop = true;
            source.minDistance = info.minDistance;
            source.maxDistance = info.maxDistance;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.spatialBlend = 1f;
            source.volume = info.volume;
            source.dopplerLevel = 0;

            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, CustomCurve);
            AudioUtilities.SetAudioClip(owAudioSource, info.audio, mod);

            return owAudioSource;
        }
    }
}
