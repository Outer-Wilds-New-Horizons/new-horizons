using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    /// <summary>
    /// A cloak can interfere with the rendering of water
    /// Water has a lower render queue and is transparent, so you can see the background black cloak over top of the water
    /// We fix this by setting the water's render queue to that of the cloak
    /// However, this means that when you are inside the water you will see through the cloak since it's not rendered on top
    /// To fix that, we set the render queue back to normal when the player enters the water
    /// Currently this doesnt nothing to fix probe camera pictures. If you are outside of the water, the probe will see the stars and through the cloak
    /// Oh well
    /// </summary>
    internal class WaterCloakFixerVolume : MonoBehaviour
    {
        public Material material;
        private OWTriggerVolume _volume;

        public const int WATER_RENDER_QUEUE = 2990;
        public const int CLOAK_RENDER_QUEUE = 3000;

        public void Start()
        {
            _volume = GetComponent<RadialFluidVolume>().GetOWTriggerVolume();

            _volume.OnEntry += WaterCloakFixerVolume_OnEntry;
            _volume.OnExit += WaterCloakFixerVolume_OnExit;

            material.renderQueue = CLOAK_RENDER_QUEUE;
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
                material.renderQueue = WATER_RENDER_QUEUE;
            }
        }

        private void WaterCloakFixerVolume_OnExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                material.renderQueue = CLOAK_RENDER_QUEUE;
            }
        }
    }
}
