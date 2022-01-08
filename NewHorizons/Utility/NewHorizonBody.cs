using NewHorizons.External;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(IPlanetConfig config, IModHelper mod)
        {
            Config = config;
            Mod = mod;
        }

        public IPlanetConfig Config;
        public IModHelper Mod;

        public GameObject Object;
    }
}
