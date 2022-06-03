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
            _cloak.OnProbeEnter = new OWEvent();
            _cloak.OnProbeExit = new OWEvent();
            _cloak.OnShipEnter = new OWEvent();
            _cloak.OnShipExit = new OWEvent();

            _cloak.OnPlayerEnter += OnPlayerEnter;
            _cloak.OnPlayerExit += OnPlayerExit;
            _cloak.OnProbeEnter += OnProbeEnter;
            _cloak.OnProbeExit += OnProbeExit;
            _cloak.OnShipEnter += OnShipEnter;
            _cloak.OnShipExit += OnShipExit;

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

            GlobalMessenger.FireEvent("PlayerEnterCloakField");
        }

        public void OnPlayerExit()
        {
            SetUpList();

            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = true;
            }

            GlobalMessenger.FireEvent("PlayerExitCloakField");
        }

        public void OnProbeEnter()
        {
            GlobalMessenger.FireEvent("ProbeEnterCloakField");
        }

        public void OnProbeExit()
        {
            GlobalMessenger.FireEvent("ProbeExitCloakField");
        }

        public void OnShipEnter()
        {
            GlobalMessenger.FireEvent("ShipEnterCloakField");
        }

        public void OnShipExit()
        {
            GlobalMessenger.FireEvent("ShipExitCloakField");
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

        public void TurnOnMusic() => _cloak._hasTriggeredMusic = false;
        public void TurnOffMusic() => _cloak._hasTriggeredMusic = true;
    }
}
