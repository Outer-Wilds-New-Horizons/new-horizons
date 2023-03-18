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
        public static GameObject MakeFromExisting(GameObject go, Transform parent, PropModule.GeneralPointPropInfo info, bool alignToBody = false, MVector3 normal = null, MVector3 defaultPosition = null, string defaultParentPath = null)
        {
            if (info == null) return go;

            if (info is PropModule.GeneralSolarSystemPropInfo solarSystemInfo && !string.IsNullOrEmpty(solarSystemInfo.parentBody))
            {
                var targetPlanet = AstroObjectLocator.GetAstroObject(solarSystemInfo.parentBody);
                if (targetPlanet != null)
                {
                    parent = targetPlanet.transform;
                } else
                {
                    Logger.LogError($"Cannot find parent body named {solarSystemInfo.parentBody}");
                }
            }

            if (!string.IsNullOrEmpty(info.rename))
            {
                go.name = info.rename;
            }

            go.transform.parent = parent;

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
            if (info is PropModule.GeneralPropInfo rotInfo)
            {
                rot = rotInfo.rotation != null ? Quaternion.Euler(rotInfo.rotation) : Quaternion.identity;
            }
            if (info.isRelativeToParent)
            {
                go.transform.localPosition = pos;
                go.transform.localRotation = rot;
            } else if (parent)
            {
                go.transform.position = parent.root.TransformPoint(pos);
                go.transform.rotation = parent.root.TransformRotation(rot);
            } else
            {
                go.transform.position = pos;
                go.transform.rotation = rot;
            }
            if (alignToBody)
            {
                var up = (go.transform.position - parent.position).normalized;
                if (normal != null) up = parent.TransformDirection(normal);
                go.transform.rotation = Quaternion.FromToRotation(Vector3.up, up);
                go.transform.rotation *= rot;
            }
            return go;
        }

        public static GameObject MakeNew(string defaultName, Transform parent, PropModule.GeneralPointPropInfo info, bool alignToBody = false, MVector3 normal = null, MVector3 defaultPosition = null, string defaultParentPath = null)
        {
            var go = new GameObject(defaultName);
            go.SetActive(false);
            return MakeFromExisting(go, parent, info, alignToBody, normal, defaultPosition, defaultParentPath);
        }

        public static GameObject MakeFromPrefab(GameObject prefab, string defaultName, Transform parent, PropModule.GeneralPointPropInfo info, bool alignToBody = false, MVector3 normal = null, MVector3 defaultPosition = null, string defaultParentPath = null)
        {
            var go = prefab.InstantiateInactive();
            go.name = defaultName;
            return MakeFromExisting(go, parent, info, alignToBody, normal, defaultPosition, defaultParentPath);
        }
    }
}
