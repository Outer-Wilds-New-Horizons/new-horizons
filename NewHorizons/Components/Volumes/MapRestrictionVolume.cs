using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class MapRestrictionVolume : BaseVolume
    {
        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                Locator.GetMapController()?.OnPlayerEnterMapRestriction();
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                Locator.GetMapController()?.OnPlayerExitMapRestriction();
            }
        }
    }
}
