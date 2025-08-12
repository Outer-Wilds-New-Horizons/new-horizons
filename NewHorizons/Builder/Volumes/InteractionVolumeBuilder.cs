using NewHorizons.Components.Volumes;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    internal static class InteractionVolumeBuilder
    {
        public static InteractReceiver Make(GameObject planetGO, Sector sector, InteractionVolumeInfo info, IModBehaviour mod)
        {
            // Interaction volumes must use colliders because the first-person interaction system uses raycasting
            if (info.shape != null)
            {
                info.shape.useShape = false;
            }

            var receiver = VolumeBuilder.Make<InteractReceiver>(planetGO, ref sector, info);
            receiver.gameObject.layer = Layer.Interactible;

            receiver._interactRange = info.range;
            receiver._checkViewAngle = info.maxViewAngle.HasValue;
            receiver._maxViewAngle = info.maxViewAngle ?? 180f;
            receiver._usableInShip = info.usableInShip;

            var volume = receiver.gameObject.AddComponent<NHInteractionVolume>();

            volume.Reusable = info.reusable;
            volume.Condition = info.condition;
            volume.Persistent = info.persistent;

            if (!string.IsNullOrEmpty(info.audio))
            {
                var audioSource = receiver.gameObject.AddComponent<AudioSource>();

                // This could be more configurable but this should cover the most common use cases without bloating the info object
                var owAudioSource = receiver.gameObject.AddComponent<OWAudioSource>();
                owAudioSource._audioSource = audioSource;
                owAudioSource.playOnAwake = false;
                owAudioSource.loop = false;
                owAudioSource.SetMaxVolume(1f);
                owAudioSource.SetClipSelectionType(OWAudioSource.ClipSelectionOnPlay.RANDOM);
                owAudioSource.SetTrack(OWAudioMixer.TrackName.Environment);
                AudioUtilities.SetAudioClip(owAudioSource, info.audio, mod);
            }

            if (!string.IsNullOrEmpty(info.pathToAnimator))
            {
                var animObj = planetGO.transform.Find(info.pathToAnimator);

                if (animObj == null)
                {
                    NHLogger.LogError($"Couldn't find child of {planetGO.transform.GetPath()} at {info.pathToAnimator}");
                }
                else
                {
                    var animator = animObj.GetComponent<Animator>();
                    if (animator == null)
                    {
                        NHLogger.LogError($"Couldn't find Animator on {animObj.name} at {info.pathToAnimator}");
                    }
                    else
                    {
                        volume.TargetAnimator = animator;
                        volume.AnimationTrigger = info.animationTrigger;
                    }
                }
            }

            receiver.gameObject.SetActive(true);

            var text = TranslationHandler.GetTranslation(info.prompt, TranslationHandler.TextType.UI);
            Delay.FireOnNextUpdate(() =>
            {
                // This NREs if set immediately
                receiver.ChangePrompt(text);
            });

            return receiver;
        }
    }
}
