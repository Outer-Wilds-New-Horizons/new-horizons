using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using OWML.Common;

namespace NewHorizons.Utility
{
    public class NewHorizonsSystem
    {
        public NewHorizonsSystem(string uniqueID, StarSystemConfig config, IModBehaviour mod)
        {
            UniqueID = uniqueID;
            Config = config;
            Mod = mod;
        }

        public string UniqueID;
        public SpawnModule Spawn = null;
        public SpawnPoint SpawnPoint = null;
        public StarSystemConfig Config;
        public IModBehaviour Mod;
    }
}