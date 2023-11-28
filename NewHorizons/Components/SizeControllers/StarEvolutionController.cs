using NewHorizons.Builder.Body;
using NewHorizons.Components.Orbital;
using NewHorizons.Components.Stars;
using NewHorizons.External.SerializableData;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace NewHorizons.Components.SizeControllers
{
    public class StarEvolutionController : SizeController
    {
        public bool isProxy;
        public bool isRemnant;

        public GameObject atmosphere;
        public StarController controller;
        public StellarDeathController supernova;
        public bool willExplode;
        public MColor startColour;
        public MColor endColour;
        public MColor supernovaColour;
        public Texture normalRamp;
        public Texture collapseRamp;
        public Light light;

        private Color _startColour;
        private Color _endColour;

        private GameObject _stellarRemnant;
        private PlanetaryFogController _fog;
        private MeshRenderer[] _atmosphereRenderers;

        public HeatHazardVolume heatVolume;
        public DestructionVolume destructionVolume;
        public StarDestructionVolume planetDestructionVolume;
        public StarFluidVolume starFluidVolume;

        private SolarFlareEmitter _flareEmitter;
        private OWRigidbody _rigidbody;

        public OWAudioSource oneShotSource;

        private bool _isCollapsing;
        private float _collapseStartSize;
        private float _collapseTimer;

        public float collapseTime = 10f; // seconds
        public float supernovaScaleStart = 45f; // seconds
        public float supernovaScaleEnd = 50f; // seconds
        public float supernovaTime = 50f; // seconds
        public float lifespan = 22f; // minutes
        public float supernovaSize = 50000f;

        public StellarDeathType deathType;

        private bool _isSupernova;
        private float _supernovaStartTime;

        private static Material _defaultCollapseStartSurfaceMaterial,
            _defaultCollapseEndSurfaceMaterial,
            _defaultStartSurfaceMaterial, 
            _defaultEndSurfaceMaterial;

        private static Texture _defaultNormalRamp, _defaultCollapseRamp;

        private Material _collapseStartSurfaceMaterial,
            _collapseEndSurfaceMaterial,
            _startSurfaceMaterial,
            _endSurfaceMaterial;

        private Material _surfaceMaterial;
        private Texture _normalRamp;
        private Texture _collapseRamp;

        private StarEvolutionController _proxy;

        public UnityEvent CollapseStart = new();
        public UnityEvent CollapseStop = new();
        public UnityEvent SupernovaStart = new();
        public UnityEvent SupernovaStop = new();

        private float _maxScale;
        private float _minScale;
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");
        private static readonly int ColorTime = Shader.PropertyToID("_ColorTime");
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");
        private static readonly int SkyColor = Shader.PropertyToID("_SkyColor");

        private Color _currentColour;

        private TessellatedSphereRenderer _surface;

        public static void Init()
        {
            var sun = SearchUtilities.Find("Sun_Body").GetComponent<SunController>();

            if (sun == null) return;

            // Need to grab all this early bc the star might only Start after the solar system was made (remnants)
            if (_defaultCollapseStartSurfaceMaterial == null) _defaultCollapseStartSurfaceMaterial = new Material(sun._collapseStartSurfaceMaterial).DontDestroyOnLoad();
            if (_defaultCollapseEndSurfaceMaterial == null) _defaultCollapseEndSurfaceMaterial = new Material(sun._collapseEndSurfaceMaterial).DontDestroyOnLoad();
            if (_defaultStartSurfaceMaterial == null) _defaultStartSurfaceMaterial = new Material(sun._startSurfaceMaterial).DontDestroyOnLoad();
            if (_defaultEndSurfaceMaterial == null) _defaultEndSurfaceMaterial = new Material(sun._endSurfaceMaterial).DontDestroyOnLoad();

            if (_defaultNormalRamp == null) _defaultNormalRamp = sun._startSurfaceMaterial.GetTexture(ColorRamp).DontDestroyOnLoad();
            if (_defaultCollapseRamp == null) _defaultCollapseRamp = sun._collapseStartSurfaceMaterial.GetTexture(ColorRamp).DontDestroyOnLoad();
        }

        private void Start()
        {
            _surface = GetComponentInChildren<TessellatedSphereRenderer>(true);
            _surfaceMaterial = _surface._materials[0];

            _collapseStartSurfaceMaterial = new Material(_defaultCollapseStartSurfaceMaterial);
            _collapseEndSurfaceMaterial = new Material(_defaultCollapseEndSurfaceMaterial);
            _startSurfaceMaterial = new Material(_defaultStartSurfaceMaterial);
            _endSurfaceMaterial = new Material(_defaultEndSurfaceMaterial);

            _rigidbody = this.GetAttachedOWRigidbody();

            if (normalRamp == null)
            {
                _normalRamp = _defaultNormalRamp;
            }
            else
            {
                _normalRamp = normalRamp;
            }
            if (collapseRamp == null)
            {
                _collapseRamp = _defaultCollapseRamp;
            }
            else
            {
                _collapseRamp = collapseRamp;
            }

            // Copy over the material that was set in star builder
            _collapseStartSurfaceMaterial.SetTexture(ColorRamp, _collapseRamp);
            _collapseEndSurfaceMaterial.SetTexture(ColorRamp, _collapseRamp);
            _startSurfaceMaterial.SetTexture(ColorRamp, _normalRamp);
            _endSurfaceMaterial.SetTexture(ColorRamp, _normalRamp);

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
                _endColour = _startColour;
                _endSurfaceMaterial.color = _startColour;
            }
            else
            {
                _endColour = endColour.ToColor();
                _endSurfaceMaterial.color = _endColour * 4.5948f;
            }

            if (heatVolume == null) heatVolume = GetComponentInChildren<HeatHazardVolume>();
            if (destructionVolume == null) destructionVolume = GetComponentInChildren<DestructionVolume>();
            if (planetDestructionVolume == null) planetDestructionVolume = GetComponentInChildren<StarDestructionVolume>();
            if (starFluidVolume == null) starFluidVolume = GetComponentInChildren<StarFluidVolume>();

            if (atmosphere != null)
            {
                _fog = atmosphere?.GetComponentInChildren<PlanetaryFogController>();
                _atmosphereRenderers = atmosphere?.transform?.Find("AtmoSphere")?.GetComponentsInChildren<MeshRenderer>();
            }

            if (scaleCurve != null)
            {
                _maxScale = scaleCurve.keys.Select(x => x.value).Max() * size;
                _minScale = scaleCurve.keys.Select(x => x.value).Min() * size;
            }
            else
            {
                _maxScale = 0;
                _minScale = 0;
                scaleCurve = new AnimationCurve();
                scaleCurve.AddKey(0, 1);
            }

            _flareEmitter = GetComponentInChildren<SolarFlareEmitter>();

            if (!isProxy) SupernovaEffectHandler.RegisterStar(this);

            var secondsElapsed = TimeLoop.GetSecondsElapsed();
            var lifespanInSeconds = lifespan * 60;
            if (!isProxy && willExplode && secondsElapsed >= lifespanInSeconds)
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
            SupernovaEffectHandler.UnregisterStar(this);
        }

        public void SetProxy(StarEvolutionController proxy)
        {
            _proxy = proxy;
            _proxy.supernova?.SetIsProxy(true);
        }

        private void UpdateMainSequence()
        {
            // Only do colour transition stuff if they set an end colour
            if (endColour != null)
            {
                // Use minutes elapsed if theres no resizing happening, else make it get redder the larger it is or wtv
                var t = TimeLoop.GetMinutesElapsed() / lifespan;
                if (_maxScale != _minScale) t = Mathf.InverseLerp(_minScale, _maxScale, CurrentScale);

                if (t < 1f)
                {
                    _currentColour = Color.Lerp(_startColour, _endColour, t);
                    _surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, t);
                    _surface._materials[0].SetFloat(ColorTime, t);
                }
                else
                {
                    _currentColour = _endColour;
                    _surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, 1);
                    _surface._materials[0].SetFloat(ColorTime, 1);
                }
            }
            else
            {
                _currentColour = _startColour;
                _surface._materials[0].Lerp(_startSurfaceMaterial, _endSurfaceMaterial, 0);
                _surface._materials[0].SetFloat(ColorTime, 0);
            }

            if (_flareEmitter != null)
            {
                _flareEmitter.tint = _currentColour;
            }
        }

        private void UpdateCollapse()
        {
            // When its collapsing we directly take over the scale
            var t = _collapseTimer / collapseTime;
            CurrentScale = Mathf.Lerp(_collapseStartSize, 0, t);
            transform.localScale = Vector3.one * CurrentScale;
            _collapseTimer += Time.deltaTime;

            _currentColour = Color.Lerp(_endColour, Color.white, t);

            _surface._materials[0].Lerp(_collapseStartSurfaceMaterial, _collapseEndSurfaceMaterial, t);

            // After the collapse is done we go supernova
            // Main star will call this on the proxy
            if (!isProxy && _collapseTimer > collapseTime) StartSupernova();
        }

        private void UpdateSupernova()
        {
            // Reset the scale back to normal bc now its just the supernova scaling itself + destruction and heat volumes
            transform.localScale = Vector3.one;

            var t = Mathf.Clamp01((Time.time - (_supernovaStartTime + supernovaScaleStart)) / (supernovaScaleEnd - supernovaScaleStart));
            // Make the destruction volume scale slightly smaller so you really have to be in the supernova to die
            if (destructionVolume != null) destructionVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius() * Mathf.Lerp(0.9f, 1, t);
            if (heatVolume != null) heatVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius();
            if (planetDestructionVolume != null)
            {
                planetDestructionVolume.transform.localScale = Vector3.one * supernova.GetSupernovaRadius() * Mathf.Lerp(0.9f, 1, t);
                planetDestructionVolume.GetComponent<SphereCollider>().radius = Mathf.Lerp(0.8f, 1, t);
            }

            if (_stellarRemnant != null && Time.time > _supernovaStartTime + 15)
            {
                _stellarRemnant.SetActive(true);
                var remnantStarController = _stellarRemnant.GetComponentInChildren<StarController>();
                if (remnantStarController != null) SunLightEffectsController.AddStar(remnantStarController);
                var remnantStarLight = _stellarRemnant.FindChild("StarLight");
                if (remnantStarLight != null) SunLightEffectsController.AddStarLight(remnantStarLight.GetComponent<Light>());
            }

            if (Time.time > _supernovaStartTime + supernovaTime)
            {
                if (destructionVolume != null && destructionVolume._shrinkingBodies.Count > 0) return;
                if (planetDestructionVolume != null && planetDestructionVolume._shrinkingBodies.Count > 0) return;
                DisableStar();
            }
        }

        private void DisableStar(bool start = false)
        {
            if (controller != null) SunLightEffectsController.RemoveStar(controller);
            if (light != null) SunLightEffectsController.RemoveStarLight(light);

            if (_stellarRemnant != null)
            {
                transform.parent.gameObject.SetActive(false); // Turn off sector

                // If the star is disabled but has a remnant, be sure it is active
                _stellarRemnant.gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }

            if (start && planetDestructionVolume != null)
            {
                foreach (var collider in Physics.OverlapSphere(planetDestructionVolume.transform.position, planetDestructionVolume.GetComponent<SphereCollider>().radius * supernovaSize * 0.9f))
                {
                    if (collider.attachedRigidbody != null)
                    {
                        // Destroy any planets that are not invulnerable to the sun
                        var rb = collider.attachedRigidbody;
                        var body = rb.GetComponent<OWRigidbody>();
                        var astroObject = collider.gameObject.GetComponent<NHAstroObject>();
                        if (astroObject != null)
                        {
                            if (!astroObject.invulnerableToSun)
                                planetDestructionVolume.Vanish(body, new RelativeLocationData(body, _rigidbody, planetDestructionVolume.transform));
                        }
                        else
                        {
                            // Vanish anything unrelated to player
                            if (!(rb.CompareTag("Player") || rb.CompareTag("Ship") || rb.CompareTag("ShipCockpit") || rb.CompareTag("Probe")))
                                planetDestructionVolume.Vanish(body, new RelativeLocationData(body, _rigidbody, planetDestructionVolume.transform));
                        }
                    }
                }
            }

            if (_proxy != null) _proxy.DisableStar(start);
        }

        public void StartCollapse()
        {
            if (_isCollapsing) return;

            NHLogger.LogVerbose($"{gameObject.transform.root.name} started collapse");

            CollapseStart.Invoke();
            _isCollapsing = true;
            _collapseStartSize = CurrentScale;
            _collapseTimer = 0f;
            _surface._materials[0].CopyPropertiesFromMaterial(_collapseStartSurfaceMaterial);
            if (oneShotSource != null && !PlayerState.IsSleepingAtCampfire() && !PlayerState.InDreamWorld()) oneShotSource.PlayOneShot(AudioType.Sun_Collapse);

            _proxy?.StartCollapse();
        }

        public void StopCollapse()
        {
            if (!_isCollapsing) return;

            NHLogger.LogVerbose($"{gameObject.transform.root.name} stopped collapse");

            CollapseStop.Invoke();
            _isCollapsing = false;
            _surface._materials[0].CopyPropertiesFromMaterial(_endSurfaceMaterial);

            _proxy?.StopCollapse();
        }

        public void StartSupernova()
        {
            if (supernova == null) return;
            if (_isSupernova) return;

            NHLogger.LogVerbose($"{gameObject.transform.root.name} started supernova");

            SupernovaStart.Invoke();
            supernova.Activate();
            _isSupernova = true;
            _supernovaStartTime = Time.time;
            atmosphere?.SetActive(false);

            if (destructionVolume != null)
            {
                destructionVolume._deathType = DeathType.Supernova;
            }

            if (planetDestructionVolume != null)
            {
                planetDestructionVolume._deathType = DeathType.Supernova;
            }

            if (oneShotSource != null && !PlayerState.IsSleepingAtCampfire() && !PlayerState.InDreamWorld())
            {
                oneShotSource.PlayOneShot(AudioType.Sun_Explosion);
            }

            _proxy?.StartSupernova();
        }

        public void StopSupernova()
        {
            if (!_isSupernova) return;

            NHLogger.LogVerbose($"{gameObject.transform.root.name} stopped supernova");

            SupernovaStop.Invoke();
            supernova?.Deactivate();
            _isSupernova = false;
            atmosphere?.SetActive(true);
            
            if (destructionVolume != null)
            {
                destructionVolume._deathType = DeathType.Energy;
                destructionVolume.transform.localScale = Vector3.one;
            }
            
            if (planetDestructionVolume != null)
            {
                planetDestructionVolume._deathType = DeathType.Energy;
                planetDestructionVolume.transform.localScale = Vector3.one;
            }

            if (heatVolume != null)
            {
                heatVolume.transform.localScale = Vector3.one;
            }
            
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
            _surface._materials[0] = _surfaceMaterial;
            _surface.transform.localScale = Vector3.one;

            _proxy?.StopSupernova();
        }

        public bool IsCollapsing() => _isCollapsing;

        public float GetCollapseProgress() => _collapseTimer / collapseTime;

        public bool HasSupernovaStarted() => _isSupernova;

        public bool IsPointInsideSupernova(Vector3 worldPosition) => _isSupernova && (worldPosition - transform.position).sqrMagnitude < (supernova.GetSupernovaRadius() * supernova.GetSupernovaRadius());

        public bool IsPointInsideMaxSupernova(Vector3 worldPosition) => (worldPosition - transform.position).sqrMagnitude < (supernovaSize * supernovaSize);

        public float GetDistanceToSupernova(Vector3 worldPosition) => Vector3.Distance(worldPosition, transform.position) - supernova.GetSupernovaRadius();

        public float GetDistanceToMaxSupernova(Vector3 worldPosition) => Vector3.Distance(worldPosition, transform.position) - supernovaSize;

        public float GetSupernovaRadius() => supernova.GetSupernovaRadius();

        public float GetSurfaceRadius() => transform.localScale.x;

        public float GetMaxSupernovaRadius() => supernovaSize;

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
                // Proxy will have its collapse triggered by the main star component
                if (!isProxy && willExplode && (TimeLoop.GetMinutesElapsed() / lifespan) >= 1)
                {
                    StartCollapse();
                }
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

        public void SetStellarRemnant(GameObject stellarRemnant)
        {
            _stellarRemnant = stellarRemnant;
            _stellarRemnant.SetActive(false);
        }
    }
}
