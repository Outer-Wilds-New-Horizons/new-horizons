using NewHorizons.Builder.Props.Audio;
using NewHorizons.Components.EyeOfTheUniverse;
using NewHorizons.External;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props.Audio;
using NewHorizons.External.Modules.Props.EyeOfTheUniverse;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class EyeOfTheUniverseBuilder
    {
        public static TravelerEyeController MakeEyeTraveler(GameObject planetGO, Sector sector, EyeTravelerInfo info, NewHorizonsBody nhBody)
        {
            var go = DetailBuilder.Make(planetGO, sector, nhBody.Mod, info);

            if (string.IsNullOrEmpty(info.name))
            {
                info.name = go.name;
            }

            var travelerController = go.GetAddComponent<TravelerEyeController>();
            if (!string.IsNullOrEmpty(info.startPlayingCondition))
            {
                travelerController._startPlayingCondition = info.startPlayingCondition;
            }
            else if (string.IsNullOrEmpty(travelerController._startPlayingCondition))
            {
                NHLogger.LogError($"Eye Traveler with ID \"{info.id}\" does not have a Start Playing condition set");
            }
            if (travelerController._animator == null)
            {
                travelerController._animator = go.GetComponentInChildren<Animator>();
            }
            if (info.dialogue != null)
            {
                var (dialogueTree, remoteTrigger) = DialogueBuilder.Make(planetGO, sector, info.dialogue, nhBody.Mod);
                dialogueTree.transform.SetParent(travelerController.transform, false);
                dialogueTree.transform.localPosition = Vector3.zero;
                if (travelerController._dialogueTree != null)
                {
                    travelerController._dialogueTree.OnStartConversation -= travelerController.OnStartConversation;
                    travelerController._dialogueTree.OnEndConversation -= travelerController.OnEndConversation;
                }
                travelerController._dialogueTree = dialogueTree;
                travelerController._dialogueTree.OnStartConversation += travelerController.OnStartConversation;
                travelerController._dialogueTree.OnEndConversation += travelerController.OnEndConversation;
            }
            else if (travelerController._dialogueTree == null)
            {
                NHLogger.LogError($"Eye Traveler with ID \"{info.id}\" does not have any dialogue set");
            }

            OWAudioSource loopAudioSource = null;

            if (!string.IsNullOrEmpty(info.loopAudio))
            {
                var signalInfo = new SignalInfo()
                {
                    name = info.name,
                    audio = info.loopAudio,
                    detectionRadius = 10f,
                    identificationRadius = 10f,
                    onlyAudibleToScope = false,
                    frequency = string.IsNullOrEmpty(info.frequency) ? "Traveler" : info.frequency,
                };
                var signalGO = SignalBuilder.Make(planetGO, sector, signalInfo, nhBody.Mod);
                signalGO.transform.SetParent(travelerController.transform, false);
                signalGO.transform.localPosition = Vector3.zero;

                var signal = signalGO.GetComponent<AudioSignal>();
                travelerController._signal = signal;
                signal.SetSignalActivation(false);
                loopAudioSource = signal.GetOWAudioSource();
            }
            else if (travelerController._signal == null)
            {
                NHLogger.LogError($"Eye Traveler with ID \"{info.id}\" does not have any loop audio set");
            }

            OWAudioSource finaleAudioSource = null;

            if (!string.IsNullOrEmpty(info.finaleAudio))
            {
                var finaleAudioInfo = new AudioSourceInfo()
                {
                    audio = info.finaleAudio,
                    track = External.SerializableEnums.NHAudioMixerTrackName.Music,
                    volume = 1f,
                };
                finaleAudioSource = GeneralAudioBuilder.Make(planetGO, sector, finaleAudioInfo, nhBody.Mod);
                finaleAudioSource.SetTrack(finaleAudioInfo.track.ConvertToOW());
                finaleAudioSource.loop = false;
                finaleAudioSource.spatialBlend = 0f;
                finaleAudioSource.playOnAwake = false;
                finaleAudioSource.gameObject.SetActive(true);
            }

            var travelerData = EyeSceneHandler.GetOrCreateEyeTravelerData(info.id);
            travelerData.info = info;
            travelerData.controller = travelerController;
            travelerData.loopAudioSource = loopAudioSource;
            travelerData.finaleAudioSource = finaleAudioSource;

            return travelerController;
        }

        public static QuantumInstrument MakeQuantumInstrument(GameObject planetGO, Sector sector, QuantumInstrumentInfo info, NewHorizonsBody nhBody)
        {
            var go = DetailBuilder.Make(planetGO, sector, nhBody.Mod, info);
            go.layer = Layer.Interactible;
            if (info.interactRadius > 0f)
            {
                var collider = go.AddComponent<SphereCollider>();
                collider.radius = info.interactRadius;
                collider.isTrigger = true;
                go.GetAddComponent<OWCollider>();
            }

            go.GetAddComponent<InteractReceiver>();
            var quantumInstrument = go.GetAddComponent<QuantumInstrument>();
            quantumInstrument._gatherWithScope = info.gatherWithScope;
            ArrayHelpers.Append(ref quantumInstrument._deactivateObjects, go);

            var trigger = go.AddComponent<QuantumInstrumentTrigger>();
            trigger.gatherCondition = info.gatherCondition;

            var travelerData = EyeSceneHandler.GetOrCreateEyeTravelerData(info.id);
            travelerData.quantumInstruments.Add(quantumInstrument);

            if (travelerData.info != null)
            {
                if (!string.IsNullOrEmpty(travelerData.info.loopAudio))
                {
                    var signalInfo = new SignalInfo()
                    {
                        name = travelerData.info.name,
                        audio = travelerData.info.loopAudio,
                        detectionRadius = 0,
                        identificationRadius = 0,
                        frequency = string.IsNullOrEmpty(travelerData.info.frequency) ? "Traveler" : travelerData.info.frequency,
                    };
                    var signalGO = SignalBuilder.Make(planetGO, sector, signalInfo, nhBody.Mod);
                    signalGO.transform.SetParent(quantumInstrument.transform, false);
                    signalGO.transform.localPosition = Vector3.zero;
                }
                else
                {
                    NHLogger.LogError($"Eye Traveler with ID \"{info.id}\" does not have any loop audio set");
                }
            }
            else
            {
                NHLogger.LogError($"Quantum instrument with ID \"{info.id}\" has no matching eye traveler");
            }

            return quantumInstrument;
        }

        public static InstrumentZone MakeInstrumentZone(GameObject planetGO, Sector sector, InstrumentZoneInfo info, NewHorizonsBody nhBody)
        {
            var go = DetailBuilder.Make(planetGO, sector, nhBody.Mod, info);

            var instrumentZone = go.AddComponent<InstrumentZone>();

            var travelerData = EyeSceneHandler.GetOrCreateEyeTravelerData(info.id);
            travelerData.instrumentZones.Add(instrumentZone);

            return instrumentZone;
        }

        public static void Make(GameObject go, Sector sector, EyeOfTheUniverseModule module, NewHorizonsBody nhBody)
        {
            if (module.eyeTravelers != null)
            {
                foreach (var info in module.eyeTravelers)
                {
                    MakeEyeTraveler(go, sector, info, nhBody);
                }
            }
            if (module.instrumentZones != null)
            {
                foreach (var info in module.instrumentZones)
                {
                    MakeInstrumentZone(go, sector, info, nhBody);
                }
            }
            if (module.quantumInstruments != null)
            {
                foreach (var info in module.quantumInstruments)
                {
                    MakeQuantumInstrument(go, sector, info, nhBody);
                }
            }
        }
    }
}
