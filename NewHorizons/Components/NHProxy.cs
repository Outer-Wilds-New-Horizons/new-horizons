using NewHorizons.Components.SizeControllers;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;
namespace NewHorizons.Components
{
    public class NHProxy : ProxyPlanet
    {
        public string astroName;

        public GameObject _planet;
        public GameObject _star;
        public StarEvolutionController _starEvolutionController;
        private Renderer[] _starRenderers;
        private TessellatedRenderer[] _starTessellatedRenderers;
        private ParticleSystemRenderer[] _starParticleRenderers;
        private SolarFlareEmitter _solarFlareEmitter;
        public CloudLightningGenerator _lightningGenerator;
        public Renderer _topClouds;
        public NHSupernovaPlanetEffectController _supernovaPlanetEffectController;
        public StellarRemnantProxy _stellarRemnant;
        public float _baseRealObjectDiameter;

        public override void Awake()
        {
            ProxyHandler.RegisterProxy(this);
            base.Awake();

            _mieCurveMaxVal = 0.1f;
            _mieCurve = AnimationCurve.EaseInOut(0.0011f, 1, 1, 0);
            _fogCurve = AnimationCurve.Linear(0, 1, 1, 0);

            // The star part cant be disabled like the rest and we have to manually disable the renderers
            // Else it can stop the supernova effect mid way through
            if (_starEvolutionController == null) _starEvolutionController = GetComponentInChildren<StarEvolutionController>();
            if (_star == null) _star = _starEvolutionController?.gameObject;
            
            if (_star != null)
            {
                _starRenderers = _star.GetComponentsInChildren<Renderer>();
                _starTessellatedRenderers = _star.GetComponentsInChildren<TessellatedRenderer>();
                _starParticleRenderers = _star.GetComponentsInChildren<ParticleSystemRenderer>();
                _solarFlareEmitter = _star.GetComponentInChildren<SolarFlareEmitter>();
            }

            if (_lightningGenerator == null) _lightningGenerator = GetComponentInChildren<CloudLightningGenerator>();

            if (_supernovaPlanetEffectController == null) _supernovaPlanetEffectController = GetComponentInChildren<NHSupernovaPlanetEffectController>();
            
            // Start off
            _outOfRange = false;
            ToggleRendering(false);
        }

        public override void OnDestroy()
        {
            ProxyHandler.UnregisterProxy(this);
            base.OnDestroy();
        }

        public override void Initialize()
        {
            AstroObject astroObject = AstroObjectLocator.GetAstroObject(astroName);
            _realObjectTransform = astroObject.transform;
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

        public override void Update()
        {
            if (_planet == null || !_planet.activeSelf)
            {
                _outOfRange = false;
                ToggleRendering(false);
                enabled = false;
                return;
            }

            if (_stellarRemnant != null)
            {
                if (_stellarRemnant.IsActivated())
                {
                    _realObjectDiameter = _stellarRemnant._realObjectDiameter;
                    if (!_stellarRemnant.IsRenderingOn()) ToggleRendering(_outOfRange);
                }
                else
                {
                    _realObjectDiameter = _baseRealObjectDiameter;
                    if (_stellarRemnant.IsRenderingOn()) ToggleRendering(_outOfRange);
                }
            }

            if (_starEvolutionController != null && _star != null && (_star.activeSelf || _star.activeInHierarchy))
            {
                if (_starEvolutionController.HasSupernovaStarted()) _realObjectDiameter = _starEvolutionController.GetSupernovaRadius();
                else if (_starEvolutionController.IsCollapsing()) Mathf.Lerp(_baseRealObjectDiameter, 0, _starEvolutionController.GetCollapseProgress());
            }

            base.Update();
        }

        public override void ToggleRendering(bool on)
        {
            if (_stellarRemnant != null)
            {
                _stellarRemnant.ToggleRendering(on);
                on = on && !_stellarRemnant.IsActivated();
            }

            base.ToggleRendering(on);

            foreach (Transform child in transform)
            {
                if (child.gameObject == _star) continue;
                if (child.gameObject == _stellarRemnant?.gameObject) continue;
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

            if (_supernovaPlanetEffectController != null)
            {
                if (on) _supernovaPlanetEffectController.Enable();
                else _supernovaPlanetEffectController.Disable();
            }
        }

        public override void UpdateScale(float scaleMultiplier, float viewDistance)
        {
            if (_stellarRemnant != null) _stellarRemnant.UpdateScale(scaleMultiplier, viewDistance);
            base.UpdateScale(scaleMultiplier, viewDistance);
        }
    }
}