using NewHorizons.Utility;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class ShipLogModule 
    {
        /// <summary>
        /// The relative path to the xml file to load ship log entries from.
        /// </summary>
        public string xmlFile;
        
        /// <summary>
        /// A path to the folder where entry sprites are stored.
        /// </summary>
        public string spriteFolder;
        
        /// <summary>
        /// A list of fact IDs to reveal when the game starts.
        /// </summary>
        public string[] initialReveal;
        
        /// <summary>
        /// Describe what this planet looks and like in map mode
        /// </summary>
        public MapModeInfo mapMode = new MapModeInfo();
        
        /// <summary>
        /// List colors of curiosity entries
        /// </summary>
        public CuriosityColorInfo[] curiosities;
        
        /// <summary>
        /// Manually layout entries in detective mode
        /// </summary>
        public EntryPositionInfo[] entryPositions;

        [JsonObject]
        public class MapModeInfo
        {
            /// <summary>
            /// The path to the sprite to show when the planet is revealed in map mode.
            /// </summary>
            public string revealedSprite;
            
            /// <summary>
            /// The path to the sprite to show when the planet is unexplored in map mode.
            /// </summary>
            public string outlineSprite;
            
            /// <summary>
            /// Scale to apply to the planet in map mode.
            /// </summary>
            [DefaultValue(1f)]
            public float scale = 1f;
            
            /// <summary>
            /// Hide the planet completely if unexplored instead of showing an outline.
            /// </summary>
            public bool invisibleWhenHidden;
            
            /// <summary>
            /// Extra distance to apply to this object in map mode.
            /// </summary>
            public float offset;
            
            /// <summary>
            /// Manually place this planet at the specified position.
            /// </summary>
            public MVector2 manualPosition;
            
            /// <summary>
            /// Specify where this planet is in terms of navigation.
            /// </summary>
            public MVector2 manualNavigationPosition;
            
            /// <summary>
            /// Completely remove this planet (and it's children) from map mode.
            /// </summary>
            public bool remove;
            
            /// <summary>
            /// Place non-selectable objects in map mode (like sand funnels).
            /// </summary>
            public ShipLogDetailInfo[] details;
        }

        [JsonObject]
        public class ShipLogDetailInfo
        {
            /// <summary>
            /// The sprite to show when the parent AstroBody is revealed.
            /// </summary>
            public string revealedSprite;
            
            /// <summary>
            /// The sprite to show when the parent AstroBody is rumored/unexplored.
            /// </summary>
            public string outlineSprite;
            
            /// <summary>
            /// The angle in degrees to rotate the detail.
            /// </summary>
            public float rotation;
            
            /// <summary>
            /// Whether to completely hide this detail when the parent AstroBody is unexplored.
            /// </summary>
            public bool invisibleWhenHidden;
            
            /// <summary>
            /// The position (relative to the parent) to place the detail.
            /// </summary>
            public MVector2 position;
            
            /// <summary>
            /// The amount to scale the x and y-axis of the detail by.
            /// </summary>
            public MVector2 scale;
        }

        [JsonObject]
        public class CuriosityColorInfo
        {
            /// <summary>
            /// The ID of the curiosity to apply the color to.
            /// </summary>
            public string id;
            
            /// <summary>
            /// The color to apply to entries with this curiosity.
            /// </summary>
            public MColor color;
            
            /// <summary>
            /// The color to apply to highlighted entries with this curiosity.
            /// </summary>
            public MColor highlightColor;
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