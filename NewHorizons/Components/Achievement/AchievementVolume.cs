using NewHorizons.AchievementsPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Achievement
{
    public class AchievementVolume : MonoBehaviour
    {
        private void Start()
        {
            _trigger = gameObject.GetRequiredComponent<OWTriggerVolume>();
            _trigger.OnEntry += OnEntry;
            return;
        }

        private void OnDestroy()
        {
            _trigger.OnEntry -= OnEntry;
        }

        private void OnEntry(GameObject hitObj)
        {
            if ((!_player || hitObj.CompareTag("PlayerDetector")) && (!_probe || hitObj.CompareTag("ProbeDetector")))
            {
                AchievementHandler.Earn(achievementID);

                _trigger.OnEntry -= OnEntry;
            }
        }

        public string achievementID;

        private bool _player = true;
        private bool _probe;

        private OWTriggerVolume _trigger;
    }
}
