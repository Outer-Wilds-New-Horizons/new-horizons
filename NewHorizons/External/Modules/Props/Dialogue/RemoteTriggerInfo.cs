using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Dialogue
{
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
