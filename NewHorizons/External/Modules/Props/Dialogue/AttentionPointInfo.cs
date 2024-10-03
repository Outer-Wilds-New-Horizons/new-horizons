using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Dialogue
{
    [JsonObject]
    public class AttentionPointInfo : GeneralPointPropInfo
    {
        /// <summary>
        /// An additional offset to apply to apply when the camera looks at this attention point.
        /// </summary>
        public MVector3 offset;
    }

    [JsonObject]
    public class SwappedAttentionPointInfo : AttentionPointInfo
    {
        /// <summary>
        /// The name of the dialogue node to activate this attention point for. If null or blank, activates for every node.
        /// </summary>
        public string dialogueNode;
        /// <summary>
        /// The index of the page in the current dialogue node to activate this attention point for, if the node has multiple pages.
        /// </summary>
        public int dialoguePage;
        /// <summary>
        /// The easing factor which determines how 'snappy' the camera is when looking at the attention point.
        /// </summary>
        [DefaultValue(1)] public float lookEasing = 1f;
    }
}
