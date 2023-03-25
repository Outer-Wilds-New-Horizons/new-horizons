using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class NotificationVolumeInfo : VolumeInfo
    {
        /// <summary>
        /// What the notification will show for.
        /// </summary>
        [DefaultValue("all")] public NotificationTarget target = NotificationTarget.All;

        /// <summary>
        /// The notification that will play when you enter this volume.
        /// </summary>
        public NotificationInfo entryNotification;

        /// <summary>
        /// The notification that will play when you exit this volume.
        /// </summary>
        public NotificationInfo exitNotification;


        [JsonObject]
        public class NotificationInfo
        {
            /// <summary>
            /// The message that will be displayed.
            /// </summary>
            public string displayMessage;

            /// <summary>
            /// The duration this notification will be displayed.
            /// </summary>
            [DefaultValue(5f)] public float duration = 5f;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum NotificationTarget
        {
            [EnumMember(Value = @"all")] All = 0,
            [EnumMember(Value = @"ship")] Ship = 1,
            [EnumMember(Value = @"player")] Player = 2,
        }
    }

}
