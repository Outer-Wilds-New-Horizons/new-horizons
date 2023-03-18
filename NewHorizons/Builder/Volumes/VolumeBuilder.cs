using NewHorizons.Builder.Props;
using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.External.Volumes;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour //Could be BaseVolume but I need to create vanilla volumes too.
        {
            var go = GeneralPropBuilder.MakeNew(typeof(TVolume).Name, sector?.transform ?? planetGO.transform, info);
            go.layer = LayerMask.NameToLayer("BasicEffectVolume");

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
