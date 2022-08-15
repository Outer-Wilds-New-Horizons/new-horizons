using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility;
using System.Collections.Generic;
using UnityEngine;
namespace NewHorizons.Components
{
    public class NHProxy : ProxyPlanet
    {
        public string astroName;

        private GameObject _star;
        private Renderer[] _starRenderers;
        private TessellatedRenderer[] _starTessellatedRenderers;
        private ParticleSystemRenderer[] _starParticleRenderers;
        private SolarFlareEmitter _solarFlareEmitter;
        public CloudLightningGenerator _lightningGenerator;
        public MeshRenderer _mainBody;

        public override void Awake()
        {
            base.Awake();

            // The star part cant be disabled like the rest and we have to manually disable the renderers
            // Else it can stop the supernova effect mid way through
            _star = GetComponentInChildren<StarEvolutionController>()?.gameObject;

            if (_star != null)
            {
                _starRenderers = _star.GetComponentsInChildren<Renderer>();
                _starTessellatedRenderers = _star.GetComponentsInChildren<TessellatedRenderer>();
                _starParticleRenderers = _star.GetComponentsInChildren<ParticleSystemRenderer>();
                _solarFlareEmitter = _star.GetComponentInChildren<SolarFlareEmitter>();
            }

            if (_lightningGenerator == null) _lightningGenerator = GetComponentInChildren<CloudLightningGenerator>();
            
            // Start off
            _outOfRange = false;
            ToggleRendering(false);
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

        public override void ToggleRendering(bool on)
        {
            base.ToggleRendering(on);

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

                if (_mainBody != null)
                {
                    _mainBody.enabled = on;
                }

                if (_lightningGenerator != null)
                {
                    _lightningGenerator.enabled = on;
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
        }
    }
}