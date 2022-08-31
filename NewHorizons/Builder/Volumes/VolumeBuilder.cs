using NewHorizons.Components;
using NewHorizons.External.Modules;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumesModule.VolumeInfo info) where TVolume : BaseVolume
        {
            var go = new GameObject(typeof(TVolume).Name);
            go.SetActive(false);

            go.transform.parent = sector?.transform ?? planetGO.transform;
            go.transform.position = planetGO.transform.TransformPoint(info.position != null ? (Vector3)info.position : Vector3.zero);
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
