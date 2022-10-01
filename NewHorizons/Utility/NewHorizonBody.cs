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
        }

        public PlanetConfig Config;
        public IModBehaviour Mod;
        public string RelativePath;

        public GameObject Object;

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
        #region Migration
    }
}
