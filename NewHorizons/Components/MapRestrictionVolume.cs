using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    [RequireComponent(typeof(OWTriggerVolume))]
    public class MapRestrictionVolume : MonoBehaviour
    {
        private OWTriggerVolume _triggerVolume;

        public void Awake()
        {
            _triggerVolume = this.GetRequiredComponent<OWTriggerVolume>();
            _triggerVolume.OnEntry += OnTriggerVolumeEntry;
            _triggerVolume.OnExit += OnTriggerVolumeExit;
        }

        public void OnDestroy()
        {
            if (_triggerVolume == null) return;
            _triggerVolume.OnEntry -= OnTriggerVolumeEntry;
            _triggerVolume.OnExit -= OnTriggerVolumeExit;
        }

        public void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                Locator.GetMapController()?.OnPlayerEnterMapRestriction();
            }
        }

        public void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                Locator.GetMapController()?.OnPlayerExitMapRestriction();
            }
        }
    }
}
