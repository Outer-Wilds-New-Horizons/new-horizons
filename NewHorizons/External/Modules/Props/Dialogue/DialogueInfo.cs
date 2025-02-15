using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Dialogue
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
        /// CharacterAnimController, TravelerController, TravelerEyeController (eye of the universe), FacePlayerWhenTalking, 
        /// HearthianRecorderEffects or SolanumAnimController.
        /// 
        /// If it's a Recorder this will also delete the existing dialogue already attached to that prop.
        /// 
        /// If none of those components are present it will add a FacePlayerWhenTalking component.
        /// 
        /// `pathToAnimController` also makes the dialogue into a child of the anim controller. This can be used with `isRelativeToParent`
        /// to position the dialogue on relative to the speaker. If you also provide `parentPath`, that will instead override which object 
        /// is the parent, but the anim controller will otherwise function as expected.
        /// </summary>
        public string pathToAnimController;

        /// <summary>
        /// If this dialogue is adding to existing character dialogue, put a path to the game object with the dialogue on it here
        /// </summary>
        public string pathToExistingDialogue;

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
        /// The point that the camera looks at when dialogue advances.
        /// </summary>
        public AttentionPointInfo attentionPoint;

        /// <summary>
        /// Additional points that the camera looks at when dialogue advances through specific dialogue nodes and pages.
        /// </summary>
        public SwappedAttentionPointInfo[] swappedAttentionPoints;

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
    }
}
