using NewHorizons.Builder.Body;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Builder.Volumes;
using NewHorizons.Components;
using NewHorizons.External.Configs;
using OWML.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumesBuildManager
    {
        public static void Make(GameObject go, Sector sector, OWRigidbody planetBody, PlanetConfig config, IModBehaviour mod)
        {
            if (config.Volumes.revealVolumes != null)
            {
                foreach (var revealInfo in config.Volumes.revealVolumes)
                {
                    try
                    {
                        RevealBuilder.Make(go, sector, revealInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make reveal location [{revealInfo.reveals}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Volumes.audioVolumes != null)
            {
                foreach (var audioVolume in config.Volumes.audioVolumes)
                {
                    AudioVolumeBuilder.Make(go, sector, audioVolume, mod);
                }
            }
            if (config.Volumes.notificationVolumes != null)
            {
                foreach (var notificationVolume in config.Volumes.notificationVolumes)
                {
                    NotificationVolumeBuilder.Make(go, sector, notificationVolume, mod);
                }
            }
            if (config.Volumes.hazardVolumes != null)
            {
                foreach (var hazardVolume in config.Volumes.hazardVolumes)
                {
                    HazardVolumeBuilder.Make(go, sector, planetBody, hazardVolume, mod);
                }
            }
            if (config.Volumes.mapRestrictionVolumes != null)
            {
                foreach (var mapRestrictionVolume in config.Volumes.mapRestrictionVolumes)
                {
                    VolumeBuilder.Make<MapRestrictionVolume>(go, sector, mapRestrictionVolume);
                }
            }
            if (config.Volumes.interferenceVolumes != null)
            {
                foreach (var interferenceVolume in config.Volumes.interferenceVolumes)
                {
                    VolumeBuilder.Make<Components.InterferenceVolume>(go, sector, interferenceVolume);
                }
            }
            if (config.Volumes.reverbVolumes != null)
            {
                foreach (var reverbVolume in config.Volumes.reverbVolumes)
                {
                    VolumeBuilder.Make<ReverbTriggerVolume>(go, sector, reverbVolume);
                }
            }
        }
    }
}
