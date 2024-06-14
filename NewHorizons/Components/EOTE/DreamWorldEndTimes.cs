using NewHorizons.Utility.Files;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Components.EOTE
{
    public class DreamWorldEndTimes : MonoBehaviour
    {
        private AudioType _endTimesAudio = AudioType.EndOfTime;
        private AudioType _endTimesDreamAudio = AudioType.EndOfTime_Dream;

        public void SetEndTimesAudio(AudioType audio)
        {
            _endTimesAudio = audio;
        }

        public void AssignEndTimes(OWAudioSource endTimesSource) => Assign(endTimesSource, _endTimesAudio);

        public void SetEndTimesDreamAudio(AudioType audio)
        {
            _endTimesDreamAudio = audio;
        }

        public void AssignEndTimesDream(OWAudioSource endTimesSource) => Assign(endTimesSource, _endTimesDreamAudio);

        public static void Assign(OWAudioSource endTimesSource, AudioType audio)
        {
            endTimesSource.Stop();
            endTimesSource.AssignAudioLibraryClip(audio);
        }
    }
}