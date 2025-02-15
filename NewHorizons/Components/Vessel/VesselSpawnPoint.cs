using NewHorizons.Handlers;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    [UsedInUnityProject]
    public class VesselSpawnPoint : EyeSpawnPoint
    {
        public GameObject warpControllerObject;
        private VesselWarpController _warpController;

        public VesselWarpController WarpController
        {
            get
            {
                if (_warpController == null && warpControllerObject != null) WarpController = warpControllerObject.GetComponent<VesselWarpController>();
                return _warpController;
            }
            set
            {
                _warpController = value;
                if (_warpController != null) _triggerVolumes = new OWTriggerVolume[1] { _warpController._bridgeVolume };
            }
        }

        public VesselSpawnPoint()
        {
            _eyeState = EyeState.AboardVessel;
        }

        public int index = 0;

        public void WarpPlayer(bool spawn = false)
        {
            var warpController = WarpController;
            var player = Locator.GetPlayerBody();
            var relativeTransform = warpController.transform;
            var relativeBody = relativeTransform.GetAttachedOWRigidbody();
            if (!spawn) Locator.GetPlayerCamera().GetComponent<PlayerCameraEffectController>().OpenEyesImmediate();
            if (VesselWarpController.s_relativeLocationSaved)
            {
                Locator.GetPlayerBody().MoveToRelativeLocation(VesselWarpController.s_playerWarpLocation, relativeBody, relativeTransform);
            }
            else
            {
                player.SetPosition(warpController._defaultPlayerWarpPoint.position);
                player.SetRotation(warpController._defaultPlayerWarpPoint.rotation);
                player.SetVelocity(relativeBody.GetPointVelocity(warpController._defaultPlayerWarpPoint.position));
            }
            AddPlayerToVolume(warpController._bridgeVolume);
            AddPlayerToTriggerVolumes();
        }

        public override void OnSpawnPlayer()
        {
            Delay.FireOnNextUpdate(() => WarpPlayer(true));
        }

        public void AddPlayerToVolume(OWTriggerVolume volume)
        {
            volume.AddObjectToVolume(Locator.GetPlayerDetector());
            volume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
        }

        public void AddPlayerToTriggerVolumes()
        {
            AddObjectToTriggerVolumes(Locator.GetPlayerDetector());
            AddObjectToTriggerVolumes(Locator.GetPlayerCameraDetector());
        }
    }
}
