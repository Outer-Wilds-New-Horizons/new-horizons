using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class ConditionTriggerVolume : BaseVolume
    {
        public string Condition { get; set; }
        public bool Persistent { get; set; }
        public bool Reversible { get; set; }
        public bool Player { get; set; } = true;
        public bool Probe { get; set; }
        public bool Ship { get; set; }

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (TestHitObject(hitObj))
            {
                if (Persistent)
                {
                    PlayerData.SetPersistentCondition(Condition, true);
                }
                else
                {
                    DialogueConditionManager.SharedInstance.SetConditionState(Condition, true);
                }
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (Reversible && TestHitObject(hitObj))
            {
                if (Persistent)
                {
                    PlayerData.SetPersistentCondition(Condition, false);
                }
                else
                {
                    DialogueConditionManager.SharedInstance.SetConditionState(Condition, false);
                }
            }
        }

        bool TestHitObject(GameObject hitObj)
        {
            if (Player && hitObj.CompareTag("PlayerDetector"))
            {
                return true;
            }
            if (Probe && hitObj.CompareTag("ProbeDetector"))
            {
                return true;
            }
            if (Ship && hitObj.CompareTag("ShipDetector"))
            {
                return true;
            }
            return false;
        }
    }
}
