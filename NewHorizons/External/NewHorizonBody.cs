using NewHorizons.External.Configs;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace NewHorizons.External
{
    public class NewHorizonsBody
    {
        public NewHorizonsBody(PlanetConfig config, IModBehaviour mod, string relativePath = null)
        {
            Config = config;
            Mod = mod;
            RelativePath = relativePath;

            // Fall back to file name if name not given
            if (!string.IsNullOrEmpty(relativePath) && string.IsNullOrEmpty(config.name))
            {
                config.name = Path.GetFileNameWithoutExtension(relativePath);
            }

            Migrate();
        }

        public PlanetConfig Config;
        public IModBehaviour Mod;
        public NHCache Cache;
        public string RelativePath;

        public GameObject Object;

        public bool RequiresDLC()
        {
            try
            {
                var detailPaths = Config?.Props?.details?.Select(x => x.path) ?? Array.Empty<string>();
                return Config?.Cloak != null
                    || Config?.Props?.rafts != null
                    || Config?.Props?.slideShows != null
                    || detailPaths.Any(x => x.StartsWith("RingWorld_Body") || x.StartsWith("DreamWorld_Body"));
            }
            catch
            {
                NHLogger.LogWarning($"Failed to check if {Mod.ModHelper.Manifest.Name} requires the DLC");
                return false;
            }

        }

        #region Cache
        public void LoadCache()
        {
            if (RelativePath == null)
            {
                return;
            }

            try
            {
                var pathWithoutExtension = RelativePath.Substring(0, RelativePath.LastIndexOf('.'));
                Cache = new NHCache(Mod, pathWithoutExtension + ".nhcache");
            }
            catch (Exception e)
            {
                NHLogger.LogError("Cache failed to load: " + e.Message);
                Cache = null;
            }
        }

        public void UnloadCache(bool writeBeforeUnload = false)
        {
            if (writeBeforeUnload)
            {
                Cache?.ClearUnaccessed();
                Cache?.WriteToFile();
            }

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
