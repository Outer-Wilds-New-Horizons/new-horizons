using UnityEngine;

namespace NewHorizons.Components.Stars
{
    public class StellarDeathController : MonoBehaviour
    {
        public ParticleSystem[] explosionParticles;
        public MeshRenderer shockwave;
        public float shockwaveLength = 5f;
        public AnimationCurve shockwaveScale = AnimationCurve.Linear(0.0f, 0.0f, 1f, 100000f);
        public AnimationCurve shockwaveAlpha = AnimationCurve.Linear(0.0f, 1f, 1f, 0.0f);

        public TessellatedSphereRenderer surface;
        public Material supernovaMaterial;
        public AnimationCurve supernovaScale = new AnimationCurve(new Keyframe(0, 200, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(45, 50000, 1758.508f, 1758.508f, 1f / 3f, 1f / 3f));
        public AnimationCurve supernovaAlpha = new AnimationCurve(new Keyframe(5, 1, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(15, 1.0002f, 0, 0, 1f / 3f, 1f / 3f), new Keyframe(50, 0, -0.0578f, 1 / 3f, -0.0578f, 1 / 3f));

        public OWAudioSource audioSource;
        public StellarDeathController mainStellerDeathController;

        private float _time;
        private float _currentSupernovaScale;
        private Material _localSupernovaMat;
        private bool _isProxy;
        private bool _renderingEnabled = true;
        private ParticleSystemRenderer[] _cachedParticleRenderers;

        public void Awake()
        {
            _cachedParticleRenderers = new ParticleSystemRenderer[explosionParticles.Length];
            for (int index = 0; index < explosionParticles.Length; ++index)
                _cachedParticleRenderers[index] = explosionParticles[index].GetComponent<ParticleSystemRenderer>();
        }

        public void Activate()
        {
            enabled = true;

            var proxy = IsProxy() ? this.GetComponentInParent<NHProxy>() : null;

            if (proxy == null || proxy._renderingEnabled)
            {
                shockwave.enabled = _renderingEnabled;
                for (int i = 0; i < explosionParticles.Length; i++)
                {
                    explosionParticles[i].Play();
                    _cachedParticleRenderers[i].enabled = _renderingEnabled;
                }
            }

            _time = 0.0f;
            _currentSupernovaScale = supernovaScale.Evaluate(0.0f);
            _localSupernovaMat = new Material(supernovaMaterial);
            surface.sharedMaterial = _localSupernovaMat;

            if (audioSource == null) return;

            audioSource.AssignAudioLibraryClip(AudioType.Sun_SupernovaWall_LP);
            audioSource.SetLocalVolume(0);
            audioSource.Play();
        }

        public void Deactivate()
        {
            enabled = false;
            shockwave.enabled = false;

            if (audioSource == null) return;

            audioSource.SetLocalVolume(0);
            audioSource.Stop();
        }

        public void FixedUpdate()
        {
            if (mainStellerDeathController != null) _time = mainStellerDeathController._time;
            else _time += Time.deltaTime;

            float shockwaveTime = Mathf.Clamp01(_time / shockwaveLength);
            shockwave.transform.localScale = Vector3.one * shockwaveScale.Evaluate(shockwaveTime);
            shockwave.material.color = Color.Lerp(Color.black, shockwave.sharedMaterial.color, shockwaveAlpha.Evaluate(shockwaveTime));
            _currentSupernovaScale = supernovaScale.Evaluate(_time);
            surface.transform.localScale = Vector3.one * _currentSupernovaScale;
            _localSupernovaMat.color = Color.Lerp(Color.black, supernovaMaterial.color, supernovaAlpha.Evaluate(_time));

            float distanceToPlayer = PlayerState.InDreamWorld() ? 20000f : Vector3.Distance(transform.position, Locator.GetPlayerCamera().transform.position) - GetSupernovaRadius();

            if (_isProxy) return;

            if (audioSource != null)
            {
                float dt = Mathf.InverseLerp(12000f, 0.0f, distanceToPlayer);
                audioSource.SetLocalVolume(Mathf.Lerp(0.0f, 1f, dt * dt) * Mathf.InverseLerp(0.0f, 5f, _time));
                audioSource.maxDistance = shockwaveScale.Evaluate(shockwaveTime);
            }

            RumbleManager.UpdateSupernova(distanceToPlayer);
        }

        public float GetSupernovaRadius() => _currentSupernovaScale;

        public bool IsProxy() => _isProxy;

        public void SetIsProxy(bool isProxy) => _isProxy = isProxy;

        public void SetParticlesVisibility(bool visible)
        {
            foreach (var particleRenderer in _cachedParticleRenderers)
            {
                particleRenderer.enabled = visible;
            }
        }

        public void SetRenderingEnabled(bool renderingEnabled)
        {
            _renderingEnabled = renderingEnabled;
            if (!enabled) return;
            shockwave.enabled = renderingEnabled;
            SetParticlesVisibility(renderingEnabled);
        }
    }
}
