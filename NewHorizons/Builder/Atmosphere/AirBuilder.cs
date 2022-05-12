using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Atmosphere
{
    public static class AirBuilder
    {
        public static void Make(GameObject body, Sector sector, AtmosphereModule.AirInfo info)
        {
            GameObject airGO = new GameObject("Air");
            airGO.SetActive(false);
            airGO.layer = 17;
            airGO.transform.parent = sector?.transform ?? body.transform;

            SphereCollider SC = airGO.AddComponent<SphereCollider>();
            SC.isTrigger = true;
            SC.radius = info.Scale;

            SimpleFluidVolume SFV = airGO.AddComponent<SimpleFluidVolume>();
            SFV._layer = 5;
            SFV._priority = 1;
            SFV._density = 1.2f;
            SFV._fluidType = FluidVolume.Type.AIR;
            SFV._allowShipAutoroll = true;
            SFV._disableOnStart = false;

            if(info.HasOxygen)
            {
                airGO.AddComponent<OxygenVolume>();
            }

            if (info.IsRaining)
            {
                VisorRainEffectVolume VREF = airGO.AddComponent<VisorRainEffectVolume>();
                VREF._rainDirection = VisorRainEffectVolume.RainDirection.Radial;
                VREF._layer = 0;
                VREF._priority = 0;

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

                OWAudioSource OWAS = airGO.AddComponent<OWAudioSource>();
                OWAS._audioLibraryClip = AudioType.GD_RainAmbient_LP;
                OWAS.SetClipSelectionType(OWAudioSource.ClipSelectionOnPlay.RANDOM);
                OWAS.SetTrack(OWAudioMixer.TrackName.Environment);

                airGO.AddComponent<AudioVolume>();
            }

            airGO.transform.localPosition = Vector3.zero;
            airGO.SetActive(true);
        }
    }
}
