using NewHorizons.External;
using OWML.Common;
using UnityEngine;

namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(IPlanetConfig config, IModAssets assets)
        {
            Config = config;
            Assets = assets;
        }

        public IPlanetConfig Config;
        public IModAssets Assets;

        public GameObject Object;
    }
}
