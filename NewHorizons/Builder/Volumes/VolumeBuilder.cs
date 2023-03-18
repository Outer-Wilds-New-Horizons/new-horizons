using NewHorizons.Components;
using NewHorizons.External.Modules;
using NewHorizons.Utility.OWUtilities;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumeBuilder
    {
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumesModule.VolumeInfo info) where TVolume : MonoBehaviour //Could be BaseVolume but I need to create vanilla volumes too.
        {
            var go = new GameObject(typeof(TVolume).Name);
            go.SetActive(false);

            go.transform.parent = sector?.transform ?? planetGO.transform;

            if (!string.IsNullOrEmpty(info.rename))
            {
                go.name = info.rename;
            }

            if (!string.IsNullOrEmpty(info.parentPath))
            {
                var newParent = planetGO.transform.Find(info.parentPath);
                if (newParent != null)
                {
                    go.transform.parent = newParent;
                }
                else
                {
                    Logger.LogError($"Cannot find parent object at path: {planetGO.name}/{info.parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? Vector3.zero);
            if (info.isRelativeToParent) go.transform.localPosition = pos;
            else go.transform.position = planetGO.transform.TransformPoint(pos);
            go.layer = LayerUtilities.BasicEffectVolume;

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
