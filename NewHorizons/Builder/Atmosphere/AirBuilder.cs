#region

using NewHorizons.External.Modules;
using UnityEngine;

#endregion

namespace NewHorizons.Builder.Atmosphere
{
    public static class AirBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule.AirInfo info)
        {
            var airGO = new GameObject("Air");
            airGO.SetActive(false);
            airGO.layer = 17;
            airGO.transform.parent = sector?.transform ? sector.transform : planetGO.transform;

            var sc = airGO.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = info.scale;

            var sfv = airGO.AddComponent<SimpleFluidVolume>();
            sfv._layer = 5;
            sfv._priority = 1;
            sfv._density = 1.2f;
            sfv._fluidType = FluidVolume.Type.AIR;
            sfv._allowShipAutoroll = true;
            sfv._disableOnStart = false;

            if (info.hasOxygen) airGO.AddComponent<OxygenVolume>();

            if (info.isRaining)
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