using NewHorizons.External.Modules;
using UnityEngine;


namespace NewHorizons.Components.Volumes
{
    internal class LoadCreditsVolume : BaseVolume
    {
        public GameOverModule gameOver;
        public DeathType? deathType;

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector") && enabled && (string.IsNullOrEmpty(gameOver.condition) || DialogueConditionManager.SharedInstance.GetConditionState(gameOver.condition)))
            {
                NHGameOverManager.Instance.StartGameOverSequence(gameOver, deathType);
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj) { }
    }
}
