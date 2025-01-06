using NewHorizons.External.Configs;
using OWML.Common;
using System.Linq;

namespace NewHorizons.External
{
    public class NewHorizonsSystem
    {
        public string UniqueID;
        public string RelativePath;
        public StarSystemConfig Config;
        public IModBehaviour Mod;
        public bool HasShipSpawn;

        public NewHorizonsSystem(string uniqueID, StarSystemConfig config, string relativePath, IModBehaviour mod)
        {
            UniqueID = uniqueID;
            Config = config;
            RelativePath = relativePath;
            Mod = mod;

            // Backwards compat
            if (new string[] { "2walker2.OogaBooga", "2walker2.EndingIfYouWarpHereYouAreMean", "FeldsparSystem" }.Contains(uniqueID))
            {
                config.canExitViaWarpDrive = false;
            }
        }
    }
}
