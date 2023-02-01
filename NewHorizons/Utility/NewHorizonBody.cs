using NewHorizons.External.Configs;
using OWML.Common;
using System.Linq;
using UnityEngine;
namespace NewHorizons.Utility
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(PlanetConfig config, IModBehaviour mod, string relativePath = null)
        {
            Config = config;
            Mod = mod;
            RelativePath = relativePath;

            Logger.LogWarning("CACHE DEBUG: NHBODY CONSTRUCTOR: " + config.name + " - *" + relativePath+"*");

            Migrate();
        }

        public PlanetConfig Config;
        public IModBehaviour Mod;
        public Cache Cache;
        public string RelativePath;

        public GameObject Object;

        #region Cache
        public void LoadCache()
        {
            if (RelativePath == null) 
            {
                Logger.LogWarning("Cannot load cache! RelativePath is null!");
                return;
            }

            var pathWithoutExtension = RelativePath.Substring(0, RelativePath.LastIndexOf('.'));
            Cache = new Cache(pathWithoutExtension+".nhcache");
        }

        public void UnloadCache(bool writeBeforeUnload=false)
        {
            if (writeBeforeUnload) Cache?.WriteToFile();

            Cache = null; // garbage collection will take care of it
        }
        #endregion Cache

        #region Migration
        private static readonly string[] _keepLoadedModsList = new string[]
        {
            "CreativeNameTxt.theirhomeworld",
            "Roggsy.enterthewarioverse",
            "Jammer.jammerlore",
            "ErroneousCreationist.solarneighbourhood",
            "ErroneousCreationist.incursionfinaldawn"
        };

        private void Migrate()
        {
            // Some old mods get really broken by this change in 1.6.1
            if (_keepLoadedModsList.Contains(Mod.ModHelper.Manifest.UniqueName))
            {
                if (Config?.Props?.details != null)
                {
                    foreach (var detail in Config.Props.details)
                    {
                        detail.keepLoaded = true;
                    }
                }
            }
        }

        #endregion
    }
}
