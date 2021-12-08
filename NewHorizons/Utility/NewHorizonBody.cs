using NewHorizons.External;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(IPlanetConfig config)
        {
            Config = config;
        }

        public IPlanetConfig Config;

        public GameObject Object;
    }
}
