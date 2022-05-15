using NewHorizons.Builder.Body;
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

        private PlanetaryFogController _fog;
        private MeshRenderer[] _atmosphereRenderers;

        private bool _isCollapsing;
        private float _collapseStartSize;
        private float _collapseTimer;

        public float collapseTime = 5f;

        private bool _isSupernova;
        private float _supernovaStartTime;

        private Material _collapseStartSurfaceMaterial;
        private Material _collapseEndSurfaceMaterial;

        void Awake()
        {
            var sun = GameObject.FindObjectOfType<SunController>();
            _collapseStartSurfaceMaterial = sun._collapseStartSurfaceMaterial;
            _collapseEndSurfaceMaterial = sun._collapseEndSurfaceMaterial;

            if (atmosphere != null)
            {
                _fog = atmosphere.GetComponentInChildren<PlanetaryFogController>();
                _atmosphereRenderers = atmosphere.transform.Find("AtmoSphere").GetComponentsInChildren<MeshRenderer>();
            }
        }

        public void Die()
        {
            _isCollapsing = true;
            _collapseStartSize = CurrentScale;
            _collapseTimer = 0f;
        }

        protected new void FixedUpdate()
        {
            // If we've gone supernova and its been 60 seconds that means it has faded out and is gone
            if (_isSupernova)
            {
                transform.localScale = Vector3.one;
                if (Time.time > _supernovaStartTime + 60f)
                {
                    base.gameObject.SetActive(false);
                }
                return;
            } 

            if (!_isCollapsing)
            {
                base.FixedUpdate();
            }
            else
            {
                // When its collapsing we directly take over the scale
                var t = _collapseTimer / collapseTime;
                CurrentScale = Mathf.Lerp(_collapseStartSize, 0, t);
                transform.localScale = Vector3.one * CurrentScale;
                _collapseTimer += Time.deltaTime;

                supernova._surface._materials[0].Lerp(this._collapseStartSurfaceMaterial, this._collapseEndSurfaceMaterial, t);

                // After the collapse is done we go supernova
                if (_collapseTimer > collapseTime)
                {
                    supernova.enabled = true;
                    _isSupernova = true;
                    _supernovaStartTime = Time.time;
                    atmosphere.SetActive(false);
                    return;
                }
            }

            // This is just all the scales stuff for the atmosphere effects
            if (atmosphere != null)
            {
                atmosphere.transform.localScale = CurrentScale * Vector3.one;
            }

            if (_fog != null)
            {
                _fog.fogRadius = CurrentScale * StarBuilder.OuterRadiusRatio;
                _fog.lodFadeDistance = CurrentScale * (StarBuilder.OuterRadiusRatio - 1f);
            }

            if (_atmosphereRenderers.Count() > 0)
            {
                foreach (var lod in _atmosphereRenderers)
                {
                    lod.material.SetFloat("_InnerRadius", CurrentScale);
                    lod.material.SetFloat("_OuterRadius", CurrentScale * StarBuilder.OuterRadiusRatio);
                }
            }
        }
    }
}
