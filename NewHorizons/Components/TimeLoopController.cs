using UnityEngine;
namespace NewHorizons.Components
{
    public class TimeLoopController : MonoBehaviour
    {
        private float _supernovaTime;
        private bool _supernovaHappened;

        public void Start()
        {
            GlobalMessenger.AddListener("TriggerSupernova", OnTriggerSupernova);
        }

        public void Update()
        {
            // So that mods can turn the time loop on/off using the TimLoop.SetTimeLoopEnabled method
            if (!TimeLoop._timeLoopEnabled) return;

            // Stock gives like 33 seconds after the sun collapses
            if (_supernovaHappened && Time.time > _supernovaTime + 50f)
            {
                Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
            }
        }

        public void OnTriggerSupernova()
        {
            _supernovaHappened = true;
            _supernovaTime = Time.time;
        }
    }
}
