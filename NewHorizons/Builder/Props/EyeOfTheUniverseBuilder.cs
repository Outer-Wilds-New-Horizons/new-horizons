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
            var travelerData = EyeSceneHandler.CreateEyeTravelerData(info.id);
            travelerData.info = info;
            travelerData.requirementsMet = true;

            if (!string.IsNullOrEmpty(info.requiredFact) && !ShipLogHandler.KnowsFact(info.requiredFact))
            {
                travelerData.requirementsMet = false;
            }

            if (!string.IsNullOrEmpty(info.requiredPersistentCondition) && !DialogueConditionManager.SharedInstance.GetConditionState(info.requiredPersistentCondition))
            {
                travelerData.requirementsMet = false;
            }

            if (!travelerData.requirementsMet)
            {
                return null;
            }

            var go = DetailBuilder.Make(planetGO, sector, nhBody.Mod, info);

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
                if (info.dialogue.position == null && info.dialogue.parentPath == null)
                {
                    info.dialogue.isRelativeToParent = true;
                }
                GeneralPropBuilder.MakeFromExisting(dialogueTree.gameObject, planetGO, sector, info.dialogue, defaultParent: go.transform);

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
                travelerController._dialogueTree = go.GetComponentInChildren<CharacterDialogueTree>();
                if (travelerController._dialogueTree == null)
                {
                    NHLogger.LogError($"Eye Traveler with ID \"{info.id}\" does not have any dialogue set");
                }
            }

            travelerData.controller = travelerController;

            OWAudioSource loopAudioSource = null;

            if (info.signal != null)
            {
                if (string.IsNullOrEmpty(info.signal.name))
                {
                    info.signal.name = go.name;
                }
                if (string.IsNullOrEmpty(info.signal.frequency))
                {
                    info.signal.frequency = "Traveler";
                }
                var signalGO = SignalBuilder.Make(planetGO, sector, info.signal, nhBody.Mod);
                if (info.signal.position == null && info.signal.parentPath == null)
                {
                    info.signal.isRelativeToParent = true;
                }
                GeneralPropBuilder.MakeFromExisting(signalGO, planetGO, sector, info.signal, defaultParent: go.transform);

                var signal = signalGO.GetComponent<AudioSignal>();
                travelerController._signal = signal;
                signal.SetSignalActivation(false);
                loopAudioSource = signal.GetOWAudioSource();

            }
            else if (travelerController._signal == null)
            {
                NHLogger.LogError($"Eye Traveler with ID \"{info.id}\" does not have any loop audio set");
            }

            travelerData.loopAudioSource = loopAudioSource;

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

            travelerData.finaleAudioSource = finaleAudioSource;

            return travelerController;
        }

        public static QuantumInstrument MakeQuantumInstrument(GameObject planetGO, Sector sector, QuantumInstrumentInfo info, NewHorizonsBody nhBody)
        {
            var travelerData = EyeSceneHandler.GetEyeTravelerData(info.id);

            if (travelerData != null && !travelerData.requirementsMet)
            {
                return null;
            }

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

            if (travelerData != null)
            {
                travelerData.quantumInstruments.Add(quantumInstrument);
            }
            else
            {
                NHLogger.LogError($"Quantum instrument with ID \"{info.id}\" has no matching eye traveler");
            }

            info.signal ??= new SignalInfo();

            if (travelerData?.info != null && travelerData.info.signal != null)
            {
                if (string.IsNullOrEmpty(info.signal.name))
                {
                    info.signal.name = travelerData.info.signal.name;
                }
                if (string.IsNullOrEmpty(info.signal.audio))
                {
                    info.signal.audio = travelerData.info.signal.audio;
                }
                if (string.IsNullOrEmpty(info.signal.frequency))
                {
                    info.signal.frequency = travelerData.info.signal.frequency;
                }
            }

            if (!string.IsNullOrEmpty(info.signal.audio))
            {
                var signalGO = SignalBuilder.Make(planetGO, sector, info.signal, nhBody.Mod);
                if (info.signal.position == null && info.signal.parentPath == null)
                {
                    info.signal.isRelativeToParent = true;
                }
                GeneralPropBuilder.MakeFromExisting(signalGO, planetGO, sector, info.signal, defaultParent: go.transform);
            }
            else
            {
                NHLogger.LogError($"Eye Traveler with ID \"{info.id}\" does not have any loop audio set");
            }

            return quantumInstrument;
        }

        public static InstrumentZone MakeInstrumentZone(GameObject planetGO, Sector sector, InstrumentZoneInfo info, NewHorizonsBody nhBody)
        {
            var travelerData = EyeSceneHandler.GetEyeTravelerData(info.id);

            if (travelerData != null && !travelerData.requirementsMet)
            {
                return null;
            }

            var go = DetailBuilder.Make(planetGO, sector, nhBody.Mod, info);

            var instrumentZone = go.AddComponent<InstrumentZone>();

            if (travelerData != null)
            {
                travelerData.instrumentZones.Add(instrumentZone);
            }
            else
            {
                NHLogger.LogError($"Instrument zone with ID \"{info.id}\" has no matching eye traveler");
            }

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
