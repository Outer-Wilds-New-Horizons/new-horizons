using NewHorizons.External.Modules;
using NewHorizons.Utility;
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
        public static GameObject MakeFromExisting(GameObject go, GameObject planetGO, Sector sector, PropModule.PositionedPropInfo info, bool alignToBody = false, MVector3 normal = null, MVector3 defaultPosition = null, string defaultParentPath = null)
        {
            if (!string.IsNullOrEmpty(info.rename))
            {
                go.name = info.rename;
            }

            go.transform.parent = sector?.transform ?? planetGO.transform;

            var parentPath = info.parentPath ?? defaultParentPath;

            if (!string.IsNullOrEmpty(parentPath))
            {
                var newParent = go.transform.Find(parentPath);
                if (newParent != null)
                {
                    go.transform.parent = newParent.transform;
                }
                else
                {
                    Logger.LogError($"Cannot find parent object at path: {go.name}/{parentPath}");
                }
            }

            var pos = (Vector3)(info.position ?? defaultPosition ?? Vector3.zero);
            var rot = Quaternion.identity;
            if (info is PropModule.PositionedAndRotatedPropInfo rotInfo)
            {
                rot = rotInfo.rotation != null ? Quaternion.Euler(rotInfo.rotation) : Quaternion.identity;
            }
            if (info.isRelativeToParent)
            {
                go.transform.localPosition = pos;
                go.transform.localRotation = rot;
            } else
            {
                go.transform.position = planetGO.transform.TransformPoint(pos);
                go.transform.rotation = planetGO.transform.TransformRotation(rot);
            }
            if (alignToBody)
            {
                var up = (go.transform.position - planetGO.transform.position).normalized;
                if (normal != null) up = planetGO.transform.TransformDirection(normal);
                go.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                go.transform.rotation *= rot;
            }
            return go;
        }

        public static GameObject MakeNew(string defaultName, GameObject planetGO, Sector sector, PropModule.PositionedPropInfo info, bool alignToBody = false, MVector3 normal = null, MVector3 defaultPosition = null, string defaultParentPath = null)
        {
            var go = new GameObject(defaultName);
            go.SetActive(false);
            return MakeFromExisting(go, planetGO, sector, info, alignToBody, normal, defaultPosition, defaultParentPath);
        }

        public static GameObject MakeFromPrefab(GameObject prefab, string defaultName, GameObject planetGO, Sector sector, PropModule.PositionedPropInfo info, bool alignToBody = false, MVector3 normal = null, MVector3 defaultPosition = null, string defaultParentPath = null)
        {
            var go = prefab.InstantiateInactive();
            go.name = defaultName;
            return MakeFromExisting(go, planetGO, sector, info, alignToBody, normal, defaultPosition, defaultParentPath);
        }
    }
}
