using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(_isInitialized)
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
    }
}
