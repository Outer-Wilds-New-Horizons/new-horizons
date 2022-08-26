using NewHorizons.OtherMods.AchievementsPlus;
using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.Props
{
    public static class SignalBuilder
    {
        private static AnimationCurve _customCurve = null;

        private static Dictionary<SignalName, string> _customSignalNames;
        private static Stack<SignalName> _availableSignalNames;
        private static int _nextCustomSignalName;

        private static Dictionary<SignalFrequency, string> _customFrequencyNames;
        private static int _nextCustomFrequencyName;

        public static int NumberOfFrequencies;

        public static List<SignalName> QMSignals { get; private set; }
        public static List<SignalName> CloakedSignals { get; private set; }

        public static bool Initialized;

        public static void Init()
        {
            Logger.LogVerbose($"Initializing SignalBuilder");
            _customSignalNames = new Dictionary<SignalName, string>();
            _availableSignalNames = new Stack<SignalName>(new SignalName[]
            {
                (SignalName)17,
                (SignalName)18,
                (SignalName)19,
                (SignalName)26,
                (SignalName)27,
                (SignalName)28,
                (SignalName)29,
                (SignalName)33,
                (SignalName)34,
                (SignalName)35,
                (SignalName)36,
                (SignalName)37,
                (SignalName)38,
                (SignalName)39,
                (SignalName)50,
                (SignalName)51,
                (SignalName)52,
                (SignalName)53,
                (SignalName)54,
                (SignalName)55,
                (SignalName)56,
                (SignalName)57,
                (SignalName)58,
                (SignalName)59,
                SignalName.WhiteHole_WH,
                SignalName.WhiteHole_SS_Receiver,
                SignalName.WhiteHole_CT_Receiver,
                SignalName.WhiteHole_CT_Experiment,
                SignalName.WhiteHole_TT_Receiver,
                SignalName.WhiteHole_TT_TimeLoopCore,
                SignalName.WhiteHole_TH_Receiver,
                SignalName.WhiteHole_BH_NorthPoleReceiver,
                SignalName.WhiteHole_BH_ForgeReceiver,
                SignalName.WhiteHole_GD_Receiver,
            });
            _customFrequencyNames = new Dictionary<SignalFrequency, string>() {
                { SignalFrequency.Statue, "FREQ_STATUE" },
                { SignalFrequency.Default, "FREQ_UNKNOWN" },
                { SignalFrequency.WarpCore, "FREQ_WARP_CORE" }
            };
            _nextCustomSignalName = 200;
            _nextCustomFrequencyName = 256;
            NumberOfFrequencies = 8;

            QMSignals = new List<SignalName>() { SignalName.Quantum_QM };
            CloakedSignals = new List<SignalName>();

            Initialized = true;
        }

        public static SignalFrequency AddFrequency(string str)
        {
            if (_customFrequencyNames == null) Init();

            Logger.Log($"Registering new frequency name [{str}]");

            if (NumberOfFrequencies == 31)
            {
                Logger.LogWarning($"Can't store any more frequencies, skipping [{str}]");
                return SignalFrequency.Default;
            }

            var freq = CollectionUtilities.KeyByValue(_customFrequencyNames, str);
            if (freq != default)
            {
                return freq;
            }

            freq = (SignalFrequency)_nextCustomFrequencyName;
            _nextCustomFrequencyName *= 2;
            _customFrequencyNames.Add(freq, str);

            NumberOfFrequencies++;

            // This stuff happens after the signalscope is Awake so we have to change the number of frequencies now
            GameObject.FindObjectOfType<Signalscope>()._strongestSignals = new AudioSignal[NumberOfFrequencies + 1];

            return freq;
        }

        public static string GetCustomFrequencyName(SignalFrequency frequencyName)
        {
            if (_customFrequencyNames == null) Init();

            _customFrequencyNames.TryGetValue(frequencyName, out string name);
            return name;
        }

        public static SignalName AddSignalName(string str)
        {
            if (_customSignalNames == null) Init();

            Logger.Log($"Registering new signal name [{str}]");
            SignalName newName;

            if (_availableSignalNames.Count == 0) newName = (SignalName)_nextCustomSignalName++;
            else newName = _availableSignalNames.Pop();

            _customSignalNames.Add(newName, str);
            return newName;
        }

        public static string GetCustomSignalName(SignalName signalName)
        {
            if (_customSignalNames == null) Init();

            _customSignalNames.TryGetValue(signalName, out string name);
            return name;
        }

        public static GameObject Make(GameObject planetGO, Sector sector, SignalModule.SignalInfo info, IModBehaviour mod)
        {
            var signalGO = new GameObject($"Signal_{info.name}");
            signalGO.SetActive(false);
            signalGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = planetGO.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    signalGO.transform.parent = newParent;
                }
                else
                {
                    Logger.LogWarning($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                }
            }

            signalGO.transform.position = planetGO.transform.TransformPoint(info.position != null ? (Vector3)info.position : Vector3.zero);
            signalGO.layer = LayerMask.NameToLayer("AdvancedEffectVolume");

            var source = signalGO.AddComponent<AudioSource>();
            var owAudioSource = signalGO.AddComponent<OWAudioSource>();
            owAudioSource._audioSource = source;

            var audioSignal = signalGO.AddComponent<AudioSignal>();
            audioSignal._owAudioSource = owAudioSource;

            var frequency = StringToFrequency(info.frequency);
            var name = StringToSignalName(info.name);

            audioSignal.SetSector(sector);

            if (name == SignalName.Default) audioSignal._preventIdentification = true;

            audioSignal._frequency = frequency;
            audioSignal._name = name;
            audioSignal._sourceRadius = info.sourceRadius;
            audioSignal._revealFactID = info.reveals;
            audioSignal._onlyAudibleToScope = info.onlyAudibleToScope;
            audioSignal._identificationDistance = info.identificationRadius;
            audioSignal._canBePickedUpByScope = true;
            audioSignal._outerFogWarpVolume = planetGO.GetComponentInChildren<OuterFogWarpVolume>(); // shouldn't break non-bramble signals
            
            source.loop = true;
            source.minDistance = 0;
            source.maxDistance = 30;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            source.rolloffMode = AudioRolloffMode.Custom;

            if (_customCurve == null)
            {
                _customCurve = new AnimationCurve(
                    new Keyframe(0.0333f, 1f, -30.012f, -30.012f, 0.3333f, 0.3333f),
                    new Keyframe(0.0667f, 0.5f, -7.503f, -7.503f, 0.3333f, 0.3333f),
                    new Keyframe(0.1333f, 0.25f, -1.8758f, -1.8758f, 0.3333f, 0.3333f),
                    new Keyframe(0.2667f, 0.125f, -0.4689f, -0.4689f, 0.3333f, 0.3333f),
                    new Keyframe(0.5333f, 0.0625f, -0.1172f, -0.1172f, 0.3333f, 0.3333f),
                    new Keyframe(1f, 0f, -0.0333f, -0.0333f, 0.3333f, 0.3333f));
            }

            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _customCurve);
            // If it can be heard regularly then we play it immediately
            source.playOnAwake = !info.onlyAudibleToScope;
            source.spatialBlend = 1f;
            source.volume = 0.5f;
            source.dopplerLevel = 0;

            owAudioSource.SetTrack(OWAudioMixer.TrackName.Signal);
            AudioUtilities.SetAudioClip(owAudioSource, info.audio, mod);

            // Frequency detection trigger volume

            var sphereShape = signalGO.AddComponent<SphereShape>();
            var owTriggerVolume = signalGO.AddComponent<OWTriggerVolume>();
            var audioSignalDetectionTrigger = signalGO.AddComponent<AudioSignalDetectionTrigger>();

            sphereShape.radius = info.detectionRadius == 0 ? info.sourceRadius + 30 : info.detectionRadius;
            audioSignalDetectionTrigger._signal = audioSignal;
            audioSignalDetectionTrigger._trigger = owTriggerVolume;

            signalGO.SetActive(true);

            // Track certain special signal things
            if (planetGO.GetComponent<AstroObject>()?.GetAstroObjectName() == AstroObject.Name.QuantumMoon) QMSignals.Add(name);
            if (info.insideCloak) CloakedSignals.Add(name);

            return signalGO;
        }

        private static SignalFrequency StringToFrequency(string str)
        {
            foreach (SignalFrequency freq in Enum.GetValues(typeof(SignalFrequency)))
            {
                if (str.Equals(freq.ToString())) return freq;
            }
            var customName = CollectionUtilities.KeyByValue(_customFrequencyNames, str);

            if (customName == default) customName = AddFrequency(str);

            return customName;
        }

        public static SignalName StringToSignalName(string str)
        {
            foreach (SignalName name in Enum.GetValues(typeof(SignalName)))
            {
                if (str.Equals(name.ToString())) return name;
            }
            var customName = CollectionUtilities.KeyByValue(_customSignalNames, str);
            if (customName == default) customName = AddSignalName(str);

            return customName;
        }
    }
}
