using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OWUtilities;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using NHNotificationVolume = NewHorizons.Components.Volumes.NotificationVolume;

namespace NewHorizons.Builder.Volumes
{
    public static class NotificationVolumeBuilder
    {
        public static NHNotificationVolume Make(GameObject planetGO, Sector sector, VolumesModule.NotificationVolumeInfo info, IModBehaviour mod)
        {
            var go = new GameObject("NotificationVolume");
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

            var notificationVolume = go.AddComponent<NHNotificationVolume>();
            notificationVolume.SetTarget(info.target);
            if (info.entryNotification != null) notificationVolume.SetEntryNotification(info.entryNotification.displayMessage, info.entryNotification.duration);
            if (info.exitNotification != null) notificationVolume.SetExitNotification(info.exitNotification.displayMessage, info.exitNotification.duration);

            go.SetActive(true);

            return notificationVolume;
        }
    }
}
