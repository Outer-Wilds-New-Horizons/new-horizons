using NewHorizons.External.Configs;
using NewHorizons.External.SerializableData;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Components;

/// <summary>
/// When a Fluid Detector enters this volume, it's splash effects will get colourized to match whats on this planet
/// </summary>
public class SplashColourizer : MonoBehaviour
{
    public float _radius;

    private SphereShape _sphereShape;

    private Dictionary<SplashEffect, GameObject> _cachedOriginalPrefabs = new();
    private Dictionary<SplashEffect, GameObject> _cachedModifiedPrefabs = new();

    private FluidDetector _playerDetector, _shipDetector, _probeDetector;

    private MColor _waterColour, _cloudColour, _plasmaColour, _sandColour;

    private GameObject _prefabHolder;

    private bool _probeInsideVolume;

    private List<Texture> _customTextures = new();

    public void Awake()
    {
        var volume = new GameObject("Volume");
        volume.transform.parent = transform;
        volume.transform.localPosition = Vector3.zero;
        volume.layer = Layer.BasicEffectVolume;

        _sphereShape = gameObject.AddComponent<SphereShape>();
        _sphereShape.radius = _radius;

        volume.AddComponent<OWTriggerVolume>();

        _prefabHolder = new GameObject("Prefabs");
        _prefabHolder.SetActive(false);
    }

    public static void Make(GameObject planet, PlanetConfig config, float soi)
    {
        var water = config.Water?.tint;
        var cloud = config.Atmosphere?.clouds?.tint;
        var plasma = config.Lava?.tint ?? config.Star?.tint;
        var sand = config.Sand?.tint;

        if (water != null || cloud != null || plasma != null || sand != null)
        {
            var size = Mathf.Max(
                soi / 1.5f,
                config.Water?.size ?? 0f,
                config.Atmosphere?.clouds?.outerCloudRadius ?? 0f,
                config.Lava?.size ?? 0f,
                config.Star?.size ?? 0f,
                config.Sand?.size ?? 0f
                ) * 1.5f;

            var colourizer = planet.AddComponent<SplashColourizer>();

            colourizer._radius = size;
            if (colourizer._sphereShape != null) colourizer._sphereShape.radius = size;

            colourizer._waterColour = water;
            colourizer._cloudColour = cloud;
            colourizer._plasmaColour = plasma;
            colourizer._sandColour = sand;
        }
    }

    public void Start()
    {
        // Cache all prefabs
        CachePrefabs(_playerDetector = Locator.GetPlayerDetector().GetComponent<DynamicFluidDetector>());
        CachePrefabs(_shipDetector = Locator.GetShipDetector().GetComponent<ShipFluidDetector>());
        CachePrefabs(_probeDetector = Locator.GetProbe().GetDetectorObject().GetComponent<ProbeFluidDetector>());

        GlobalMessenger<SurveyorProbe>.AddListener("RetrieveProbe", OnRetrieveProbe);

        // Check if player/ship are already inside
        if ((_playerDetector.transform.position - transform.position).magnitude < _radius)
        {
            SetSplashEffects(_playerDetector, true);
        }
        if ((_shipDetector.transform.position - transform.position).magnitude < _radius)
        {
            SetSplashEffects(_shipDetector, true);
        }
    }

    public void OnDestroy()
    {
        GlobalMessenger<SurveyorProbe>.RemoveListener("RetrieveProbe", OnRetrieveProbe);
    }

    private void OnRetrieveProbe(SurveyorProbe probe)
    {
        if (_probeInsideVolume)
        {
            // Else it never leaves the volume
            SetProbeSplashEffects(false);
        }
    }

    public void OnTriggerEnter(Collider hitCollider) => OnEnterExit(hitCollider, true);

    public void OnTriggerExit(Collider hitCollider) => OnEnterExit(hitCollider, false);

    /// <summary>
    /// The probe keeps being null idgi
    /// </summary>
    /// <returns></returns>
    private bool IsProbeLaunched()
    {
        return Locator.GetProbe()?.IsLaunched() ?? false;
    }

