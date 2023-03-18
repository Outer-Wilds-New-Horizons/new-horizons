
using NewHorizons.Utility;
using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public class DialogueInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// Prevents the dialogue from being created after a specific persistent condition is set. Useful for remote dialogue
        /// triggers that you want to have happen only once.
        /// </summary>
        public string blockAfterPersistentCondition;

        /// <summary>
        /// If a pathToAnimController is supplied, if you are within this distance the character will look at you. If it is set
        /// to 0, they will only look at you when spoken to.
        /// </summary>
        public float lookAtRadius;

        /// <summary>
        /// If this dialogue is meant for a character, this is the relative path from the planet to that character's
        /// CharacterAnimController, TravelerController, TravelerEyeController (eye of the universe), FacePlayerWhenTalking, or SolanumAnimController.
        /// 
        /// If none of those components are present it will add a FacePlayerWhenTalking component.
        /// </summary>
        public string pathToAnimController;

        /// <summary>
        /// Radius of the spherical collision volume where you get the "talk to" prompt when looking at. If you use a
        /// remoteTrigger, you can set this to 0 to make the dialogue only trigger remotely.
        /// </summary>
        public float radius = 1f;

        /// <summary>
        /// Distance from radius the prompt appears
        /// </summary>
        [DefaultValue(2f)] public float range = 2f;

        /// <summary>
        /// Allows you to trigger dialogue from a distance when you walk into an area.
        /// </summary>
        public RemoteTriggerInfo remoteTrigger;

        [Obsolete("remoteTriggerPosition is deprecated. Use remoteTrigger.position instead")] public MVector3 remoteTriggerPosition;
        [Obsolete("remoteTriggerRadius is deprecated. Use remoteTrigger.radius instead")] public float remoteTriggerRadius;
        [Obsolete("remoteTriggerPrereqCondition is deprecated. Use remoteTrigger.prereqCondition instead")] public string remoteTriggerPrereqCondition;

        /// <summary>
        /// Relative path to the xml file defining the dialogue.
        /// </summary>
        public string xmlFile;

        /// <summary>
        /// What type of flashlight toggle to do when dialogue is interacted with
        /// </summary>
        [DefaultValue("none")] public FlashlightToggle flashlightToggle = FlashlightToggle.None;

        [JsonConverter(typeof(StringEnumConverter))]
        public enum FlashlightToggle
        {
            [EnumMember(Value = @"none")] None = -1,
            [EnumMember(Value = @"turnOff")] TurnOff = 0,
            [EnumMember(Value = @"turnOffThenOn")] TurnOffThenOn = 1,
        }

        [JsonObject]
        public class RemoteTriggerInfo : GeneralPointPropInfo
        {
            /// <summary>
            /// The radius of the remote trigger volume.
            /// </summary>
            public float radius;
            /// <summary>
            /// This condition must be met for the remote trigger volume to trigger.
            /// </summary>
            public string prereqCondition;
        }
    }
}
