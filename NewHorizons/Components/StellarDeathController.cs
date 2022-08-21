using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    public class StellarDeathController : MonoBehaviour
    {
        public ParticleSystem[] _explosionParticles;
        public MeshRenderer _shockwave;
        public float _shockwaveLength = 5f;
        public AnimationCurve _shockwaveScale = AnimationCurve.Linear(0.0f, 0.0f, 1f, 100000f);
        public AnimationCurve _shockwaveAlpha = AnimationCurve.Linear(0.0f, 1f, 1f, 0.0f);
        [Space]
        public TessellatedSphereRenderer _surface;
        public Material _supernovaMaterial;
        public AnimationCurve _supernovaScale = new AnimationCurve(new Keyframe(0, 200, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(45, 50000, 1758.508f, 1758.508f, 1f / 3f, 1f / 3f));
        public AnimationCurve _supernovaAlpha = new AnimationCurve(new Keyframe(5, 1, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(15, 1.0002f, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(50, 0, -0.0578f, 1 / 3f, -0.0578f, 1 / 3f));
        [Space]
        public OWAudioSource _audioSource;
        private float _time;
        private float _currentSupernovaScale;
        private Material _localSupernovaMat;
        private bool _isProxy;
        private ParticleSystemRenderer[] _cachedParticleRenderers;

        private void Awake()
        {
            _cachedParticleRenderers = new ParticleSystemRenderer[_explosionParticles.Length];
            for (int index = 0; index < _explosionParticles.Length; ++index)
                _cachedParticleRenderers[index] = _explosionParticles[index].GetComponent<ParticleSystemRenderer>();
        }

        private void OnEnable()
        {
            _shockwave.enabled = true;
            foreach (var particle in _explosionParticles) particle.Play();
            _time = 0.0f;
            _currentSupernovaScale = _supernovaScale.Evaluate(0.0f);
            _localSupernovaMat = new Material(_supernovaMaterial);
            _surface.sharedMaterial = _localSupernovaMat;

            if (_audioSource == null) return;

            _audioSource.AssignAudioLibraryClip(AudioType.Sun_SupernovaWall_LP);
            _audioSource.SetLocalVolume(0.0f);
            _audioSource.Play();
        }

        private void OnDisable()
        {
            _shockwave.enabled = false;

            if (_audioSource == null) return;

            _audioSource.SetLocalVolume(0.0f);
            _audioSource.Stop();
        }

        private void FixedUpdate()
        {
            _time += Time.deltaTime;
            float shockwaveTime = Mathf.Clamp01(_time / _shockwaveLength);
            _shockwave.transform.localScale = Vector3.one * _shockwaveScale.Evaluate(shockwaveTime);
            _shockwave.material.color = Color.Lerp(Color.black, _shockwave.sharedMaterial.color, _shockwaveAlpha.Evaluate(shockwaveTime));
            _currentSupernovaScale = _supernovaScale.Evaluate(_time);
            _surface.transform.localScale = Vector3.one * _currentSupernovaScale;
            _localSupernovaMat.color = Color.Lerp(Color.black, _supernovaMaterial.color, _supernovaAlpha.Evaluate(_time));

            float distanceToPlayer = PlayerState.InDreamWorld() ? 20000f : (Vector3.Distance(transform.position, Locator.GetPlayerCamera().transform.position) - GetSupernovaRadius());

            if (_isProxy) return;

            float dt = Mathf.InverseLerp(12000f, 0.0f, distanceToPlayer);
            _audioSource.SetLocalVolume(Mathf.Lerp(0.0f, 1f, dt * dt) * Mathf.InverseLerp(0.0f, 5f, _time));
            RumbleManager.UpdateSupernova(distanceToPlayer);
        }

        public float GetSupernovaRadius() => _currentSupernovaScale;

        public void SetIsProxy(bool isProxy) => _isProxy = isProxy;

        public void SetParticlesVisibility(bool visible)
        {
            foreach (var particleRenderer in _cachedParticleRenderers) particleRenderer.enabled = visible;
        }

        public void SetRenderingEnabled(bool renderingEnabled)
        {
            if (!enabled) return;
            _shockwave.enabled = renderingEnabled;
            SetParticlesVisibility(renderingEnabled);
        }
    }
}
