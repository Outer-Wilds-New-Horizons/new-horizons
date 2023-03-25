using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class ProjectionInfo : GeneralPropInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SlideShowType
        {
            [EnumMember(Value = @"slideReel")] SlideReel = 0,

            [EnumMember(Value = @"autoProjector")] AutoProjector = 1,

            [EnumMember(Value = @"visionTorchTarget")] VisionTorchTarget = 2,

            [EnumMember(Value = @"standingVisionTorch")] StandingVisionTorch = 3,

        }

        /// <summary>
        /// The ship log facts revealed after finishing this slide reel.
        /// </summary>
        public string[] reveals;

        /// <summary>
        /// The ship log facts that make the reel play when they are displayed in the computer (by selecting entries or arrows).
        /// You should probably include facts from `reveals` here.
        /// If you only specify a rumor fact, then it would only play in its ship log entry if this has revealed only
        /// rumor facts because an entry with revealed explore facts doesn't display rumor facts.
        /// </summary>
        public string[] playWithShipLogFacts;

        /// <summary>
        /// The list of slides for this object.
        /// </summary>
        public SlideInfo[] slides;

        /// <summary>
        /// The type of object this is.
        /// </summary>
        [DefaultValue("slideReel")] public SlideShowType type = SlideShowType.SlideReel;
    }

}
