using NewHorizons.External.Configs;
using OWML.Common;
using UnityEngine;
namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(PlanetConfig config, IModBehaviour mod)
        {
            Config = config;
            Mod = mod;
        }

        public PlanetConfig Config;
        public IModBehaviour Mod;

        public GameObject Object;
    }
}
