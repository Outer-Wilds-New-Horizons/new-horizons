using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class NomaiWarpPlatformEntrywayController : MonoBehaviour
    {
        NomaiWarpPlatform _platform;

        void Awake()
        {
            _platform = GetComponent<NomaiWarpPlatform>();
            _platform.OnReceiveWarpedBody += OnReceiveWarpedBody;
        }

        void OnDestroy()
        {
            _platform.OnReceiveWarpedBody -= OnReceiveWarpedBody;
        }

        private void OnReceiveWarpedBody(OWRigidbody warpedBody, NomaiWarpPlatform startPlatform, NomaiWarpPlatform targetPlatform)
        {
            EntrywayHandler.RemoveBodyFromTriggerVolumes(warpedBody, startPlatform.GetComponent<EntrywayVolumeHelper>());
            EntrywayHandler.AddBodyToTriggerVolumes(warpedBody, targetPlatform.GetComponent<EntrywayVolumeHelper>());
        }
    }
}
