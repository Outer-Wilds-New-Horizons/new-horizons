using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using OWML.Common;
namespace NewHorizons.Utility
{
    public class NewHorizonsSystem
    {
        public NewHorizonsSystem(string name, StarSystemConfig config, IModBehaviour mod)
        {
            Name = name;
            Config = config;
            Mod = mod;
        }

        public string Name;
        public SpawnModule Spawn = null;
        public SpawnPoint SpawnPoint = null;
        public StarSystemConfig Config;
        public IModBehaviour Mod;
    }
}
