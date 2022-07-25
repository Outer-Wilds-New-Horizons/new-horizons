using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class AirBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, PlanetConfig config)
        {
            GameObject airGO = new GameObject("Air");
            airGO.SetActive(false);
            airGO.layer = 17;
            airGO.transform.parent = sector?.transform ? sector.transform : planetGO.transform;

            SphereCollider sc = airGO.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = config.Atmosphere.size;

            SimpleFluidVolume sfv = airGO.AddComponent<SimpleFluidVolume>();
            sfv._layer = 5;
            sfv._priority = 1;
            sfv._density = 1.2f;
            sfv._fluidType = FluidVolume.Type.AIR;
            sfv._allowShipAutoroll = true;
            sfv._disableOnStart = false;

            ShockLayerRuleset shockLayerRuleset = planetGO.GetComponentInChildren<PlanetoidRuleset>().gameObject.AddComponent<ShockLayerRuleset>();
            shockLayerRuleset._type = ShockLayerRuleset.ShockType.Atmospheric;
            shockLayerRuleset._radialCenter = airGO.transform;
            shockLayerRuleset._minShockSpeed = config.Atmosphere.minShockSpeed;
            shockLayerRuleset._maxShockSpeed = config.Atmosphere.maxShockSpeed;

            if (config.Atmosphere.clouds != null)
            {
                shockLayerRuleset._innerRadius = config.Atmosphere.clouds.innerCloudRadius;
                shockLayerRuleset._outerRadius = config.Atmosphere.clouds.outerCloudRadius;
            }
            else
            {
                var bottom = config.Base.surfaceSize;
                var top = config.Atmosphere.size;

                shockLayerRuleset._innerRadius = (bottom + top) / 2f;
                shockLayerRuleset._outerRadius = top;
            }

            if (config.Atmosphere.hasOxygen)
            {
                airGO.AddComponent<OxygenVolume>();
            }

            if (config.Atmosphere.hasRain)
            {
                var vref = airGO.AddComponent<VisorRainEffectVolume>();
                vref._rainDirection = VisorRainEffectVolume.RainDirection.Radial;
                vref._layer = 0;
                vref._priority = 0;

                AudioSource AS = airGO.AddComponent<AudioSource>();
                AS.mute = false;
                AS.bypassEffects = false;
                AS.bypassListenerEffects = false;
                AS.bypassReverbZones = false;
                AS.playOnAwake = false;
                AS.loop = true;
                AS.priority = 128;
                AS.volume = 0.35f;
                AS.pitch = 1f;
                AS.panStereo = 0f;
                AS.spatialBlend = 0f;
                AS.reverbZoneMix = 1f;

                var owAudioSource = airGO.AddComponent<OWAudioSource>();
                owAudioSource._audioLibraryClip = AudioType.GD_RainAmbient_LP;
                owAudioSource.SetClipSelectionType(OWAudioSource.ClipSelectionOnPlay.RANDOM);
                owAudioSource.SetTrack(OWAudioMixer.TrackName.Environment);

                airGO.AddComponent<AudioVolume>();
            }

            airGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            airGO.SetActive(true);
        }
    }
}
