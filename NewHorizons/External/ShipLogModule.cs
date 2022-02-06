using NewHorizons.Utility;

namespace NewHorizons.External
{
    public class ShipLogModule : Module
    {
        public string xmlFile;
        public string spriteFolder;
        public MapMode mapMode;
        public CuriosityColor[] curiosities;
        public EntryPosition[] positions;
        public string[] initialReveal;

        public class MapMode
        {
            public string revealedSprite;
            public string outlineSprite;
            public float scale = 1f;
            public bool invisibleWhenHidden;
            public float offset = 0f;
            public bool remove = false;
        }

        public class CuriosityColor
        {
            public string id;
            public MColor color;
            public MColor highlightColor;
        }

        public class EntryPosition
        {
            public string id;
            public MVector2 position;
        }
    }
}