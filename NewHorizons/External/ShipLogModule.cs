using NewHorizons.Utility;

namespace NewHorizons.External
{
    public class ShipLogModule : Module
    {
        public string xmlFile;
        public string spriteFolder;
        public string[] initialReveal;
        public MapModeInfo mapMode;
        public CuriosityColorInfo[] curiosities;
        public EntryPositionInfo[] positions;

        public class MapModeInfo
        {
            public string revealedSprite;
            public string outlineSprite;
            public float scale = 1f;
            public bool invisibleWhenHidden;
            public float offset = 0f;
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