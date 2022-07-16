using NewHorizons.Builder.Body;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components.SizeControllers
{
    public class StarEvolutionController : SizeController
    {
        public GameObject atmosphere;
        public SupernovaEffectController supernova;
        public bool WillExplode { get; set; }
        public MColor StartColour { get; set; }
        public MColor EndColour { get; set; }
        public Texture normalRamp;
        public Texture collapseRamp;

        private Color _startColour;
        private Color _endColour;

        private PlanetaryFogController _fog;
        private MeshRenderer[] _atmosphereRenderers;
        private HeatHazardVolume _heatVolume;
        private DestructionVolume _destructionVolume;
        private SolarFlareEmitter _flareEmitter;

        private bool _isCollapsing;
        private float _collapseStartSize;
        private float _collapseTimer;

        public float collapseTime = 5f; // seconds
        public float lifespan = 22f; // minutes
        private float _age;

        private bool _isSupernova;
        private float _supernovaStartTime;

        private Material _collapseStartSurfaceMaterial;
        private Material _collapseEndSurfaceMaterial;
        private Material _startSurfaceMaterial;
        private Material _endSurfaceMaterial;
        private Texture _normalRamp;
        private Texture _collapseRamp;

        private StarEvolutionController _proxy;

        public UnityEvent SupernovaStart = new UnityEvent();

        private float maxScale;
        private float minScale;
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");

        private Color _currentColour;

        void Start()
        {
            var sun = GameObject.FindObjectOfType<SunController>();
            _collapseStartSurfaceMaterial = new Material(sun._collapseStartSurfaceMaterial);
            _collapseEndSurfaceMaterial = new Material(sun._collapseEndSurfaceMaterial);
            _startSurfaceMaterial = new Material(sun._startSurfaceMaterial);
            _endSurfaceMaterial = new Material(sun._endSurfaceMaterial);

            if (normalRamp == null)
            {
                _normalRamp = sun._startSurfaceMaterial.GetTexture(ColorRamp);
            } else
            {
                _normalRamp = normalRamp;
            }
            if (collapseRamp == null)
            {
                _collapseRamp = sun._collapseStartSurfaceMaterial.GetTexture(ColorRamp);
            } else
            {
                _collapseRamp = collapseRamp;
            }

            // Copy over the material that was set in star builder
            _collapseStartSurfaceMaterial.SetTexture(ColorRamp, _collapseRamp);
            _collapseEndSurfaceMaterial.SetTexture(ColorRamp, _collapseRamp);
            _startSurfaceMaterial.SetTexture(ColorRamp, _normalRamp);
            _endSurfaceMaterial.SetTexture(ColorRamp, _normalRamp);

            if (StartColour == null)
            {
                _startColour = _startSurfaceMaterial.color;
            }
            else
            {
                _startColour = StartColour.ToColor();
                _startSurfaceMaterial.color = _startColour;
            }

            if (EndColour == null)
            {
                _endColour = _startColour;
                _endSurfaceMaterial.color = _startColour;
            }
            else
            {
                _endColour = EndColour.ToColor();
                _endSurfaceMaterial.color = _startColour * 4.5948f;
            }

            _heatVolume = GetComponentInChildren<HeatHazardVolume>();
            _destructionVolume = GetComponentInChildren<DestructionVolume>();

            if (atmosphere != null)
            {
                _fog = atmosphere?.GetComponentInChildren<PlanetaryFogController>();
                _atmosphereRenderers = atmosphere?.transform?.Find("AtmoSphere")?.GetComponentsInChildren<MeshRenderer>();
            }

            if (WillExplode) GlobalMessenger.AddListener("TriggerSupernova", StartCollapse);

            if (scaleCurve != null)
            {
                maxScale = scaleCurve.keys.Select(x => x.value).Max() * size;
                minScale = scaleCurve.keys.Select(x => x.value).Min() * size;
            }
            else
            {
                maxScale = 0;
                minScale = 0;
                scaleCurve = new AnimationCurve();
                scaleCurve.AddKey(0, 1);
            }

            _flareEmitter = GetComponentInChildren<SolarFlareEmitter>();
        }

        public void OnDestroy()
        {
            if (WillExplode) GlobalMessenger.RemoveListener("TriggerSupernova", StartCollapse);
        }

        public void SetProxy(StarEvolutionController proxy)
        {
            _proxy = proxy;
            _proxy.supernova.SetIsProxy(true);
        }

        private void UpdateMainSequence()
        {
            // Only do colour transition stuff if they set an end colour
            if (EndColour != null)
            {
                // Use the age if theres no resizing happening, else make it get redder the larger it is or wtv
                var t = _age / (lifespan * 60f);
                if (maxScale != minScale) t = Mathf.InverseLerp(minScale, maxScale, CurrentScale);

                // Only go to 98% else if it reaches the endSurfaceMaterial it'll morb
                if (t < 0.98f)
                {
                    _currentColour = Color.Lerp(_startColour, _endColour, t);
                    supernova._surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, t);
                }
                else
                {
                    _currentColour = _endColour;
                }
            }
            else
            {
                _currentColour = _startColour;
            }

            if (_flareEmitter != null) _flareEmitter._tint = _currentColour;
        }

        private void UpdateCollapse()
        {
            // When its collapsing we directly take over the scale
            var t = _collapseTimer / collapseTime;
            CurrentScale = Mathf.Lerp(_collapseStartSize, 0, t);
            transform.localScale = Vector3.one * CurrentScale;
            _collapseTimer += Time.deltaTime;

            _currentColour = Color.Lerp(_endColour, Color.white, t);

            supernova._surface._materials[0].Lerp(_collapseStartSurfaceMaterial, _collapseEndSurfaceMaterial, t);

            // After the collapse is done we go supernova
            if (_collapseTimer > collapseTime) StartSupernova();
        }

        private void UpdateSupernova()
        {
            // Reset the scale back to normal bc now its just the supernova scaling itself + destruction and heat volumes
            transform.localScale = Vector3.one;

            // Make the destruction volume scale slightly smaller so you really have to be in the supernova to die
            if (_destructionVolume != null) _destructionVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius() * 0.9f;
            if (_heatVolume != null) _heatVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius();

            if (Time.time > _supernovaStartTime + 45f)
            {
                // Just turn off the star entirely
                base.gameObject.SetActive(false);
            }
        }

        public void StartCollapse()
        {
            Logger.LogVerbose($"{gameObject.transform.root.name} started collapse");

            _isCollapsing = true;
            _collapseStartSize = CurrentScale;
            _collapseTimer = 0f;
            supernova._surface._materials[0].CopyPropertiesFromMaterial(_collapseStartSurfaceMaterial);

            if (_proxy != null) _proxy.StartCollapse();
        }

        private void StartSupernova()
        {
            Logger.LogVerbose($"{gameObject.transform.root.name} started supernova");

            SupernovaStart.Invoke();
            supernova.enabled = true;
            _isSupernova = true;
            _supernovaStartTime = Time.time;
            if (atmosphere != null) atmosphere.SetActive(false);
            if (_destructionVolume != null) _destructionVolume._deathType = DeathType.Supernova;
        }

        protected new void FixedUpdate()
        {
            _age += Time.deltaTime;

            // If we've gone supernova and its been 45 seconds that means it has faded out and is gone
            // The 45 is from the animation curve used for the supernova alpha
            if (_isSupernova)
            {
                UpdateSupernova();
                return;
            }
            
            if (!_isCollapsing)
            {
                base.FixedUpdate();
                UpdateMainSequence();
            }
            else
            {
                UpdateCollapse();
                if (_isSupernova) return;
            }
            
            // This is just all the scales stuff for the atmosphere effects
            if (_fog != null)
            {
                _fog.fogRadius = CurrentScale * StarBuilder.OuterRadiusRatio;
                _fog.lodFadeDistance = CurrentScale * StarBuilder.OuterRadiusRatio / 3f;

                // The colour thing goes over one
                var max = Math.Max(_currentColour.g, Math.Max(_currentColour.b, _currentColour.r));
                var fogColour = _currentColour / max / 1.5f;
                fogColour.a = 1f;
                _fog.fogTint = fogColour;
                _fog._fogTint = fogColour;
            }

            if (_atmosphereRenderers != null && _atmosphereRenderers.Count() > 0)
            {
                foreach (var lod in _atmosphereRenderers)
                {
                    lod.material.SetFloat("_InnerRadius", CurrentScale);
                    lod.material.SetFloat("_OuterRadius", CurrentScale * StarBuilder.OuterRadiusRatio);
                    lod.material.SetColor("_SkyColor", _currentColour);
                }
            }
        }
    }
}
