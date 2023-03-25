using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    internal class WarpVolume : BaseVolume
    {
        public string TargetSolarSystem;

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                if (Main.Instance.CurrentStarSystem != TargetSolarSystem) // Otherwise it really breaks idk why
                {
                    Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem, PlayerState.AtFlightConsole());
                }
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {

        }
    }
}
