using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    internal class WaterCloakFixerVolume : MonoBehaviour
    {
        public Material material;
        private OWTriggerVolume _volume;

        public void Start()
        {
            _volume = GetComponent<RadialFluidVolume>().GetOWTriggerVolume();

            _volume.OnEntry += WaterCloakFixerVolume_OnEntry;
            _volume.OnExit += WaterCloakFixerVolume_OnExit;

            material.renderQueue = 3000;
        }

        public void OnDestroy()
        {
            _volume.OnEntry -= WaterCloakFixerVolume_OnEntry;
            _volume.OnExit -= WaterCloakFixerVolume_OnExit;
        }

        private void WaterCloakFixerVolume_OnEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                material.renderQueue = 2990;
            }
        }

        private void WaterCloakFixerVolume_OnExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                material.renderQueue = 3000;
            }
        }
    }
}
