using NewHorizons.External.Modules.Props.Audio;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NewHorizons.External.Modules.Props.Audio;
using System.Linq;

namespace NewHorizons.Builder.Props.Audio
{
    public static class SignalBuilder
    {
        private static Dictionary<SignalName, string> _customSignalNames;

        private static Dictionary<SignalFrequency, string> _customFrequencyNames;

        public static int NumberOfFrequencies;

        private static HashSet<AudioSignal> _qmSignals;
        private static HashSet<AudioSignal> _cloakedSignals;

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

            _qmSignals = new (){ SearchUtilities.Find("QuantumMoon_Body/Signal_Quantum").GetComponent<AudioSignal>() };
            _cloakedSignals = new();

            Initialized = true;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static HashSet<SignalFrequency> _frequenciesInUse = new();

        private static void OnSceneUnloaded(Scene _)
        {
            _frequenciesInUse.Clear();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            // If its the base game solar system or eye we get all the main frequencies
            if (scene.name == LoadManager.SceneToName(OWScene.EyeOfTheUniverse) ||
                (scene.name == LoadManager.SceneToName(OWScene.SolarSystem) && Main.Instance.CurrentStarSystem == "SolarSystem"))
            {
                _frequenciesInUse.Add(SignalFrequency.Quantum);
                _frequenciesInUse.Add(SignalFrequency.EscapePod);
                _frequenciesInUse.Add(SignalFrequency.Radio);
                _frequenciesInUse.Add(SignalFrequency.HideAndSeek);
            }

            // By default lets always have travelers
            _frequenciesInUse.Add(SignalFrequency.Traveler);
        }

        public static bool IsFrequencyInUse(SignalFrequency freq) => _frequenciesInUse.Contains(freq);

        public static bool IsCloaked(this AudioSignal signal)
        {
            return _cloakedSignals.Contains(signal);
        }

        public static bool IsOnQuantumMoon(this AudioSignal signal)
        {
            return _qmSignals.Contains(signal);
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
            if (planetGO.GetComponent<AstroObject>()?.GetAstroObjectName() == AstroObject.Name.QuantumMoon) _qmSignals.Add(audioSignal);
            if (info.insideCloak) _cloakedSignals.Add(audioSignal);

            _frequenciesInUse.Add(frequency);

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
