using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace NewHorizons.Components
{
    public class CloakSectorController : MonoBehaviour
    {
        private CloakFieldController _cloak;
        private GameObject _root;

        private bool _isInitialized;

        private List<Renderer> _renderers = null;

        public void Init(CloakFieldController cloak, GameObject root)
        {
            _cloak = cloak;
            _root = root;

            // Lets just clear these off idc
            _cloak.OnPlayerEnter = new OWEvent();
            _cloak.OnPlayerExit = new OWEvent();

            _cloak.OnPlayerEnter += OnPlayerEnter;
            _cloak.OnPlayerExit += OnPlayerExit;

            _isInitialized = true;
        }

        void OnDestroy()
        {
            if (_isInitialized)
            {
                _cloak.OnPlayerEnter -= OnPlayerEnter;
                _cloak.OnPlayerExit -= OnPlayerExit;
            }
        }

        private void SetUpList()
        {
            _renderers = _root.GetComponentsInChildren<Renderer>().ToList();
        }

        public void OnPlayerEnter()
        {
            SetUpList();

            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = false;
            }
        }

        public void OnPlayerExit()
        {
            SetUpList();

            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = true;
            }
        }

        public void EnableCloak()
        {
            SunLightController.RegisterSunOverrider(_cloak, 900);
            _cloak._cloakSphereRenderer.SetActivation(true);
            Shader.EnableKeyword("_CLOAKINGFIELDENABLED");
            _cloak._cloakVisualsEnabled = true;
        }

        public void DisableCloak()
        {
            SunLightController.UnregisterSunOverrider(_cloak);
            _cloak._cloakSphereRenderer.SetActivation(false);
            Shader.DisableKeyword("_CLOAKINGFIELDENABLED");
            _cloak._cloakVisualsEnabled = false;
        }

        public void SetReferenceFrameVolumeActive(bool active) => _cloak._referenceFrameVolume.gameObject.SetActive(active);
        public void EnableReferenceFrameVolume() => SetReferenceFrameVolumeActive(true);
        public void DisableReferenceFrameVolume() => SetReferenceFrameVolumeActive(false);
    }
}
