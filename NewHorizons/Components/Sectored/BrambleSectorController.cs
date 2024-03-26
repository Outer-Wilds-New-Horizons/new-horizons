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
            DisableRenderers();
        }

        private void GetRenderers()
        {
            _renderers = gameObject.GetComponentsInChildren<Renderer>();
            _tessellatedRenderers = gameObject.GetComponentsInChildren<TessellatedRenderer>();
            _colliders = gameObject.GetComponentsInChildren<Collider>();
            _lights = gameObject.GetComponentsInChildren<Light>();
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
            GetRenderers();

            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = !visible;
            }

            foreach (var tessellatedRenderer in _tessellatedRenderers)
            {
                tessellatedRenderer.enabled = visible;
            }

            foreach (var collider in _colliders)
            {
                collider.enabled = visible;
            }

            foreach (var light in _lights)
            {
                light.enabled = visible;
            }

            _renderersShown = visible;
        }
    }
}
