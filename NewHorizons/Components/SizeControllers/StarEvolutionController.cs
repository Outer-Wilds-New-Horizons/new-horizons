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

        private StarEvolutionController _proxy;

        public UnityEvent SupernovaStart = new UnityEvent();

        private float maxScale;
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");

        void Start()
        {
            var sun = GameObject.FindObjectOfType<SunController>();
            _collapseStartSurfaceMaterial = new Material(sun._collapseStartSurfaceMaterial);
            _collapseEndSurfaceMaterial = new Material(sun._collapseEndSurfaceMaterial);
            _startSurfaceMaterial = new Material(sun._startSurfaceMaterial);
            _endSurfaceMaterial = new Material(sun._endSurfaceMaterial);

            var supernovaSurfaceColorRamp = supernova._surface.sharedMaterial.GetTexture(ColorRamp);

            // Copy over the material that was set in star builder
            _collapseStartSurfaceMaterial.SetTexture(ColorRamp, supernovaSurfaceColorRamp);
            _collapseEndSurfaceMaterial.SetTexture(ColorRamp, supernovaSurfaceColorRamp);
            _startSurfaceMaterial.SetTexture(ColorRamp, supernovaSurfaceColorRamp);
            _endSurfaceMaterial.SetTexture(ColorRamp, supernovaSurfaceColorRamp);

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
                _endSurfaceMaterial.color = _endColour;
            }

            _heatVolume = GetComponentInChildren<HeatHazardVolume>();
            _destructionVolume = GetComponentInChildren<DestructionVolume>();

            if (atmosphere != null)
            {
                _fog = atmosphere?.GetComponentInChildren<PlanetaryFogController>();
                _atmosphereRenderers = atmosphere?.transform?.Find("AtmoSphere")?.GetComponentsInChildren<MeshRenderer>();
            }

            if (WillExplode) GlobalMessenger.AddListener("TriggerSupernova", Die);

            if (scaleCurve != null)
            {
                maxScale = scaleCurve.keys.Select(x => x.value).Max() * size;
            }
            else
            {
                maxScale = 0;
                scaleCurve = new AnimationCurve();
                scaleCurve.AddKey(0, 1);
            }

            _flareEmitter = GetComponentInChildren<SolarFlareEmitter>();
        }

        public void OnDestroy()
        {
            if (WillExplode) GlobalMessenger.RemoveListener("TriggerSupernova", Die);
        }

        public void SetProxy(StarEvolutionController proxy)
        {
            _proxy = proxy;
            _proxy.supernova.SetIsProxy(true);
        }

        public void Die()
        {
            _isCollapsing = true;
            _collapseStartSize = CurrentScale;
            _collapseTimer = 0f;

            if (_proxy != null) _proxy.Die();
        }

        protected new void FixedUpdate()
        {
            _age += Time.deltaTime;

            var ageValue = _age / (lifespan * 60f);

            // If we've gone supernova and its been 45 seconds that means it has faded out and is gone
            // The 45 is from the animation curve used for the supernova alpha
            if (_isSupernova)
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
                return;
            }

            Color currentColour;

            if (!_isCollapsing)
            {
                base.FixedUpdate();

                // Only do colour transition stuff if they set an end colour
                if (EndColour != null)
                {
                    // Use the age if theres no resizing happening, else make it get redder the larger it is or wtv
                    var t = ageValue;
                    if (maxScale > 0) t = CurrentScale / maxScale;
                    currentColour = Color.Lerp(_startColour, _endColour, t);
                    supernova._surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, t);
                }
                else
                {
                    currentColour = _startColour;
                }

                if (_flareEmitter != null) _flareEmitter._tint = currentColour;
            }
            else
            {
                // When its collapsing we directly take over the scale
                var t = _collapseTimer / collapseTime;
                CurrentScale = Mathf.Lerp(_collapseStartSize, 0, t);
                transform.localScale = Vector3.one * CurrentScale;
                _collapseTimer += Time.deltaTime;

                currentColour = Color.Lerp(_endColour, Color.white, t);

                supernova._surface._materials[0].Lerp(_collapseStartSurfaceMaterial, _collapseEndSurfaceMaterial, t);

                // After the collapse is done we go supernova
                if (_collapseTimer > collapseTime)
                {
                    SupernovaStart.Invoke();
                    supernova.enabled = true;
                    _isSupernova = true;
                    _supernovaStartTime = Time.time;
                    if (atmosphere != null) atmosphere.SetActive(false);
                    if (_destructionVolume != null) _destructionVolume._deathType = DeathType.Supernova;
                    return;
                }
            }

            // This is just all the scales stuff for the atmosphere effects
            if (_fog != null)
            {
                _fog.fogRadius = CurrentScale * StarBuilder.OuterRadiusRatio;
                _fog.lodFadeDistance = CurrentScale * StarBuilder.OuterRadiusRatio / 3f;

                // The colour thing goes over one
                var max = Math.Max(currentColour.g, Math.Max(currentColour.b, currentColour.r));
                var fogColour = currentColour / max / 1.5f;
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
                    lod.material.SetColor("_AtmosFar", currentColour);
                    lod.material.SetColor("_AtmosNear", currentColour);
                    lod.material.SetColor("_SkyColor", currentColour);
                }
            }
        }
    }
}
