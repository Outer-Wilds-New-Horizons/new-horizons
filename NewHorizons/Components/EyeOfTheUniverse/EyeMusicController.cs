using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components.EyeOfTheUniverse
{
    public class EyeMusicController : MonoBehaviour
    {
        private List<OWAudioSource> _loopSources = new();
        private List<OWAudioSource> _finaleSources = new();
        private bool _transitionToFinale;
        private bool _isPlaying;

        public void RegisterLoopSource(OWAudioSource src)
        {
            src.loop = false;
            src.SetLocalVolume(1f);
            _loopSources.Add(src);
        }

        public void RegisterFinaleSource(OWAudioSource src)
        {
            src.loop = false;
            src.SetLocalVolume(1f);
            _finaleSources.Add(src);
        }

        public void StartPlaying()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            StartCoroutine(DoLoop());
        }

        public void TransitionToFinale()
        {
            _transitionToFinale = true;
        }

        private IEnumerator DoLoop()
        {
            // Initial delay to ensure audio system has time to schedule all loops for the same tick
            double timeBufferWindow = 0.5;

            // Track when the next audio (loop or finale) should play, in audio system time
            double nextAudioEventTime = AudioSettings.dspTime + timeBufferWindow;

            // Determine timing using the first loop audio clip (should be Riebeck's banjo loop)
            var referenceLoopClip = _loopSources.First().clip;
            double loopDuration = referenceLoopClip.samples / (double)referenceLoopClip.frequency;
            double segmentDuration = loopDuration / 4.0;

            while (!_transitionToFinale)
            {
                // Play loops in sync
                var loopStartTime = nextAudioEventTime;
                foreach (var loopSrc in _loopSources)
                {
                    loopSrc._audioSource.PlayScheduled(loopStartTime);
                }

                nextAudioEventTime += loopDuration;

                // Handle loop segments (the current musical measure will always finish playing before transitioning to the finale)
                for (int i = 0; i < 4; i++)
                {
                    // Interrupting the upcoming segment for the finale
                    if (_transitionToFinale)
                    {
                        // End the loop at the start time of the upcoming segment
                        var loopStopTime = loopStartTime + segmentDuration * (i + 1);

                        // Cancel scheduled upcoming loop
                        foreach (var loopSrc in _loopSources)
                        {
                            loopSrc._audioSource.SetScheduledEndTime(loopStopTime);
                        }

                        // Schedule finale for as soon as the loop ends
                        nextAudioEventTime = loopStopTime;
                        break;
                    }

                    // Wait until shortly before the next segment (`nextAudioEventTime` will be ahead of current time by `timeBufferWindow`)
                    yield return new WaitForSecondsRealtime((float)segmentDuration);
                }
            }

            // Play finale in sync
            foreach (var finaleSrc in _finaleSources)
            {
                finaleSrc._audioSource.PlayScheduled(nextAudioEventTime);
            }
        }
    }
}
