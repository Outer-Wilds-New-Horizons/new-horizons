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
        private Sector _sector;

        private bool _isInitialized;

        public void Init(CloakFieldController cloak, Sector sector)
        {
            _cloak = cloak;
            _sector = sector;

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

        public void OnPlayerEnter()
        {
            foreach(Transform child in _sector.transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        public void OnPlayerExit()
        {
            foreach (Transform child in _sector.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
