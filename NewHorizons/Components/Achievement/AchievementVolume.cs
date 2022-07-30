using NewHorizons.AchievementsPlus;
using UnityEngine;

namespace NewHorizons.Components.Achievement
{
    public class AchievementVolume : MonoBehaviour
    {
        private void Start()
        {
            _trigger = gameObject.GetRequiredComponent<OWTriggerVolume>();
            _trigger.OnEntry += OnEntry;
        }

        private void OnDestroy()
        {
            _trigger.OnEntry -= OnEntry;
        }

        private void OnEntry(GameObject hitObj)
        {
            if ((!player || hitObj.CompareTag("PlayerDetector")) && (!probe || hitObj.CompareTag("ProbeDetector")))
            {
                AchievementHandler.Earn(achievementID);

                _trigger.OnEntry -= OnEntry;
            }
        }

        public string achievementID;

        public bool player = true;
        public bool probe;

        private OWTriggerVolume _trigger;
    }
}
