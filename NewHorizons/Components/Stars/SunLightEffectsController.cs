using NewHorizons.Builder.Atmosphere;
using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Components.Stars
{
    [RequireComponent(typeof(SunLightController))]
    [RequireComponent(typeof(SunLightParamUpdater))]
    public class SunLightEffectsController : MonoBehaviour
    {
        private static readonly int SunIntensity = Shader.PropertyToID("_SunIntensity");
        private static readonly float hearthSunDistanceSqr = 8593 * 8593;

        public static SunLightEffectsController Instance { get; private set; }

        private readonly List<StarController> _stars = new();
        private readonly List<Light> _lights = new();

        // SunController or StarEvolutionController
        public StarEvolutionController ActiveStarEvolutionController { get; private set; }
        public SunController ActiveSunController { get; private set; }

        private StarController _activeStarController;
        private SunLightController _sunLightController;
        private SunLightParamUpdater _sunLightParamUpdater;

        public void Awake()
        {
            Instance = this;

            _sunLightController = GetComponent<SunLightController>();
            _sunLightController.enabled = true;

            _sunLightParamUpdater = GetComponent<SunLightParamUpdater>();
            _sunLightParamUpdater._sunLightController = _sunLightController;
        }

        public void Start()
        {
            // Using GameObject.Find here so that if its null we just dont find it
            var sunlight = GameObject.Find("Sun_Body/Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>();
            if (sunlight != null) AddStarLight(sunlight);
        }

        public static void AddStar(StarController star)
        {
            if (star == null) return;

            NHLogger.LogVerbose($"Adding new star to list: {star.gameObject.name}");
            Instance._stars.Add(star);
        }

        public static void RemoveStar(StarController star)
        {
            if (Instance == null) return;

            NHLogger.LogVerbose($"Removing star from list: {star?.gameObject?.name}");
            if (Instance._stars.Contains(star))
            {
                if (Instance._activeStarController != null && Instance._activeStarController.Equals(star))
                {
                    Instance._stars.Remove(star);
                    if (Instance._stars.Count > 0) Instance.ChangeActiveStar(Instance._stars[0]);
                }
                else
                {
                    Instance._stars.Remove(star);
                }
            }
        }

        public static void AddStarLight(Light light)
        {
            if (light != null)
            {
                Instance._lights.SafeAdd(light);
            }
        }

        public static void RemoveStarLight(Light light)
        {
            if (Instance != null && light != null && Instance._lights.Contains(light))
            {
                Instance._lights.Remove(light);
            }
        }

        public void Update()
        {
            // Player is always at 0,0,0 more or less so if they arent using the map camera then wtv
            var origin = Vector3.zero;

            if (PlayerState.InMapView())
            {
                origin = Locator.GetActiveCamera().transform.position;

                foreach (var light in _lights)
                {
                    // Keep all star lights on in map
                    light.enabled = true;
                    // Fixes everything transparent breaking due to directional
                    if (light.type == LightType.Directional) light.type = LightType.Point;
                }
            }
            else
            {
                // Outside map, only show lights within 50km range or light.range
                // For some reason outside of the actual range of the lights they still show reflection effects on water and glass
                foreach (var light in _lights)
                {
                    // Minimum 50km range so it's not badly noticeable for dim stars
                    if ((light.transform.position - origin).sqrMagnitude <= Mathf.Max(light.range * light.range, 2500000000))
                    {
                        light.enabled = true;
                    }
                    else
                    {
                        light.enabled = false;
                    }
                    // Revert map fix
                    if (light.type == LightType.Point) light.type = LightType.Directional;
                }
            }

            if (_stars.Count > 0)
            {
                if (_activeStarController == null || !_activeStarController.gameObject.activeInHierarchy)
                {
                    if (_stars.Contains(_activeStarController))
                    {
                        _stars.Remove(_activeStarController);
                    }

                    if (_stars.Count > 0)
                    {
                        ChangeActiveStar(_stars[0]);
                    }
                    else
                    {
                        foreach (var (_, material) in AtmosphereBuilder.Skys)
                        {
                            material.SetFloat(SunIntensity, 0);
                        }
                    }
                }
                else
                {
                    // Update atmo shaders
                    foreach (var (planet, material) in AtmosphereBuilder.Skys)
                    {
                        var sqrDist = (planet.transform.position - _activeStarController.transform.position).sqrMagnitude;
                        var intensity = Mathf.Min(_activeStarController.Intensity / (sqrDist / hearthSunDistanceSqr), 1f);

                        material.SetFloat(SunIntensity, intensity);
                    }

                    foreach (var star in _stars)
                    {
                        if (star == null) continue;
                        if (!(star.gameObject.activeSelf && star.gameObject.activeInHierarchy)) continue;

                        if (star.Intensity * (star.transform.position - origin).sqrMagnitude < _activeStarController.Intensity * (_activeStarController.transform.position - origin).sqrMagnitude)
                        {
                            ChangeActiveStar(star);
                            break;
                        }
                    }
                }
            }
        }

        private void ChangeActiveStar(StarController star)
        {
            if (_sunLightController == null || _sunLightParamUpdater == null) return;

            _activeStarController?.Disable();

            NHLogger.LogVerbose($"Switching active star: {star.gameObject.name}");

            _activeStarController = star;
            ActiveStarEvolutionController = star.GetComponentInChildren<StarEvolutionController>();
            ActiveSunController = star.GetComponent<SunController>();

            star.Enable();

            _sunLightController._sunBaseColor = star.SunColor;
            _sunLightController._sunBaseIntensity = star.Intensity;
            _sunLightController._sunLight = star.Light;
            _sunLightController._ambientLight = star.AmbientLight;

            _sunLightParamUpdater.sunLight = star.Light;
            _sunLightParamUpdater._sunController = star.transform.GetComponent<SunController>();
            _sunLightParamUpdater._propID_SunPosition = Shader.PropertyToID("_SunPosition");
            _sunLightParamUpdater._propID_OWSunPositionRange = Shader.PropertyToID("_OWSunPositionRange");
            _sunLightParamUpdater._propID_OWSunColorIntensity = Shader.PropertyToID("_OWSunColorIntensity");

            // For the param thing to work it wants this to be on the star idk
            transform.parent = star.transform;
            transform.localPosition = Vector3.zero;

            // Some effects use Locator.GetSunTransform so hopefully its fine to change it
            // Use the root transform of the star because that's the default behaviour, breaks SunProxy if not (potentially others)
            Locator._sunTransform = star.transform;
            
            // TODO?: maybe also turn off star controller stuff (mainly proxy light) since idk if that can handle more than 1 being on
        }
    }
}
