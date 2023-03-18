using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OWUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class GeneralPropBuilder
    {
        public static GameObject MakeFromExisting(GameObject go,
            GameObject planetGO, Sector sector, GeneralPointPropInfo info,
            bool alignToBody = false, MVector3 normal = null,
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform parentOverride = null)
        {
            if (info == null) return go;

            go.transform.parent = parentOverride ?? sector?.transform ?? planetGO?.transform;

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
                    Logger.LogError($"Cannot find parent body named {solarSystemInfo.parentBody}");
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
                    Logger.LogError($"Cannot find parent object at path: {planetGO.name}/{parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? defaultPosition ?? Vector3.zero);
            var rot = Quaternion.identity;
            if (info is GeneralPropInfo rotInfo)
            {
                rot = rotInfo.rotation != null ? Quaternion.Euler(rotInfo.rotation) : Quaternion.identity;
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
            if (alignToBody)
            {
                var up = (go.transform.position - planetGO.transform.position).normalized;
                if (normal != null)
                {
                    if (info.isRelativeToParent)
                    {
                        up = go.transform.parent.TransformDirection(normal);
                    }
                    else
                    {
                        up = planetGO.transform.TransformDirection(normal);
                    }
                }
                go.transform.rotation = Quaternion.FromToRotation(go.transform.up, up) * rot;
            }
            return go;
        }

        public static GameObject MakeNew(string defaultName,
            GameObject planetGO, Sector sector, GeneralPointPropInfo info, 
            bool alignToBody = false, MVector3 normal = null,
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform parentOverride = null)
        {
            var go = new GameObject(defaultName);
            go.SetActive(false);
            return MakeFromExisting(go, planetGO, sector, info, alignToBody, normal, defaultPosition, defaultParentPath, parentOverride);
        }

        public static GameObject MakeFromPrefab(GameObject prefab, string defaultName,
            GameObject planetGO, Sector sector, GeneralPointPropInfo info,
            bool alignToBody = false, MVector3 normal = null, 
            MVector3 defaultPosition = null, string defaultParentPath = null, Transform parentOverride = null)
        {
            var go = prefab.InstantiateInactive();
            go.name = defaultName;
            return MakeFromExisting(go, planetGO, sector, info, alignToBody, normal, defaultPosition, defaultParentPath, parentOverride);
        }
    }
}
