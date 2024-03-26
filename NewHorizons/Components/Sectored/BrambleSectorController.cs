using UnityEngine;

namespace NewHorizons.Components.Sectored
{
    public class BrambleSectorController : MonoBehaviour, ISectorGroup
    {
        private Sector _sector;

        private Renderer[] _renderers = null;
        private TessellatedRenderer[] _tessellatedRenderers = null;
        private Collider[] _colliders = null;
        private Light[] _lights = null;

        public static bool isPlayerInside = false;
        public static bool isProbeInside = false;
        public static bool isShipInside = false;

        private bool _renderersShown = false;

        public Sector GetSector() => _sector;

        public void SetSector(Sector sector)
        {
            if (_sector != null) _sector.OnSectorOccupantsUpdated -= OnSectorOccupantsUpdated;

            _sector = sector;
            _sector.OnSectorOccupantsUpdated += OnSectorOccupantsUpdated;
        }

        private void OnDestroy()
        {
            if (_sector != null) _sector.OnSectorOccupantsUpdated -= OnSectorOccupantsUpdated;
        }

        private void Start()
        {
            _renderers = gameObject.GetComponentsInChildren<Renderer>();
            _tessellatedRenderers = gameObject.GetComponentsInChildren<TessellatedRenderer>();
            _colliders = gameObject.GetComponentsInChildren<Collider>();
            _lights = gameObject.GetComponentsInChildren<Light>();

            DisableRenderers();
        }

        private void OnSectorOccupantsUpdated()
        {
            if (_sector.ContainsAnyOccupants(DynamicOccupant.Player | DynamicOccupant.Probe))
            {
                if (!_renderersShown) EnableRenderers();
            }
            else
            {
                if (_renderersShown) DisableRenderers();
            }
        }

        private void EnableRenderers() => ToggleRenderers(true);

        private void DisableRenderers() => ToggleRenderers(false);

        private void ToggleRenderers(bool visible)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer != null)
                {
                    renderer.forceRenderingOff = visible;
                }
            }

            foreach (var tessellatedRenderer in _tessellatedRenderers)
            {
                if (tessellatedRenderer != null)
                {
                    tessellatedRenderer.enabled = visible;
                }
            }

            foreach (var collider in _colliders)
            {
                if (collider != null)
                {
                    collider.enabled = visible;
                }
            }

            foreach (var light in _lights)
            {
                if (light != null)
                {
                    light.enabled = visible;
                }
            }

            _renderersShown = visible;
        }
    }
}
