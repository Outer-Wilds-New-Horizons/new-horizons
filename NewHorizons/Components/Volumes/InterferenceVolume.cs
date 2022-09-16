using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Volumes
{
    public class InterferenceVolume : BaseVolume
    {
        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                OnPlayerEnter();
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                OnProbeEnter();
            }
            else if (hitObj.CompareTag("ShipDetector"))
            {
                OnShipEnter();
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                OnPlayerExit();
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                OnProbeExit();
            }
            else if (hitObj.CompareTag("ShipDetector"))
            {
                OnShipExit();
            }
        }

        public void OnPlayerEnter() => InterferenceHandler.OnPlayerEnterInterferenceVolume(this);
        public void OnPlayerExit() => InterferenceHandler.OnPlayerExitInterferenceVolume(this);

        public void OnProbeEnter() => InterferenceHandler.OnProbeEnterInterferenceVolume(this);
        public void OnProbeExit() => InterferenceHandler.OnProbeExitInterferenceVolume(this);

        public void OnShipEnter() => InterferenceHandler.OnShipEnterInterferenceVolume(this);
        public void OnShipExit() => InterferenceHandler.OnShipExitInterferenceVolume(this);
    }
}
