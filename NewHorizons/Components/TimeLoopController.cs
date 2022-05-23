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
            // Stock gives like 33 seconds after the sun collapses
            // Gonna assume it takes like 7 seconds to collapse after the supernova trigger
            if (_supernovaHappened && Time.time > _supernovaTime + 40f)
                Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
        }

        public void OnTriggerSupernova()
        {
            _supernovaHappened = true;
            _supernovaTime = Time.time;
        }
    }
}