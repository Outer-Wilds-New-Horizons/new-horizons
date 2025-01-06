using NewHorizons.Handlers;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    internal class WarpVolume : BaseVolume
    {
        public string TargetSolarSystem;
        public string TargetSpawnID;

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                if (Main.Instance.CurrentStarSystem != TargetSolarSystem) // Otherwise it really breaks idk why
                {
                    Main.Instance.ChangeCurrentStarSystem(TargetSolarSystem, PlayerState.AtFlightConsole());
                    PlayerSpawnHandler.TargetSpawnID = TargetSpawnID;
                }
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {

        }
    }
}
