using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.VariableSize
{
    public class FunnelModule : VariableSizeModule
    {
        public string Target { get; set; }
        public string Type { get; set; } = "Sand";
        public MColor32 Tint { get; set; }
    }
}
