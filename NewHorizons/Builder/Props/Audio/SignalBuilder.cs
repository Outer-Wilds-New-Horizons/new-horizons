using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;
using NewHorizons.External.Modules.Props.Audio;

namespace NewHorizons.Builder.Props.Audio
{
    public static class SignalBuilder
    {
        private static Dictionary<SignalName, string> _customSignalNames;

        private static Dictionary<SignalFrequency, string> _customFrequencyNames;

        public static int NumberOfFrequencies;

        private static List<SignalName> _qmSignals;
        private static List<SignalName> _cloakedSignals;

        public static bool Initialized;

        public static void Init()
        {
            NHLogger.LogVerbose($"Initializing SignalBuilder");
            _customSignalNames = new Dictionary<SignalName, string>();
            _customFrequencyNames = new Dictionary<SignalFrequency, string>() {
                { SignalFrequency.Statue, "FREQ_STATUE" },
                { SignalFrequency.Default, "FREQ_UNKNOWN" },
                { SignalFrequency.WarpCore, "FREQ_WARP_CORE" }
            };
            NumberOfFrequencies = EnumUtils.GetValues<SignalFrequency>().Length;

            _qmSignals = new List<SignalName>() { SignalName.Quantum_QM };
            _cloakedSignals = new List<SignalName>();

            Initialized = true;
        }

        public static bool IsCloaked(this SignalName signalName)
        {
            return _cloakedSignals.Contains(signalName);
        }

        public static bool IsOnQuantumMoon(this SignalName signalName)
        {
            return _qmSignals.Contains(signalName);
        }

        public static SignalFrequency AddFrequency(string str)
        {
            if (_customFrequencyNames == null) Init();

            var freq = CollectionUtilities.KeyByValue(_customFrequencyNames, str);
            if (freq != default) return freq;

            NHLogger.Log($"Registering new frequency name [{str}]");

            if (NumberOfFrequencies == 31)
            {
                NHLogger.LogWarning($"Can't store any more frequencies, skipping [{str}]");
                return SignalFrequency.Default;
            }

            freq = EnumUtilities.Create<SignalFrequency>(str);
            _customFrequencyNames.Add(freq, str);

            NumberOfFrequencies = EnumUtils.GetValues<SignalFrequency>().Length;

            // This stuff happens after the signalscope is Awake so we have to change the number of frequencies now
            Object.FindObjectOfType<Signalscope>()._strongestSignals = new AudioSignal[NumberOfFrequencies + 1];

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

            var name = CollectionUtilities.KeyByValue(_customSignalNames, str);
            if (name != default) return name;

            NHLogger.Log($"Registering new signal name [{str}]");

            name = EnumUtilities.Create<SignalName>(str);
            _customSignalNames.Add(name, str);

            return name;
        }

        public static string GetCustomSignalName(SignalName signalName)
        {
            if (_customSignalNames == null) Init();

            _customSignalNames.TryGetValue(signalName, out string name);
            return name;
        }

        public static GameObject Make(GameObject planetGO, Sector sector, SignalInfo info, IModBehaviour mod)
        {
            var owAudioSource = GeneralAudioBuilder.Make(planetGO, sector, info, mod);
            var signalGO = owAudioSource.gameObject;

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

            // If it can be heard regularly then we play it immediately
            owAudioSource.playOnAwake = !info.onlyAudibleToScope;

            // Frequency detection trigger volume
            var sphereShape = signalGO.AddComponent<SphereShape>();
            var owTriggerVolume = signalGO.AddComponent<OWTriggerVolume>();
            var audioSignalDetectionTrigger = signalGO.AddComponent<AudioSignalDetectionTrigger>();

            sphereShape.radius = info.detectionRadius == 0 ? info.sourceRadius + 30 : info.detectionRadius;
            audioSignalDetectionTrigger._signal = audioSignal;
            audioSignalDetectionTrigger._trigger = owTriggerVolume;

            owAudioSource.SetTrack(OWAudioMixer.TrackName.Signal);

            signalGO.SetActive(true);

            // Track certain special signal things
            if (planetGO.GetComponent<AstroObject>()?.GetAstroObjectName() == AstroObject.Name.QuantumMoon) _qmSignals.Add(name);
            if (info.insideCloak) _cloakedSignals.Add(name);

            return signalGO;
        }

        private static SignalFrequency StringToFrequency(string str)
        {
            return EnumUtils.TryParse(str, out SignalFrequency frequency) ? frequency : AddFrequency(str);
        }

        public static SignalName StringToSignalName(string str)
        {
            return EnumUtils.TryParse(str, out SignalName name) ? name : AddSignalName(str);
        }
    }
}
