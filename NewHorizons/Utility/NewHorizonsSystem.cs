using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using OWML.Common;

namespace NewHorizons.Utility
{
    public class NewHorizonsSystem
    {
        public string UniqueID;
        public string RelativePath;
        public SpawnModule Spawn = null;
        public SpawnPoint SpawnPoint = null;
        public StarSystemConfig Config;
        public IModBehaviour Mod;

        public NewHorizonsSystem(string uniqueID, StarSystemConfig config, string relativePath, IModBehaviour mod)
        {
            UniqueID = uniqueID;
            Config = config;
            RelativePath = relativePath;
            Mod = mod;
        }
    }
}
