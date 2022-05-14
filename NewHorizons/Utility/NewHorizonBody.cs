using NewHorizons.External;
using OWML.Common;
using UnityEngine;
using NewHorizons.External.Configs;

namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(IPlanetConfig config, IModBehaviour mod, string relativePath = null)
        {
            Config = config;
            Mod = mod;
            RelativePath = relativePath;
        }

        public IPlanetConfig Config;
        public IModBehaviour Mod;
        public string RelativePath;

        public GameObject Object;
    }
}
