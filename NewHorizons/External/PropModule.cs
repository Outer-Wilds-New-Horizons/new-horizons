using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class PropModule : Module
    {
        public ScatterInfo[] Scatter;
        public DetailInfo[] Details;
        public RaftInfo[] Rafts;
        public GeyserInfo[] Geysers;
        public TornadoInfo[] Tornados;
        public DialogueInfo[] Dialogue;
        public RevealInfo[] Reveal;
        public EntryLocationInfo[] EntryLocation;

        public class ScatterInfo
        {
            public int seed = 0;
            public int count;
            public string path;
            public string assetBundle;
            public MVector3 offset;
            public MVector3 rotation;
            public float scale { get; set; } = 1f;
        }

        public class DetailInfo
        {
            public string path;
            public string objFilePath;
            public string mtlFilePath;
            public string assetBundle;
            public MVector3 position;
            public MVector3 rotation;
            public float scale { get; set; } = 1f;
            public bool alignToNormal;
            public string[] removeChildren;
        }

        public class RaftInfo
        {
            public MVector3 position;
        }

        public class GeyserInfo
        {
            public MVector3 position;
        }

        public class TornadoInfo
        {
            public float elevation;
            public MVector3 position;
            public float height;
            public float width;
            public MColor tint;
        }

        public class DialogueInfo
        {
            public MVector3 position;
            public float radius = 1f;
            public string xmlFile;
            public MVector3 remoteTriggerPosition;
            public string blockAfterPersistentCondition;
        }

        public class RevealInfo
        {
            public string revealOn;
            public string[] reveals;
            public MVector3 position;
            public float radius = 1f;
            public float maxDistance = -1f; // Snapshot & Observe Only
            public float maxAngle = 180f; // Observe Only
        }

        public class EntryLocationInfo
        {
            public string id;
            public bool cloaked;
            public MVector3 position;
        }
    }
}
