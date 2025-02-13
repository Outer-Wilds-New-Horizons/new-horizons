using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components.EyeOfTheUniverse
{
    public class EyeMusicController : MonoBehaviour
    {
        // Delay between game logic and audio to ensure audio system has time to schedule all loops for the same tick
        const double TIME_BUFFER_WINDOW = 0.5;

        private List<OWAudioSource> _loopSources = new();
        private List<OWAudioSource> _finaleSources = new();
        private bool _transitionToFinale;
        private bool _isPlaying;
        private double _segmentEndAudioTime;
        private float _segmentEndGameTime;
        public CosmicInflationController CosmicInflationController { get; private set; }

        public void RegisterLoopSource(OWAudioSource src)
        {
            src.loop = false;
            src.SetLocalVolume(1f);
            src.Stop();
            src.playOnAwake = false;
            _loopSources.Add(src);
        }

        public void RegisterFinaleSource(OWAudioSource src)
        {
            src.loop = false;
            src.SetLocalVolume(1f);
            src.Stop();
            src.playOnAwake = false;
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

            // Schedule finale for as soon as the current segment loop ends
            double finaleAudioTime = _segmentEndAudioTime;
            float finaleGameTime = _segmentEndGameTime;

            // Cancel loop audio
            foreach (var loopSrc in _loopSources)
            {
                loopSrc._audioSource.SetScheduledEndTime(finaleAudioTime);
            }

            // Set quantum sphere inflation timer
            var finaleDuration = CosmicInflationController._travelerFinaleSource.clip.length;
            CosmicInflationController._startFormationTime = Time.time;
            CosmicInflationController._finishFormationTime = finaleGameTime + finaleDuration - 4f;

            // Play finale in sync
            foreach (var finaleSrc in _finaleSources)
            {
                finaleSrc._audioSource.PlayScheduled(finaleAudioTime);
            }
        }

        public void Awake()
        {
            // EOTP makes 2 new CosmicInflationControllers for no reason
            CosmicInflationController = SearchUtilities.Find("EyeOfTheUniverse_Body/Sector_EyeOfTheUniverse/Sector_Campfire/InflationController").GetComponent<CosmicInflationController>();
        }

        private IEnumerator DoLoop()
        {
            // Determine timing using the first loop audio clip (should be Riebeck's banjo loop)
            var referenceLoopClip = _loopSources.First().clip;
            double loopDuration = referenceLoopClip.samples / (double)referenceLoopClip.frequency;

            // Vanilla audio divides the loop into 4 segments, but that actually causes weird key shifting during the crossfade
            int segmentCount = 2;
            double segmentDuration = loopDuration / segmentCount;

            // Track when the next loop will play, in both audio system time and game time
            double nextLoopAudioTime = AudioSettings.dspTime + TIME_BUFFER_WINDOW;
            float nextLoopGameTime = Time.time + (float)TIME_BUFFER_WINDOW;

            while (!_transitionToFinale)
            {
                // Play loops in sync
                double loopStartAudioTime = nextLoopAudioTime;
                float loopStartGameTime = nextLoopGameTime;

                foreach (var loopSrc in _loopSources)
                {
                    if (!loopSrc.gameObject.activeInHierarchy) continue;
                    if (loopSrc.loop) continue;
                    // We only need to schedule once and then Unity will loop it for us
                    loopSrc._audioSource.PlayScheduled(loopStartAudioTime);
                    loopSrc.loop = true;
                }

                // Schedule next loop
                nextLoopAudioTime += loopDuration;
                nextLoopGameTime += (float)loopDuration;

                // Track loop segment timing (the current musical verse should always finish playing before the finale)
                for (int i = 0; i < segmentCount; i++)
                {
                    _segmentEndAudioTime = loopStartAudioTime + segmentDuration * (i + 1);
                    _segmentEndGameTime = loopStartGameTime + (float)(segmentDuration * (i + 1));

                    // Wait until the next segment
                    while (Time.time < _segmentEndGameTime && !_transitionToFinale)
                    {
                        yield return null;
                    }

                    // Interrupt the remaining segments for the finale
                    if (_transitionToFinale) break;
                }
            }

            // Wait until the bubble has finished expanding
            while (Time.time < CosmicInflationController._finishFormationTime)
            {
                yield return null;
            }

            // Disable audio signals
            foreach (var loopSrc in _loopSources)
            {
                var signal = loopSrc.GetComponent<AudioSignal>();
                if (signal != null)
                {
                    signal.SetSignalActivation(false, 0f);
                }
            }
        }
    }
}
