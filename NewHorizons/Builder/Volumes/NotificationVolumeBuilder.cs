using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.Utility.OuterWilds;
using OWML.Common;
using UnityEngine;
using NHNotificationVolume = NewHorizons.Components.Volumes.NotificationVolume;

namespace NewHorizons.Builder.Volumes
{
    public static class NotificationVolumeBuilder
    {
        public static NHNotificationVolume Make(GameObject planetGO, Sector sector, NotificationVolumeInfo info, IModBehaviour mod)
        {
            var go = GeneralPropBuilder.MakeNew("NotificationVolume", planetGO, sector, info);
            go.layer = Layer.BasicEffectVolume;

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
