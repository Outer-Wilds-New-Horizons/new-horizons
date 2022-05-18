using NewHorizons.Utility;
namespace NewHorizons.External.Modules
{
    public class ShipLogModule 
    {
        public string xmlFile;
        public string spriteFolder;
        public string[] initialReveal;
        public MapModeInfo mapMode = new MapModeInfo();
        public CuriosityColorInfo[] curiosities;
        public EntryPositionInfo[] entryPositions;

        public class MapModeInfo
        {
            public string revealedSprite;
            public string outlineSprite;
            public float scale = 1f;
            public bool invisibleWhenHidden;
            public float offset = 0f;
            public MVector2 manualPosition;
            public MVector2 manualNavigationPosition;
            public bool remove = false;
            public ShipLogDetailInfo[] details;
        }

        public class ShipLogDetailInfo
        {
            public string revealedSprite;
            public string outlineSprite;
            public float rotation = 0f;
            public bool invisibleWhenHidden;
            public MVector2 position;
            public MVector2 scale;
        }

        public class CuriosityColorInfo
        {
            public string id;
            public MColor color;
            public MColor highlightColor;
        }

        public class EntryPositionInfo
        {
            public string id;
            public MVector2 position;
        }
    }
}