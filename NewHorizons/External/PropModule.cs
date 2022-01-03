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
            public string path;
            public int count;
            public MVector3 offset;
            public MVector3 rotation;
        }

        public class DetailInfo
        {
            public string path;
            public string objFilePath;
            public string mtlFilePath;
            public string assetBundle;
            public MVector3 position;
            public MVector3 rotation;
            public float scale;
            public bool alignToNormal;
        }
    }
}
