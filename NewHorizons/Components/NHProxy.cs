using NewHorizons.Components.Props;
using NewHorizons.Components.SizeControllers;
using NewHorizons.Handlers;
using NewHorizons.Utility.OuterWilds;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NHProxy : ProxyPlanet
    {
        public string astroName;

        public GameObject planet;

        private GameObject[] _stars;

        public StarEvolutionController[] StarEvolutionControllers { get; private set; }

        private IEnumerable<Renderer> _starRenderers = new List<Renderer>();
        private IEnumerable<TessellatedRenderer> _starTessellatedRenderers = new List<TessellatedRenderer>();
        private IEnumerable<ParticleSystemRenderer> _starParticleRenderers = new List<ParticleSystemRenderer>();
        private IEnumerable<SolarFlareEmitter> _solarFlareEmitter = new List<SolarFlareEmitter>();

        // Public stuff from the builder
        public CloudLightningGenerator lightningGenerator;
        public Renderer topClouds;
        public NHSupernovaPlanetEffectController supernovaPlanetEffectController;
        public float baseRealObjectDiameter;

        public GameObject root;
        public GameObject stellarRemnantGO;

        public override void Awake()
        {
            ProxyHandler.RegisterProxy(this);
            base.Awake();

            _mieCurveMaxVal = 0.1f;
            _mieCurve = AnimationCurve.EaseInOut(0.0011f, 1, 1, 0);
            _fogCurve = AnimationCurve.Linear(0, 1, 1, 0);

            // The star part cant be disabled like the rest and we have to manually disable the renderers
            // Else it can stop the supernova effect mid way through
            StarEvolutionControllers = GetComponentsInChildren<StarEvolutionController>(true);
            _stars = StarEvolutionControllers.Select(x => x.gameObject).ToArray();

            foreach (var star in _stars)
            {
                _starRenderers = _starRenderers.Concat(star.GetComponentsInChildren<Renderer>(true));
                _starTessellatedRenderers = _starTessellatedRenderers.Concat(star.GetComponentsInChildren<TessellatedRenderer>(true));
                _starParticleRenderers = _starParticleRenderers.Concat(star.GetComponentsInChildren<ParticleSystemRenderer>(true));
                _solarFlareEmitter = _solarFlareEmitter.Append(star.GetComponentInChildren<SolarFlareEmitter>(true));
            }

            var progenitorEvolutionController = root.GetComponentInChildren<StarEvolutionController>(true);
            if (progenitorEvolutionController != null && stellarRemnantGO != null)
            {
                progenitorEvolutionController.SetStellarRemnant(stellarRemnantGO);
            }

            if (lightningGenerator == null) lightningGenerator = GetComponentInChildren<CloudLightningGenerator>(true);

            if (supernovaPlanetEffectController == null) supernovaPlanetEffectController = GetComponentInChildren<NHSupernovaPlanetEffectController>(true);
            
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
            if (planet == null || !planet.activeSelf)
            {
                _outOfRange = false;
                ToggleRendering(false);
                enabled = false;
                return;
            }

            base.Update();
        }

        public override void ToggleRendering(bool on)
        {
            base.ToggleRendering(on);

            foreach (Transform child in transform)
            {
                // The first layer of children are the different states of the proxy; root, remnant, eventually quantum states
                foreach (Transform grandChild in child)
                {
                    // Don't disable any stars
                    if (_stars.Contains(grandChild.gameObject)) continue;

                    // Toggle the grandchildren
                    grandChild.gameObject.SetActive(on);
                }
            }

            foreach (var solarFlare in _solarFlareEmitter)
            {
                solarFlare.gameObject.SetActive(on);
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

            if (topClouds != null)
            {
                topClouds.enabled = on;
            }

            if (lightningGenerator != null)
            {
                lightningGenerator.enabled = on;
            }

            if (supernovaPlanetEffectController != null)
            {
                if (on) supernovaPlanetEffectController.Enable();
                else supernovaPlanetEffectController.Disable();
            }
        }

        public override void UpdateScale(float scaleMultiplier, float viewDistance)
        {
            base.UpdateScale(scaleMultiplier, viewDistance);
        }
    }
}