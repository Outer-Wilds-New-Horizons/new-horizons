using NewHorizons.Utility.Files;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Components.EOTE
{
    public class DreamWorldEndTimes : MonoBehaviour
    {
        private IModBehaviour _mod;
        private string _endTimesAudio;
        private IModBehaviour _dreamMod;
        private string _endTimesDreamAudio;

        public void SetEndTimesAudio(string audio, IModBehaviour mod)
        {
            _mod = mod;
            _endTimesAudio = audio;
        }

        public void AssignEndTimes(OWAudioSource endTimesSource) => Assign(endTimesSource, _endTimesAudio, _mod, AudioType.EndOfTime);

        public void SetEndTimesDreamAudio(string audio, IModBehaviour mod)
        {
            _dreamMod = mod;
            _endTimesDreamAudio = audio;
        }

        public void AssignEndTimesDream(OWAudioSource endTimesSource) => Assign(endTimesSource, _endTimesDreamAudio, _dreamMod, AudioType.EndOfTime_Dream);

        public static void Assign(OWAudioSource endTimesSource, string endTimesClip, IModBehaviour mod, AudioType defaultClip)
        {
            endTimesSource.Stop();
            if (!string.IsNullOrWhiteSpace(endTimesClip))
            {
                AudioUtilities.SetAudioClip(endTimesSource, endTimesClip, mod);
            }
            else
                endTimesSource.AssignAudioLibraryClip(defaultClip);
        }
    }
}