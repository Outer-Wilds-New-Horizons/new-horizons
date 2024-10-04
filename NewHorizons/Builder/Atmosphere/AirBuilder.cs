using NewHorizons.External.Configs;
using NewHorizons.Utility.OuterWilds;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class AirBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, PlanetConfig config)
        {
            var airGO = new GameObject("Air");
            airGO.SetActive(false);
            airGO.layer = Layer.BasicEffectVolume;
            airGO.transform.parent = sector?.transform ?? planetGO.transform;

            var sc = airGO.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = config.Atmosphere.size;

            // copied from gd
            var sfv = airGO.AddComponent<SimpleFluidVolume>();
            sfv._layer = 5;
            sfv._priority = 0;
            sfv._density = 1.2f;
            sfv._fluidType = FluidVolume.Type.AIR;
            sfv._allowShipAutoroll = config.Atmosphere.allowShipAutoroll;
            sfv._disableOnStart = false;

            if (config.Atmosphere.hasShockLayer)
            {
                // Try to parent it to the same as other rulesets to match vanilla but if its null put it on the root object
                var ruleSetGO = planetGO.GetComponentInChildren<PlanetoidRuleset>()?.gameObject;
                if (ruleSetGO == null) ruleSetGO = planetGO;

                var shockLayerRuleset = ruleSetGO.AddComponent<ShockLayerRuleset>();
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
            }

            if (config.Atmosphere.hasOxygen)
            {
                airGO.AddComponent<OxygenVolume>()._treeVolume = config.Atmosphere.hasTrees;
            }

            if (config.Atmosphere.hasRain)
            {
                var vref = airGO.AddComponent<VisorRainEffectVolume>();
                vref._rainDirection = VisorRainEffectVolume.RainDirection.Radial;
                vref._layer = 0;
                vref._priority = 0;

                var AS = airGO.AddComponent<AudioSource>();
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
