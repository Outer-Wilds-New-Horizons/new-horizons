using NewHorizons.Builder.Body;
using NewHorizons.Builder.ShipLog;
using NewHorizons.Builder.Volumes;
using NewHorizons.Components.Volumes;
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
                    VolumeBuilder.Make<Components.Volumes.InterferenceVolume>(go, sector, interferenceVolume);
                }
            }
            if (config.Volumes.reverbVolumes != null)
            {
                foreach (var reverbVolume in config.Volumes.reverbVolumes)
                {
                    VolumeBuilder.Make<ReverbTriggerVolume>(go, sector, reverbVolume);
                }
            }
            if (config.Volumes.insulatingVolumes != null)
            {
                foreach (var insulatingVolume in config.Volumes.insulatingVolumes)
                {
                    VolumeBuilder.Make<InsulatingVolume>(go, sector, insulatingVolume);
                }
            }
            if (config.Volumes.zeroGravityVolumes != null)
            {
                foreach (var zeroGravityVolume in config.Volumes.zeroGravityVolumes)
                {
                    ZeroGVolumeBuilder.Make(go, sector, zeroGravityVolume);
                }
            }
            if (config.Volumes.destructionVolumes != null)
            {
                foreach (var destructionVolume in config.Volumes.destructionVolumes)
                {
                    DestructionVolumeBuilder.Make(go, sector, destructionVolume);
                }
            }
            if (config.Volumes.oxygenVolumes != null)
            {
                foreach (var oxygenVolume in config.Volumes.oxygenVolumes)
                {
                    OxygenVolumeBuilder.Make(go, sector, oxygenVolume);
                }
            }
            if (config.Volumes.fluidVolumes != null)
            {
                foreach (var fluidVolume in config.Volumes.fluidVolumes)
                {
                    FluidVolumeBuilder.Make(go, sector, fluidVolume);
                }
            }
            if (config.Volumes.probe != null)
            {
                if (config.Volumes.probe.destructionVolumes != null)
                {
                    foreach (var destructionVolume in config.Volumes.probe.destructionVolumes)
                    {
                        VolumeBuilder.Make<ProbeDestructionVolume>(go, sector, destructionVolume);
                    }
                }
                if (config.Volumes.probe.safetyVolumes != null)
                {
                    foreach (var safetyVolume in config.Volumes.probe.safetyVolumes)
                    {
                        VolumeBuilder.Make<ProbeSafetyVolume>(go, sector, safetyVolume);
                    }
                }
            }
        }
    }
}
