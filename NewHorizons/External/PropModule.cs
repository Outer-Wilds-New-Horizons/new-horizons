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

        public class ScatterInfo
        {
            public int count;
            public string path;
            public string assetBundle;
            public MVector3 offset;
            public MVector3 rotation;
            public float scale { get; set; } = 1f;
            public bool generateColliders = false;
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
            public bool generateColliders = false;
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
            public string persistentCondition;
        }
    }
}