    private void OnEnterExit(Collider hitCollider, bool entering)
    {
        if (!enabled) return;

        if (hitCollider.attachedRigidbody != null)
        {
            var isPlayer = hitCollider.attachedRigidbody.CompareTag("Player");
            var isShip = hitCollider.attachedRigidbody.CompareTag("Ship");
            var isProbe = hitCollider.attachedRigidbody.CompareTag("Probe");

            if (isPlayer)
            {
                SetSplashEffects(_playerDetector, entering);
                if (IsProbeLaunched())
                {
                    SetProbeSplashEffects(entering);
                }
            }
            else if (isShip)
            {
                SetSplashEffects(_shipDetector, entering);
                if (PlayerState.IsInsideShip())
                {
                    SetSplashEffects(_playerDetector, entering);
                }
                if (IsProbeLaunched())
                {
                    SetProbeSplashEffects(entering);
                }
            }
            else if (isProbe)
            {
                SetProbeSplashEffects(entering);
            }

            // If the probe isn't launched we always consider it as being inside the volume
            if (isProbe || !IsProbeLaunched())
            {
                _probeInsideVolume = entering;
            }
        }
    }

    public void CachePrefabs(FluidDetector detector)
    {
        foreach (var splashEffect in detector._splashEffects)
        {
            if (!_cachedOriginalPrefabs.ContainsKey(splashEffect))
            {
                _cachedOriginalPrefabs[splashEffect] = splashEffect.splashPrefab;
            }
            if (!_cachedModifiedPrefabs.ContainsKey(splashEffect))
            {
                Color? colour = null;
                if (splashEffect.fluidType == FluidVolume.Type.CLOUD)
                {
                    colour = _cloudColour?.ToColor();
                }
                switch(splashEffect.fluidType)
                {
                    case FluidVolume.Type.CLOUD:
                        colour = _cloudColour?.ToColor();
                        break;
                    case FluidVolume.Type.WATER:
                        colour = _waterColour?.ToColor();
                        break;
                    case FluidVolume.Type.PLASMA:
                        colour = _plasmaColour?.ToColor();
                        break;
                    case FluidVolume.Type.SAND:
                        colour = _sandColour?.ToColor();
                        break;
                }

                if (colour is Color tint)
                {
                    var flagError = false;
                    var prefab = splashEffect.splashPrefab.InstantiateInactive();
                    var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var meshRenderer in meshRenderers)
                    {
                        if (_customTextures.Contains(meshRenderer.material.mainTexture))
                        {
                            // Might be some shared material stuff? This image is already tinted, so skip it
                            continue;
                        }

                        // Can't access the textures in memory so we need to have our own copies
                        var texture = ImageUtilities.GetTexture(Main.Instance, $"Assets/textures/{meshRenderer.material.mainTexture.name}.png");
                        if (texture == null)
                        {
                            NHLogger.LogError($"Go tell an NH dev to add this image texture to the mod because I can't be bothered to until somebody complains: {meshRenderer.material.mainTexture.name}");
                            GameObject.Destroy(prefab);
                            flagError = true;
                        }

                        _customTextures.Add(texture);

                        meshRenderer.material = new(meshRenderer.material)
                        {
                            color = Color.white,
                            mainTexture = ImageUtilities.TintImage(texture, tint)
                        };
                    }

                    if (flagError) continue;

                    // Have to be active when being used by the base game classes but can't be seen
                    // Keep them active as children of an inactive game object
                    prefab.transform.parent = _prefabHolder.transform;
                    prefab.SetActive(true);

                    _cachedModifiedPrefabs[splashEffect] = prefab;
                }
            }
        }
    }

    public void SetSplashEffects(FluidDetector detector, bool entering)
    {
        NHLogger.LogVerbose($"Body {detector.name} {(entering ? "entered" : "left")} colourizing volume on {name}");

        foreach (var splashEffect in detector._splashEffects)
        {
            var prefabs = entering ? _cachedModifiedPrefabs : _cachedOriginalPrefabs;
            if (prefabs.TryGetValue(splashEffect, out var prefab))
            {
                splashEffect.splashPrefab = prefab;
            }
        }
    }

    public void SetProbeSplashEffects(bool entering)
    {
        _probeInsideVolume = entering;

        SetSplashEffects(_probeDetector, entering);
    }
}
