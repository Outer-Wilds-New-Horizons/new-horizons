using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProxyPlanet;

namespace NewHorizons.Components
{
    public class StellarRemnantProxy : MonoBehaviour
    {
        public float _realObjectDiameter;
        public Renderer _atmosphere;
        public Renderer _fog;
        public float _mieCurveMinDistance = 45000f;
        public float _mieCurveMaxDistance = 750000f;
        public float _mieCurveMinVal;
        public float _mieCurveMaxVal;
        public AnimationCurve _mieCurve;
        public float _fogCurveMinDistance = 45000f;
        public float _fogCurveMaxDistance = 750000f;
        public float _fogCurveMinVal;
        public float _fogCurveMaxVal;
        public AnimationCurve _fogCurve;
        public Material _atmosphereMaterial;
        public float _baseAtmoMatShellInnerRadius;
        public float _baseAtmoMatShellOuterRadius;
        public bool _hasAtmosphere;
        public bool _hasFog;
        public Material _fogMaterial;
        public GameObject _star;
        public Renderer[] _starRenderers;
        public TessellatedRenderer[] _starTessellatedRenderers;
        public ParticleSystemRenderer[] _starParticleRenderers;
        public SolarFlareEmitter _solarFlareEmitter;
        public CloudLightningGenerator _lightningGenerator;
        public Renderer _topClouds;

        private bool _renderingOn;

        public void Awake()
        {
            _mieCurveMaxVal = 0.1f;
            _mieCurve = AnimationCurve.EaseInOut(0.0011f, 1, 1, 0);
            _fogCurve = AnimationCurve.Linear(0, 1, 1, 0);

            // The star part cant be disabled like the rest and we have to manually disable the renderers
            // Else it can stop the supernova effect mid way through
            _star = GetComponentInChildren<TessellatedSphereRenderer>(true)?.transform?.parent?.gameObject;

            if (_star != null)
            {
                _starRenderers = _star.GetComponentsInChildren<Renderer>();
                _starTessellatedRenderers = _star.GetComponentsInChildren<TessellatedRenderer>();
                _starParticleRenderers = _star.GetComponentsInChildren<ParticleSystemRenderer>();
                _solarFlareEmitter = _star.GetComponentInChildren<SolarFlareEmitter>();
            }

            if (_lightningGenerator == null) _lightningGenerator = GetComponentInChildren<CloudLightningGenerator>();

            ToggleRendering(false);
        }

        public void ToggleRendering(bool on)
        {
            on = on && IsActiveAndEnabled();

            _renderingOn = on;

            if (_atmosphere != null) _atmosphere.enabled = on;
            if (_fog != null) _fog.enabled = on;

            foreach (Transform child in transform)
            {
                if (child.gameObject == _star) continue;
                child.gameObject.SetActive(on);
            }

            if (_star != null)
            {
                if (_solarFlareEmitter != null)
                {
                    _solarFlareEmitter.gameObject.SetActive(on);
                }

                foreach (var renderer in _starRenderers)
                {
                    renderer.enabled = on;
                }

                foreach (var renderer in _starTessellatedRenderers)
                {
                    renderer.enabled = on;
                }

                foreach (var renderer in _starParticleRenderers)
                {
                    renderer.enabled = on;
                }
            }

            if (_topClouds != null)
            {
                _topClouds.enabled = on;
            }

            if (_lightningGenerator != null)
            {
                _lightningGenerator.enabled = on;
            }
        }

        public void UpdateScale(float scaleMultiplier, float viewDistance)
        {
            if (_hasAtmosphere)
            {
                _atmosphereMaterial.SetFloat(propID_AtmoInnerRadius, _baseAtmoMatShellInnerRadius * scaleMultiplier);
                _atmosphereMaterial.SetFloat(propID_AtmoOuterRadius, _baseAtmoMatShellOuterRadius * scaleMultiplier);
                _atmosphereMaterial.SetFloat(propID_MieConstant, Mathf.Lerp(_mieCurveMinVal, _mieCurveMaxVal, _mieCurve.Evaluate(Mathf.InverseLerp(_mieCurveMinDistance, _mieCurveMaxDistance, viewDistance))));
            }
            if (_hasFog) _fogMaterial.SetFloat(propID_FogDensity, Mathf.Lerp(_fogCurveMinVal, _fogCurveMaxVal, _fogCurve.Evaluate(Mathf.InverseLerp(_fogCurveMinDistance, _fogCurveMaxDistance, viewDistance))));
        }

        public void Initialize()
        {
            if (_atmosphere != null)
            {
                _hasAtmosphere = true;
                _atmosphereMaterial = new Material(_atmosphere.sharedMaterial);
                _baseAtmoMatShellInnerRadius = _atmosphereMaterial.GetFloat(propID_AtmoInnerRadius);
                _baseAtmoMatShellOuterRadius = _atmosphereMaterial.GetFloat(propID_AtmoOuterRadius);
                _atmosphere.sharedMaterial = _atmosphereMaterial;
            }
            if (_fog != null)
            {
                _hasFog = true;
                _fogMaterial = new Material(_fog.sharedMaterial);
                _fogMaterial.SetFloat(propID_LODFade, 1f);
                _fog.sharedMaterial = _fogMaterial;
            }
        }

        public StellarRemnantController _stellarRemnantController;
        public void SetStellarRemnantController(StellarRemnantController controller)
        {
            _stellarRemnantController = controller;
            controller.SetProxy(this);
        }

        public bool IsActiveAndEnabled() => _stellarRemnantController.isActiveAndEnabled;
        public bool IsRenderingOn() => _renderingOn;
    }
}
