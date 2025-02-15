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

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SlideReelType
        {
            [EnumMember(Value = @"sixSlides")] SixSlides = 6,

            [EnumMember(Value = @"sevenSlides")] SevenSlides = 7,

            [EnumMember(Value = @"eightSlides")] EightSlides = 8,

            [EnumMember(Value = @"whole")] Whole = 9,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum SlideReelCondition
        {
            [EnumMember(Value = @"antique")] Antique = 0,

            [EnumMember(Value = @"pristine")] Pristine = 1,

            [EnumMember(Value = @"rusted")] Rusted = 2,
        }

        /// <summary>
        /// The ship log facts revealed after finishing this slide reel.
        /// </summary>
        public string[] reveals;

        /// <summary>
        /// The dialogue conditions to set after finishing this slide reel.
        /// </summary>
        public string[] conditionsToSet;

        /// <summary>
        /// The persistent conditions to set after finishing this slide reel.
        /// </summary>
        public string[] persistentConditionsToSet;

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

        /// <summary>
        /// Exclusive to the slide reel type. Model/mesh of the reel. Each model has a different number of slides on it. Whole has 7 slides but a full ring like 8.
        /// </summary>
        [DefaultValue("sevenSlides")] public SlideReelType reelModel = SlideReelType.SevenSlides;

        /// <summary>
        /// Exclusive to the slide reel type. Condition/material of the reel. Antique is the Stranger, Pristine is the Dreamworld, Rusted is a burned reel.
        /// </summary>
        [DefaultValue("antique")] public SlideReelCondition reelCondition = SlideReelCondition.Antique;

        /// <summary>
        /// Set which slides appear on the slide reel model. Leave empty to default to the first few slides.
        /// Takes a list of indices, i.e., to show the first 5 slides in reverse you would put [4, 3, 2, 1, 0].
        /// Index starts at 0.
        /// </summary>
        public int[] displaySlides;
    }
}
