using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OuterWilds;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumeBuilder
    {
        public static TVolume MakeExisting<TVolume>(GameObject go, GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour
        {
            // Respect existing layer if set to a valid volume layer
            if (go.layer != Layer.AdvancedEffectVolume)
            {
                go.layer = Layer.BasicEffectVolume;
            }

            // Skip creating a trigger volume if one already exists and has a shape set and we aren't overriding it
            var trigger = go.GetComponent<OWTriggerVolume>();
            if (trigger == null || (trigger._shape == null && trigger._owCollider == null) || info.shape != null || info.radius > 0f)
            {
                ShapeBuilder.AddTriggerVolume(go, info.shape, info.radius);
            }

            var volume = go.AddComponent<TVolume>();
            
            return volume;
        }

        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour //Could be BaseVolume but I need to create vanilla volumes too.
        {
            var go = GeneralPropBuilder.MakeNew(typeof(TVolume).Name, planetGO, sector, info);
            return MakeExisting<TVolume>(go, planetGO, sector, info);
        }

        public static TVolume MakeAndEnable<TVolume>(GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour
        {
            var volume = Make<TVolume>(planetGO, sector, info);
            volume.gameObject.SetActive(true);
            return volume;
        }
    }
}
