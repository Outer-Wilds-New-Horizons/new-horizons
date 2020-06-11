using Marshmallow.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.Utility
{
    public class MarshmallowBody
    {
        public MarshmallowBody(IPlanetConfig config)
        {
            Config = config;
        }

        public IPlanetConfig Config;

        public GameObject Object;
    }
}
