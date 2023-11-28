using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class NHDayNightAudioVolume : AudioVolume
    {
        public float dayWindow = 180f;
        public string dayAudio;
        public string nightAudio;
        public string sunName;
        public IModBehaviour modBehaviour;
        public float volume;

        private OWAudioSource _daySource;
        private OWAudioSource _nightSource;
        private OWAudioMixer.TrackName _track;

        private Transform _planetTransform;
        private Transform _sunTransform;

        private bool _wasDay;

        public override void Start()
        {
            _fadeSeconds = 2f;

            _planetTransform = gameObject.GetAttachedOWRigidbody().transform;
            _sunTransform = AstroObjectLocator.GetAstroObject(sunName).GetAttachedOWRigidbody().transform;

            if (!string.IsNullOrEmpty(nightAudio))
            {
                var nightGO = new GameObject("NightAudioSource");
                nightGO.transform.SetParent(transform);
                nightGO.AddComponent<AudioSource>();
                _nightSource = nightGO.AddComponent<OWAudioSource>();
            }

            if (!string.IsNullOrEmpty(dayAudio))
            {
                var dayGO = new GameObject("DayAudioSource");
                dayGO.transform.SetParent(transform);
                dayGO.AddComponent<AudioSource>();
                _daySource = dayGO.AddComponent<OWAudioSource>();
            }

            enabled = false;
        }

        public override void Init()
        {
            base.Init();

            if (_daySource != null)
            {
                _daySource.Stop();
                _daySource.SetLocalVolume(0f);

                _daySource.rolloffMode = AudioRolloffMode.Custom;
                _daySource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0f, 1f, 1f, 1f));
                _daySource.spatialBlend = 1f;
                _daySource.spread = 180f;
                _daySource.dopplerLevel = 0f;
                _daySource.SetMaxVolume(volume);
                _daySource.SetTrack(_track);
                _daySource.loop = true;
                AudioUtilities.SetAudioClip(_daySource, dayAudio, modBehaviour);
            }

            if (_nightSource != null)
            {
                _nightSource.Stop();
                _nightSource.SetLocalVolume(0f);

                _nightSource.rolloffMode = AudioRolloffMode.Custom;
                _nightSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Linear(0f, 1f, 1f, 1f));
                _nightSource.spatialBlend = 1f;
                _nightSource.spread = 180f;
                _nightSource.dopplerLevel = 0f;
                _nightSource.SetMaxVolume(volume);
                _nightSource.SetTrack(_track);
                _nightSource.loop = true;
                AudioUtilities.SetAudioClip(_nightSource, nightAudio, modBehaviour);
            }
        }

        public override void Activate()
        {
            enabled = true;
            _isActive = true;
            UpdatePlayState(IsDay());
        }

        public override void Deactivate()
        {
            enabled = false;
            _isActive = false;

            _daySource?.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP, 0f);
            _nightSource?.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP, 0f);
        }

        public void Update()
        {
            var isDay = IsDay();

            if (isDay)
            {
                if (!_wasDay)
                {
                    UpdatePlayState(isDay);
                }
            }
            else if (_wasDay)
            {
                UpdatePlayState(isDay);
            }

            _wasDay = isDay;
        }

        private void UpdatePlayState(bool isDay)
        {
            if (!_initialized)
            {
                Init();
            }
            if (isDay)
            {
                _daySource?.FadeIn(_fadeSeconds, false, _randomizePlayhead, 1f);
                _nightSource?.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP, 0f);
            }
            else
            {
                _daySource?.FadeOut(_fadeSeconds, _pauseOnFadeOut ? OWAudioSource.FadeOutCompleteAction.PAUSE : OWAudioSource.FadeOutCompleteAction.STOP, 0f);
                _nightSource?.FadeIn(_fadeSeconds, false, _randomizePlayhead, 1f);
            }
        }

        private bool IsDay()
        {
            return Vector3.Angle(_planetTransform.position - Locator.GetPlayerTransform().position, Locator.GetPlayerTransform().position - _sunTransform.position) < dayWindow * 0.5f;
        }

        public void SetTrack(OWAudioMixer.TrackName track)
        {
            _track = track;
            _nightSource?.SetTrack(track);
            _daySource?.SetTrack(track);
        }
    }
}
