using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Volumes;
using NewHorizons.Utility.OWUtilities;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour //Could be BaseVolume but I need to create vanilla volumes too.
        {
            var go = GeneralPropBuilder.MakeNew(typeof(TVolume).Name, planetGO, sector, info);
            go.layer = Layer.BasicEffectVolume;

            var shape = go.AddComponent<SphereShape>();
            shape.radius = info.radius;

            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();
            owTriggerVolume._shape = shape;

            var volume = go.AddComponent<TVolume>();

            go.SetActive(true);

            return volume;
        }
    }
}
