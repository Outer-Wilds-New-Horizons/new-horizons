using System;
using System.ComponentModel;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class ShipLogModule
    {
        [Obsolete("curiosities is deprecated, please use curiosities in the star system config instead")]
        public CuriosityColorInfo[] curiosities;

        [Obsolete("entryPositions is deprecated, please use entryPositions in the star system config instead")]
        public EntryPositionInfo[] entryPositions;

        [Obsolete("initialReveal is deprecated, please use initialReveal in the star system config instead")]
        public string[] initialReveal;

        /// <summary>
        /// Describe what this planet looks and like in map mode
        /// </summary>
        public MapModeInfo mapMode;

        /// <summary>
        /// A path to the folder where entry sprites are stored.
        /// </summary>
        public string spriteFolder;

        /// <summary>
        /// The relative path to the xml file to load ship log entries from.
        /// </summary>
        public string xmlFile;

        [JsonObject]
        public class MapModeInfo
        {
            /// <summary>
            /// Place non-selectable objects in map mode (like sand funnels).
            /// </summary>
            public ShipLogDetailInfo[] details;

            /// <summary>
            /// Hide the planet completely if unexplored instead of showing an outline.
            /// </summary>
            public bool invisibleWhenHidden;

            /// <summary>
            /// Specify where this planet is in terms of navigation.
            /// </summary>
            public MVector2 manualNavigationPosition;

            /// <summary>
            /// Manually place this planet at the specified position.
            /// </summary>
            public MVector2 manualPosition;

            /// <summary>
            /// Extra distance to apply to this object in map mode.
            /// </summary>
            public float offset;

            /// <summary>
            /// The path to the sprite (.png) to show when the planet is unexplored in map mode.
            /// </summary>
            public string outlineSprite;

            /// <summary>
            /// Completely remove this planet (and it's children) from map mode.
            /// </summary>
            public bool remove;

            /// <summary>
            /// The path to the sprite (.png) to show when the planet is revealed in map mode.
            /// </summary>
            public string revealedSprite;

            /// <summary>
            /// Scale to apply to the planet in map mode.
            /// </summary>
            [DefaultValue(1f)] public float scale = 1f;
        }

        [JsonObject]
        public class ShipLogDetailInfo
        {
            /// <summary>
            /// Whether to completely hide this detail when the parent AstroBody is unexplored.
            /// </summary>
            public bool invisibleWhenHidden;

            /// <summary>
            /// The sprite (.png) to show when the parent AstroBody is rumored/unexplored.
            /// </summary>
            public string outlineSprite;

            /// <summary>
            /// The position (relative to the parent) to place the detail.
            /// </summary>
            public MVector2 position;

            /// <summary>
            /// The sprite (.png) to show when the parent AstroBody is revealed.
            /// </summary>
            public string revealedSprite;

            /// <summary>
            /// The angle in degrees to rotate the detail.
            /// </summary>
            public float rotation;

            /// <summary>
            /// The amount to scale the x and y-axis of the detail by.
            /// </summary>
            public MVector2 scale;
        }

        [JsonObject]
        public class CuriosityColorInfo
        {
            /// <summary>
            /// The color to apply to entries with this curiosity.
            /// </summary>
            public MColor color;

            /// <summary>
            /// The color to apply to highlighted entries with this curiosity.
            /// </summary>
            public MColor highlightColor;

            /// <summary>
            /// The ID of the curiosity to apply the color to.
            /// </summary>
            public string id;
        }

        [JsonObject]
        public class EntryPositionInfo
        {
            /// <summary>
            /// The name of the entry to apply the position to.
            /// </summary>
            public string id;

            /// <summary>
            /// Position of the entry
            /// </summary>
            public MVector2 position;
        }
    }
}