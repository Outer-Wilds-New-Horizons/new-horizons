using NewHorizons.OtherMods.AchievementsPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Achievement
{
    // Modified version of ShipLogFactSnapshotTrigger
    public class AchievementSnapshotTrigger : MonoBehaviour
    {
        private void Awake()
        {
            _visibilityTracker = GetComponent<VisibilityTracker>();
            GlobalMessenger<ProbeCamera>.AddListener("ProbeSnapshot", new Callback<ProbeCamera>(OnProbeSnapshot));
        }

        private void OnDestroy()
        {
            GlobalMessenger<ProbeCamera>.RemoveListener("ProbeSnapshot", new Callback<ProbeCamera>(OnProbeSnapshot));
        }

        private void OnProbeSnapshot(ProbeCamera probeCamera)
        {
            if (_visibilityTracker != null && _visibilityTracker.IsVisibleToProbe(probeCamera.GetOWCamera()) && (_visibilityTracker.transform.position - probeCamera.transform.position).magnitude < maxDistance)
            {
                AchievementHandler.Earn(achievementID);
            }
        }

        public string achievementID;
        public float maxDistance = 200f;

        private VisibilityTracker _visibilityTracker;
    }
}
