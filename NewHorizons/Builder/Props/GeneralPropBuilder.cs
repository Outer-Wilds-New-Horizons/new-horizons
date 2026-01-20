using NewHorizons.External.Modules;
using NewHorizons.External.SerializableData;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class GeneralPropBuilder
    {
        #region obsolete
        // Changed to ref sector
        [Obsolete]
        public static GameObject MakeFromExisting(GameObject go, GameObject planetGO, Sector sector, GeneralPointPropInfo info, MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
            => MakeFromExisting(go, planetGO, ref sector, info, defaultPosition, defaultParentPath, defaultParent);
        [Obsolete]
        public static GameObject MakeNew(string defaultName, GameObject planetGO, Sector sector, GeneralPointPropInfo info, MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
            => MakeNew(defaultName, planetGO, ref sector, info, defaultPosition, defaultParentPath, defaultParent);
        [Obsolete]
        public static GameObject MakeFromPrefab(GameObject prefab, string defaultName, GameObject planetGO, Sector sector, GeneralPointPropInfo info, MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
            => MakeFromPrefab(prefab, defaultName, planetGO, ref sector, info, defaultPosition, defaultParentPath, defaultParent);
        #endregion

        public static GameObject MakeFromExisting(GameObject go,
            GameObject planetGO, ref Sector sector, GeneralPointPropInfo info,
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

            sector = GetPropSector(go, planetGO, sector, info);

            return go;
        }

        public static GameObject MakeNew(string defaultName,
            GameObject planetGO, ref Sector sector, GeneralPointPropInfo info,
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
        {
            var go = new GameObject(defaultName);
            go.SetActive(false);
            return MakeFromExisting(go, planetGO, ref sector, info, defaultPosition, defaultParentPath, defaultParent);
        }

        public static GameObject MakeFromPrefab(GameObject prefab, string defaultName,
            GameObject planetGO, ref Sector sector, GeneralPointPropInfo info,
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform defaultParent = null)
        {
            var go = prefab.InstantiateInactive();
            go.name = defaultName;
            return MakeFromExisting(go, planetGO, ref sector, info, defaultPosition, defaultParentPath, defaultParent);
        }

        static Sector GetPropSector(GameObject go, GameObject planetGO, Sector sector, BasePropInfo info)
        {
            if (string.IsNullOrEmpty(info.sectorPath))
            {
                return sector;
            }
            else if (info.sectorPath == "auto")
            {
                return go.GetComponentInParent<Sector>();
            }
            else
            {
                var newSectorObj = planetGO.transform.Find(info.sectorPath);
                if (newSectorObj != null)
                {
                    var newSector = newSectorObj.GetComponent<Sector>();
                    if (newSector != null)
                    {
                        return newSector;
                    }
                }
                NHLogger.LogError($"Cannot find sector at path: {planetGO.name}/{info.sectorPath}");
                return sector;
            }
        }
    }
}
