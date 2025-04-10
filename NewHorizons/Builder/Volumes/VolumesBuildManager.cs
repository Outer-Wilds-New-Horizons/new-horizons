using NewHorizons.Builder.ShipLog;
using NewHorizons.Builder.Volumes.Rulesets;
using NewHorizons.Builder.Volumes.VisorEffects;
using NewHorizons.Components.Volumes;
using NewHorizons.External.Configs;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using UnityEngine;

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
                        NHLogger.LogError($"Couldn't make reveal location [{revealInfo.reveals}] for [{go.name}]:\n{ex}");
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
            if (config.Volumes.dayNightAudioVolumes != null)
            {
                foreach (var dayNightAudioVolume in config.Volumes.dayNightAudioVolumes)
                {
                    DayNightAudioVolumeBuilder.Make(go, sector, dayNightAudioVolume, mod);
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
            if (config.Volumes.visorEffects != null)
            {
                if (config.Volumes.visorEffects.frostEffectVolumes != null)
                {
                    foreach (var frostEffectVolume in config.Volumes.visorEffects.frostEffectVolumes)
                    {
                        VisorFrostEffectVolumeBuilder.Make(go, sector, frostEffectVolume);
                    }
                }
                if (config.Volumes.visorEffects.rainEffectVolumes != null)
                {
                    foreach (var rainEffectVolume in config.Volumes.visorEffects.rainEffectVolumes)
                    {
                        VisorRainEffectVolumeBuilder.Make(go, sector, rainEffectVolume);
                    }
                }
            }
            if (config.Volumes.rulesets != null)
            {
                if (config.Volumes.rulesets.antiTravelMusicRulesets != null)
                {
                    foreach (var antiTravelMusicRuleset in config.Volumes.rulesets.antiTravelMusicRulesets)
                    {
                        VolumeBuilder.Make<AntiTravelMusicRuleset>(go, sector, antiTravelMusicRuleset);
                    }
                }
                if (config.Volumes.rulesets.playerImpactRulesets != null)
                {
                    foreach (var playerImpactRuleset in config.Volumes.rulesets.playerImpactRulesets)
                    {
                        PlayerImpactRulesetBuilder.Make(go, sector, playerImpactRuleset);
                    }
                }
                if (config.Volumes.rulesets.probeRulesets != null)
                {
                    foreach (var probeRuleset in config.Volumes.rulesets.probeRulesets)
                    {
                        ProbeRulesetBuilder.Make(go, sector, probeRuleset);
                    }
                }
                if (config.Volumes.rulesets.thrustRulesets != null)
                {
                    foreach (var thrustRuleset in config.Volumes.rulesets.thrustRulesets)
                    {
                        ThrustRulesetBuilder.Make(go, sector, thrustRuleset);
                    }
                }
            }
            if (config.Volumes.referenceFrameBlockerVolumes != null)
            {
                foreach (var referenceFrameBlockerVolume in config.Volumes.referenceFrameBlockerVolumes)
                {
                    VolumeBuilder.Make<ReferenceFrameBlockerVolume>(go, sector, referenceFrameBlockerVolume);
                }
            }
            if (config.Volumes.speedTrapVolumes != null)
            {
                foreach (var speedTrapVolume in config.Volumes.speedTrapVolumes)
                {
                    SpeedTrapVolumeBuilder.Make(go, sector, speedTrapVolume);
                }
            }
            if (config.Volumes.speedLimiterVolumes != null)
            {
                foreach (var speedLimiterVolume in config.Volumes.speedLimiterVolumes)
                {
                    SpeedLimiterVolumeBuilder.Make(go, sector, speedLimiterVolume);
                }
            }
            if (config.Volumes.lightSourceVolumes != null)
            {
                foreach (var lightSourceVolume in config.Volumes.lightSourceVolumes)
                {
                    VolumeBuilder.Make<LightlessLightSourceVolume>(go, sector, lightSourceVolume);
                }
            }
            if (config.Volumes.solarSystemVolume != null)
            {
                foreach (var solarSystemVolume in config.Volumes.solarSystemVolume)
                {
                    ChangeStarSystemVolumeBuilder.Make(go, sector, solarSystemVolume);
                }
            }
            if (config.Volumes.creditsVolume != null)
            {
                foreach (var creditsVolume in config.Volumes.creditsVolume)
                {
                    CreditsVolumeBuilder.Make(go, sector, creditsVolume);
                }
            }
        }
    }
}
