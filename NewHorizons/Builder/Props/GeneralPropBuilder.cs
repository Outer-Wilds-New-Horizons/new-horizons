using NewHorizons.External.Modules;
using NewHorizons.External.SerializableData;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class GeneralPropBuilder
    {
        public static GameObject MakeFromExisting(GameObject go,
            GameObject planetGO, Sector sector, GeneralPointPropInfo info,
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
        {
            if (info == null) return go;

            go.transform.parent = defaultParent ?? sector?.transform ?? planetGO?.transform;

            if (info is GeneralSolarSystemPropInfo solarSystemInfo && !string.IsNullOrEmpty(solarSystemInfo.parentBody))
            {
                // This can fail if the prop is built before the target planet. Only use it for SolarSystem module props
                var targetPlanet = AstroObjectLocator.GetAstroObject(solarSystemInfo.parentBody);
                if (targetPlanet != null)
                {
                    planetGO = targetPlanet.gameObject;
                    sector = targetPlanet.GetRootSector() ?? targetPlanet.GetComponentInChildren<Sector>();
                    go.transform.parent = sector?.transform ?? planetGO?.transform ?? go.transform.parent;
                }
                else
                {
                    NHLogger.LogError($"Cannot find parent body named {solarSystemInfo.parentBody}");
                }
            }

            if (!string.IsNullOrEmpty(info.rename))
            {
                go.name = info.rename;
            }

            var parentPath = info.parentPath ?? defaultParentPath;

            if (planetGO != null && !string.IsNullOrEmpty(parentPath))
            {
                var newParent = planetGO.transform.Find(parentPath);
                if (newParent != null)
                {
                    go.transform.parent = newParent;
                    sector = newParent.GetComponentInParent<Sector>();
                }
                else
                {
                    NHLogger.LogError($"Cannot find parent object at path: {planetGO.name}/{parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? defaultPosition ?? Vector3.zero);
            var rot = Quaternion.identity;
            var alignRadial = false;
            if (info is GeneralPropInfo rotInfo)
            {
                rot = rotInfo.rotation != null ? Quaternion.Euler(rotInfo.rotation) : Quaternion.identity;
                alignRadial = rotInfo.alignRadial.HasValue && rotInfo.alignRadial.Value;
            }
            if (info.isRelativeToParent)
            {
                go.transform.localPosition = pos;
                go.transform.localRotation = rot;
            }
            else if (planetGO != null)
            {
                go.transform.position = planetGO.transform.TransformPoint(pos);
                go.transform.rotation = planetGO.transform.TransformRotation(rot);
            }
            else
            {
                go.transform.position = pos;
                go.transform.rotation = rot;
            }
            if (alignRadial && planetGO != null)
            {
                var up = (go.transform.position - planetGO.transform.position).normalized;
                go.transform.rotation = Quaternion.FromToRotation(Vector3.up, up) * rot;
            }
            return go;
        }

        public static GameObject MakeNew(string defaultName,
            GameObject planetGO, Sector sector, GeneralPointPropInfo info,
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
        {
            var go = new GameObject(defaultName);
            go.SetActive(false);
            return MakeFromExisting(go, planetGO, sector, info, defaultPosition, defaultParentPath, defaultParent);
        }

        public static GameObject MakeFromPrefab(GameObject prefab, string defaultName,
            GameObject planetGO, Sector sector, GeneralPointPropInfo info,
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
        {
            var go = prefab.InstantiateInactive();
            go.name = defaultName;
            return MakeFromExisting(go, planetGO, sector, info, defaultPosition, defaultParentPath, defaultParent);
        }
    }
}
