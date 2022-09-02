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
        private string _id = Guid.NewGuid().ToString();

        public override void OnTriggerVolumeEntry(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                InterferenceHandler.OnPlayerEnterInterferenceVolume(_id);
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                InterferenceHandler.OnProbeEnterInterferenceVolume(_id);
            }
            else if (hitObj.CompareTag("ShipDetector"))
            {
                InterferenceHandler.OnShipEnterInterferenceVolume(_id);
            }
        }

        public override void OnTriggerVolumeExit(GameObject hitObj)
        {
            if (hitObj.CompareTag("PlayerDetector"))
            {
                InterferenceHandler.OnPlayerExitInterferenceVolume(_id);
            }
            else if (hitObj.CompareTag("ProbeDetector"))
            {
                InterferenceHandler.OnProbeExitInterferenceVolume(_id);
            }
            else if (hitObj.CompareTag("ShipDetector"))
            {
                InterferenceHandler.OnShipExitInterferenceVolume(_id);
            }
        }

        public void OnPlayerEnter() => InterferenceHandler.OnPlayerEnterInterferenceVolume(_id);
        public void OnPlayerExit() => InterferenceHandler.OnPlayerExitInterferenceVolume(_id);

        public void OnProbeEnter() => InterferenceHandler.OnProbeEnterInterferenceVolume(_id);
        public void OnProbeExit() => InterferenceHandler.OnProbeExitInterferenceVolume(_id);

        public void OnShipEnter() => InterferenceHandler.OnShipEnterInterferenceVolume(_id);
        public void OnShipExit() => InterferenceHandler.OnShipExitInterferenceVolume(_id);
    }
}
