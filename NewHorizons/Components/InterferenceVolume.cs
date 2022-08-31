using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class InterferenceVolume : BaseVolume
    {
        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                InterferenceHandler.OnPlayerEnterInterferenceVolume();
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                InterferenceHandler.OnProbeEnterInterferenceVolume();
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                InterferenceHandler.OnPlayerExitInterferenceVolume();
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                InterferenceHandler.OnProbeExitInterferenceVolume();
            }
        }
    }
}
