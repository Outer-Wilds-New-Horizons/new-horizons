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
        public MVector3[] Rafts;

        public class ScatterInfo
        {
            public string path;
            public int count;
            public MVector3 offset;
            public MVector3 rotation;
        }
    }
}
