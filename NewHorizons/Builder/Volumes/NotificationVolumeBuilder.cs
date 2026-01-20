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
            var notificationVolume = VolumeBuilder.Make<NHNotificationVolume>(planetGO, ref sector, info);

            // Preserving name for backwards compatibility
            notificationVolume.gameObject.name = string.IsNullOrEmpty(info.rename) ? "NotificationVolume" : info.rename;

            notificationVolume.SetTarget(info.target);
            if (info.entryNotification != null) notificationVolume.SetEntryNotification(info.entryNotification.displayMessage, info.entryNotification.duration);
            if (info.exitNotification != null) notificationVolume.SetExitNotification(info.exitNotification.displayMessage, info.exitNotification.duration);

            notificationVolume.gameObject.SetActive(true);

            return notificationVolume;
        }
    }
}
