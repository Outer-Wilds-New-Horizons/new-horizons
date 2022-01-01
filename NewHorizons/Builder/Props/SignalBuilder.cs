using NewHorizons.External;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class SignalBuilder
    {
        private static AnimationCurve _customCurve = null;

        private static Dictionary<SignalName, string> _customSignalNames;
        private static Stack<int> _availableSignalNames;

        // TODO : Save and load this from/to a file
        public static List<string> KnownSignals { get; private set; } = new List<string>();

        public static void Reset()
        {
            _customSignalNames = new Dictionary<SignalName, string>();
            _availableSignalNames = new Stack<int> (new int[]
            {
                17,
                18,
                19,
                26,
                27,
                28,
                29,
                33,
                34,
                35,
                36,
                37,
                38,
                39,
                50,
                51,
                52,
                53,
                54,
                55,
                56,
                57,
                58,
                59
            });
        }

        public static SignalName AddSignalName(string str)
        {
            if (_availableSignalNames.Count == 0)
            {
                Logger.LogWarning($"There are no more available SignalName spots. Cannot use name [{str}].");
                return SignalName.Default;
            }

            Logger.Log($"Registering new signal name [{str}]");
            var newName = (SignalName)_availableSignalNames.Pop();
            _customSignalNames.Add(newName, str.ToUpper());
            return newName;
        }

        public static string GetCustomSignalName(SignalName signalName)
        {
            if (_customSignalNames.ContainsKey(signalName)) return _customSignalNames[signalName];
            return null;
        }

        public static void Make(GameObject body, Sector sector, SignalModule module)
        {
            foreach(var info in module.Signals)
            {
                Make(body, sector, info);
            }
        }

        public static void Make(GameObject body, Sector sector, SignalModule.SignalInfo info)
        {
            var signalGO = new GameObject($"Signal_{info.Name}");
            signalGO.SetActive(false);
            signalGO.transform.parent = sector.transform;
            signalGO.transform.localPosition = info.Position != null ? (Vector3)info.Position : Vector3.zero;

            var source = signalGO.AddComponent<AudioSource>();
            var owAudioSource = signalGO.AddComponent<OWAudioSource>();
            var audioSignal = signalGO.AddComponent<AudioSignal>();

            var frequency = StringToFrequency(info.Frequency);
            var name = StringToSignalName(info.Name);

            AudioClip clip = SearchUtilities.FindResourceOfTypeAndName<AudioClip>(info.AudioClip);
            if (clip == null) return;

            audioSignal.SetSector(sector);
            audioSignal._frequency = frequency;
            if (name == SignalName.Default)
            {
                name = AddSignalName(info.Name);
                if(name == SignalName.Default) audioSignal._preventIdentification = true; 
            }
            audioSignal._name = name;
            audioSignal._sourceRadius = info.SourceRadius;
            audioSignal._onlyAudibleToScope = info.OnlyAudibleToScope;

            source.clip = clip;
            source.loop = true;
            source.minDistance = 0;
            source.maxDistance = 30;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            source.rolloffMode = AudioRolloffMode.Custom;

            if(_customCurve == null)
                _customCurve = GameObject.Find("Moon_Body/Sector_THM/Characters_THM/Villager_HEA_Esker/Signal_Whistling").GetComponent<AudioSource>().GetCustomCurve(AudioSourceCurveType.CustomRolloff);

            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _customCurve);
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.volume = 0.5f;

            owAudioSource.SetTrack(OWAudioMixer.TrackName.Signal);

            signalGO.SetActive(true);
        }

        private static SignalFrequency StringToFrequency(string str)
        {
            foreach(SignalFrequency freq in Enum.GetValues(typeof(SignalFrequency)))
            {
                if (str.Equals(freq.ToString())) return freq;
            }
            return SignalFrequency.Default;
        }

        private static SignalName StringToSignalName(string str)
        {
            foreach (SignalName name in Enum.GetValues(typeof(SignalName)))
            {
                if (str.Equals(name.ToString())) return name;
            }
            return SignalName.Default;
        } 
    }
}
