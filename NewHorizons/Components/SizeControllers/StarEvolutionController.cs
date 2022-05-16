using NewHorizons.Builder.Body;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.SizeControllers
{
    public class StarEvolutionController : SizeController
    {
        public GameObject atmosphere;
        public SupernovaEffectController supernova;

        public MColor startColour;
        public MColor endColour;

        private Color _startColour;
        private Color _endColour;

        private PlanetaryFogController _fog;
        private MeshRenderer[] _atmosphereRenderers;
        private HeatHazardVolume _heatVolume;
        private DestructionVolume _destructionVolume;

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

        void Awake()
        {
            var sun = GameObject.FindObjectOfType<SunController>();
            _collapseStartSurfaceMaterial = new Material(sun._collapseStartSurfaceMaterial);
            _collapseEndSurfaceMaterial = new Material(sun._collapseEndSurfaceMaterial);
            _startSurfaceMaterial = new Material(sun._startSurfaceMaterial);
            _endSurfaceMaterial = new Material(sun._endSurfaceMaterial);

            // Copy over the material that was set in star builder
            _collapseStartSurfaceMaterial.SetTexture("_ColorRamp", supernova._surface.sharedMaterial.GetTexture("_ColorRamp"));
            _collapseEndSurfaceMaterial.SetTexture("_ColorRamp", supernova._surface.sharedMaterial.GetTexture("_ColorRamp"));
            _startSurfaceMaterial.SetTexture("_ColorRamp", supernova._surface.sharedMaterial.GetTexture("_ColorRamp"));
            _endSurfaceMaterial.SetTexture("_ColorRamp", supernova._surface.sharedMaterial.GetTexture("_ColorRamp"));

            if (startColour == null)
            {
                _startColour = _startSurfaceMaterial.color;
            }
            else
            {
                _startColour = startColour.ToColor();
                _startSurfaceMaterial.color = _startColour;
            }

            if (endColour == null)
            {
                _endColour = _endSurfaceMaterial.color;
            }
            else
            {
                _endColour = endColour.ToColor();
                _endSurfaceMaterial.color = _endColour;
            }


            _heatVolume = GetComponentInChildren<HeatHazardVolume>();
            _destructionVolume = GetComponentInChildren<DestructionVolume>();

            if (atmosphere != null)
            {
                _fog = atmosphere.GetComponentInChildren<PlanetaryFogController>();
                _atmosphereRenderers = atmosphere.transform.Find("AtmoSphere").GetComponentsInChildren<MeshRenderer>();
            }

            GlobalMessenger.AddListener("TriggerSupernova", Die);
        }

        public void OnDestroy()
        {
            GlobalMessenger.RemoveListener("TriggerSupernova", Die);
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
                if(_destructionVolume != null) _destructionVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius() * 0.9f;
                if(_heatVolume != null) _heatVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius();

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

                currentColour = Color.Lerp(_startColour, _endColour, ageValue);

                supernova._surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, ageValue);
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
                    supernova.enabled = true;
                    _isSupernova = true;
                    _supernovaStartTime = Time.time;
                    atmosphere.SetActive(false);
                    _destructionVolume._deathType = DeathType.Supernova;
                    return;
                }
            }

            // This is just all the scales stuff for the atmosphere effects
            if (_fog != null)
            {
                _fog.fogRadius = CurrentScale * StarBuilder.OuterRadiusRatio;
                _fog.lodFadeDistance = CurrentScale * (StarBuilder.OuterRadiusRatio - 1f);

                _fog.fogTint = currentColour;
            }

            if (_atmosphereRenderers.Count() > 0)
            {
                foreach (var lod in _atmosphereRenderers)
                {
                    lod.material.SetFloat("_InnerRadius", CurrentScale);
                    lod.material.SetFloat("_OuterRadius", CurrentScale * StarBuilder.OuterRadiusRatio);
                    lod.material.SetColor("_SkyColor", currentColour);
                }
            }
        }
    }
}
