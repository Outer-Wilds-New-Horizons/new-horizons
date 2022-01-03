using NewHorizons.External;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(IPlanetConfig config, IModAssets assets, string modUniqueName)
        {
            Config = config;
            Assets = assets;
            ModUniqueName = modUniqueName;
        }

        public IPlanetConfig Config;
        public IModAssets Assets;
        public string ModUniqueName;

        public GameObject Object;
    }
}
