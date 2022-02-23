using NewHorizons.External;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(IPlanetConfig config, IModBehaviour mod)
        {
            Config = config;
            Mod = mod;
        }

        public IPlanetConfig Config;
        public IModBehaviour Mod;

        public GameObject Object;
    }
}
