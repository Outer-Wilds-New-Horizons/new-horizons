using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System;
using UnityEngine;

namespace NewHorizons.Builder.Volumes
{
    public static class VolumeBuilder
    {
        #region obsolete
        // Changed to ref sector
        [Obsolete]
        public static TVolume MakeExisting<TVolume>(GameObject go, GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour
            => MakeExisting<TVolume>(go, planetGO, ref sector, info);
        [Obsolete]
        public static TVolume Make<TVolume>(GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour
            => Make<TVolume>(planetGO, ref sector, info);
        // Intentionally not marking this one Obsolete because it's only used by VolumesBuildManager and would clutter that code
        public static TVolume MakeAndEnable<TVolume>(GameObject planetGO, Sector sector, VolumeInfo info) where TVolume : MonoBehaviour
            => MakeAndEnable<TVolume>(planetGO, ref sector, info);
        #endregion

        public static TVolume MakeExisting<TVolume>(GameObject go, GameObject planetGO, ref Sector sector, VolumeInfo info) where TVolume : MonoBehaviour
        {
            // Backwards compat for the two possible radii settings
            // Both radii default to 1
            if (info.shape != null && info.shape.type == External.Modules.Props.ShapeType.Sphere && info.shape.radius != info.radius)
            {
                // If the info shape radius if the default but the info radius is not the default, use it
                if (info.shape.radius == 1f && info.radius != 1f)
                {
                    info.shape.radius = info.radius;
                }
            }

            // Warning if you set the radius to not be one but are using a non sphere shape
            if (info.radius != 1f && (info.shape != null && info.shape.type != External.Modules.Props.ShapeType.Sphere))
            {
                NHLogger.LogError($"Volume [{typeof(TVolume).Name}] on [{go.name}] has a radius value set but it's shape is [{info.shape.type}]");
            }

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

        public static TVolume Make<TVolume>(GameObject planetGO, ref Sector sector, VolumeInfo info) where TVolume : MonoBehaviour // Could be BaseVolume but I need to create vanilla volumes too.
        {
            var go = GeneralPropBuilder.MakeNew(typeof(TVolume).Name, planetGO, ref sector, info);
            return MakeExisting<TVolume>(go, planetGO, ref sector, info);
        }

        public static TVolume MakeAndEnable<TVolume>(GameObject planetGO, ref Sector sector, VolumeInfo info) where TVolume : MonoBehaviour
        {
            var volume = Make<TVolume>(planetGO, ref sector, info);
            volume.gameObject.SetActive(true);
            return volume;
        }
    }
}
