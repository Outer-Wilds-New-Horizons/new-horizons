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
        public MVector3[] Rafts;

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

        public class GeyserInfo
        {
            public MVector3 position;
        }
    }
}
