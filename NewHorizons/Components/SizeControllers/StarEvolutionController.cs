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
        public StarController controller;
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
        public DestructionVolume _destructionVolume;
        public DestructionVolume _planetDestructionVolume;
        private SolarFlareEmitter _flareEmitter;
        private MapMarker _mapMarker;
        private OWRigidbody _rigidbody;

        private bool _isCollapsing;
        private float _collapseStartSize;
        private float _collapseTimer;

        public float collapseTime = 10f; // seconds
        public float supernovaScaleStart = 45f; // seconds
        public float supernovaScaleEnd = 50f; // seconds
        public float supernovaTime = 50f; // seconds
        public float lifespan = 22f; // minutes
        public float supernovaSize = 50000f;

        private bool _isSupernova;
        private float _supernovaStartTime;

        private Material _collapseStartSurfaceMaterial;
        private Material _collapseEndSurfaceMaterial;
        private Material _startSurfaceMaterial;
        private Material _endSurfaceMaterial;
        private Material _surfaceMaterial;
        private Texture _normalRamp;
        private Texture _collapseRamp;

        private StarEvolutionController _proxy;

        public UnityEvent SupernovaStart = new UnityEvent();

        private float maxScale;
        private float minScale;
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");
        private static readonly int ColorTime = Shader.PropertyToID("_ColorTime");
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");
        private static readonly int SkyColor = Shader.PropertyToID("_SkyColor");

        private Color _currentColour;

        private void Start()
        {
            _rigidbody = this.GetAttachedOWRigidbody();
            if (_rigidbody != null) _mapMarker = _rigidbody.GetComponent<MapMarker>();

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
                _endSurfaceMaterial.color = _endColour * 4.5948f;
            }

            _heatVolume = GetComponentInChildren<HeatHazardVolume>();
            if (_destructionVolume != null) _destructionVolume = GetComponentInChildren<DestructionVolume>();

            if (atmosphere != null)
            {
                _fog = atmosphere?.GetComponentInChildren<PlanetaryFogController>();
                _atmosphereRenderers = atmosphere?.transform?.Find("AtmoSphere")?.GetComponentsInChildren<MeshRenderer>();
            }

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
            _surfaceMaterial = supernova._surface._materials[0];

            var secondsElapsed = TimeLoop.GetSecondsElapsed();
            var lifespanInSeconds = lifespan * 60;
            if (secondsElapsed >= lifespanInSeconds)
            {
                var timeAfter = secondsElapsed - lifespanInSeconds;
                if (timeAfter <= collapseTime)
                    Delay.RunWhen(() => Main.IsSystemReady, StartCollapse);
                else if (timeAfter <= collapseTime + supernovaTime)
                    Delay.RunWhen(() => Main.IsSystemReady, StartSupernova);
                else
                    Delay.RunWhen(() => Main.IsSystemReady, () => Delay.FireOnNextUpdate(() => DisableStar(true)));
            }
        }

        public void OnDestroy()
        {
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
                // Use minutes elapsed if theres no resizing happening, else make it get redder the larger it is or wtv
                var t = TimeLoop.GetMinutesElapsed() / lifespan;
                if (maxScale != minScale) t = Mathf.InverseLerp(minScale, maxScale, CurrentScale);

                if (t < 1f)
                {
                    _currentColour = Color.Lerp(_startColour, _endColour, t);
                    supernova._surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, t);
                    supernova._surface._materials[0].SetFloat(ColorTime, t);
                }
                else
                {
                    _currentColour = _endColour;
                    supernova._surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, 1);
                    supernova._surface._materials[0].SetFloat(ColorTime, 1);
                }
            }
            else
            {
                _currentColour = _startColour;
                supernova._surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, 0);
                supernova._surface._materials[0].SetFloat(ColorTime, 0);
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
            if (_planetDestructionVolume != null) _planetDestructionVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius() * 0.9f;
            if (_heatVolume != null) _heatVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius();

            var t = Mathf.Clamp01((Time.time - (_supernovaStartTime + supernovaScaleStart)) / (supernovaScaleEnd - supernovaScaleStart));
            if (t > 0)
            {
                _planetDestructionVolume.GetComponent<SphereCollider>().radius = Mathf.Lerp(0.8f, 1, t);
            }

            if (Time.time > _supernovaStartTime + supernovaTime)
            {
                DisableStar();
            }
        }

        private void DisableStar(bool start = false)
        {
            if (_rigidbody != null)
            {
                ReferenceFrameTracker referenceFrameTracker = Locator.GetPlayerBody().GetComponent<ReferenceFrameTracker>();
                if (referenceFrameTracker.GetReferenceFrame() != null && referenceFrameTracker.GetReferenceFrame().GetOWRigidBody() == _rigidbody) referenceFrameTracker.UntargetReferenceFrame();
                _rigidbody._isTargetable = false;
                if (_rigidbody._attachedRFVolume != null)
                {
                    _rigidbody._attachedRFVolume._minColliderRadius = 0;
                    _rigidbody._attachedRFVolume._maxColliderRadius = 0;
                }
            }

            if (_mapMarker != null) _mapMarker.DisableMarker();

            if (controller != null) StarLightController.RemoveStar(controller);

            // Just turn off the star entirely
            base.gameObject.SetActive(false);

            if (start && _planetDestructionVolume != null)
            {
                foreach (var collider in Physics.OverlapSphere(_planetDestructionVolume.transform.position, _planetDestructionVolume.GetComponent<SphereCollider>().radius * supernovaSize * 0.9f))
                {
                    if (collider.attachedRigidbody != null)
                    {
                        var body = collider.attachedRigidbody.GetComponent<OWRigidbody>();
                        if (body != null && body != _rigidbody)
                        {
                            // Vanish anything that is not a player-related object
                            if (!(collider.attachedRigidbody.CompareTag("Player") || collider.attachedRigidbody.CompareTag("Ship") || collider.attachedRigidbody.CompareTag("ShipCockpit") || collider.attachedRigidbody.CompareTag("Probe")))
                            {
                                _planetDestructionVolume.Vanish(body, new RelativeLocationData(body, _rigidbody, _planetDestructionVolume.transform));
                            }
                        }
                    }
                }
            }
        }

        public void StartCollapse()
        {
            if (_isCollapsing) return;

            Logger.LogVerbose($"{gameObject.transform.root.name} started collapse");

            _isCollapsing = true;
            _collapseStartSize = CurrentScale;
            _collapseTimer = 0f;
            supernova._surface._materials[0].CopyPropertiesFromMaterial(_collapseStartSurfaceMaterial);

            if (_proxy != null) _proxy.StartCollapse();
        }

        public void StopCollapse()
        {
            if (!_isCollapsing) return;

            Logger.LogVerbose($"{gameObject.transform.root.name} stopped collapse");

            _isCollapsing = false;
            supernova._surface._materials[0].CopyPropertiesFromMaterial(_endSurfaceMaterial);

            if (_proxy != null) _proxy.StopCollapse();
        }

        public void StartSupernova()
        {
            if (_isSupernova) return;

            Logger.LogVerbose($"{gameObject.transform.root.name} started supernova");

            SupernovaStart.Invoke();
            supernova.enabled = true;
            _isSupernova = true;
            _supernovaStartTime = Time.time;
            if (atmosphere != null) atmosphere.SetActive(false);
            if (_destructionVolume != null) _destructionVolume._deathType = DeathType.Supernova;
            if (_planetDestructionVolume != null) _planetDestructionVolume._deathType = DeathType.Supernova;

            if (_proxy != null) _proxy.StartSupernova();
        }

        public void StopSupernova()
        {
            if (!_isSupernova) return;

            Logger.LogVerbose($"{gameObject.transform.root.name} stopped supernova");

            supernova.enabled = false;
            _isSupernova = false;
            if (atmosphere != null) atmosphere.SetActive(true);
            if (_destructionVolume != null)
            {
                _destructionVolume._deathType = DeathType.Energy;
                _destructionVolume.transform.localScale = Vector3.one;
            }
            if (_planetDestructionVolume != null)
            {
                _planetDestructionVolume._deathType = DeathType.Energy;
                _planetDestructionVolume.transform.localScale = Vector3.one;
            }
            if (_heatVolume != null) _heatVolume.transform.localScale = Vector3.one;
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            supernova._surface._materials[0] = _surfaceMaterial;
            supernova._surface.transform.localScale = Vector3.one;

            if (_proxy != null) _proxy.StopSupernova();
        }

        protected new void FixedUpdate()
        {
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
                if (WillExplode && (TimeLoop.GetMinutesElapsed() / lifespan) >= 1) StartCollapse();
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

            if (_atmosphereRenderers != null)
            {
                foreach (var lod in _atmosphereRenderers)
                {
                    lod.material.SetFloat(InnerRadius, CurrentScale);
                    lod.material.SetFloat(OuterRadius, CurrentScale * StarBuilder.OuterRadiusRatio);
                    lod.material.SetColor(SkyColor, _currentColour);
                }
            }
        }
    }
}
