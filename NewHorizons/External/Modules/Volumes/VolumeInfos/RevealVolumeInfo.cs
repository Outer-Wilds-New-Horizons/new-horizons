using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Volumes.VolumeInfos
{
    [JsonObject]
    public class RevealVolumeInfo : VolumeInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum RevealVolumeType
        {
            [EnumMember(Value = @"enter")] Enter = 0,

            [EnumMember(Value = @"observe")] Observe = 1,

            [EnumMember(Value = @"snapshot")] Snapshot = 2
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EnterType
        {
            [EnumMember(Value = @"both")] Both = 0,

            [EnumMember(Value = @"player")] Player = 1,

            [EnumMember(Value = @"probe")] Probe = 2
        }

        /// <summary>
        /// The max view angle (in degrees) the player can see the volume with to unlock the fact (`observe` only)
        /// </summary>
        [DefaultValue(180f)]
        public float maxAngle = 180f; // Observe Only

        /// <summary>
        /// The max distance the user can be away from the volume to reveal the fact (`snapshot` and `observe` only)
        /// </summary>
        [DefaultValue(-1f)]
        public float maxDistance = -1f; // Snapshot & Observe Only

        /// <summary>
        /// What needs to be done to the volume to unlock the facts
        /// </summary>
        [DefaultValue("enter")] public RevealVolumeType revealOn = RevealVolumeType.Enter;

        /// <summary>
        /// What can enter the volume to unlock the facts (`enter` only)
        /// </summary>
        [DefaultValue("both")] public EnterType revealFor = EnterType.Both;

        /// <summary>
        /// A list of facts to reveal
        /// </summary>
        public string[] reveals;

        /// <summary>
        /// An achievement to unlock. Optional.
        /// </summary>
        public string achievementID;
    }
}
