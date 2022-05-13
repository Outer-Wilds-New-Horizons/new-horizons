using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Logger = NewHorizons.Utility.Logger;
using System.Reflection;
using NewHorizons.Builder.General;
using NewHorizons.Utility;
using OWML.Common;
using NewHorizons.Builder.ShipLog;
using NewHorizons.External.Configs;
using System.IO;

namespace NewHorizons.Builder.Props
{
    public static class PropBuildManager
    {
        public static void Make(GameObject go, Sector sector, OWRigidbody planetBody, IPlanetConfig config, IModBehaviour mod, string uniqueModName)
        {
            if (config.Props.Scatter != null)
            {
                try
                {
                    ScatterBuilder.Make(go, sector, config, mod, uniqueModName);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Couldn't make planet scatter for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                }
            }
            if (config.Props.Details != null)
            {
                foreach (var detail in config.Props.Details)
                {
                    try
                    {
                        DetailBuilder.Make(go, sector, config, mod, uniqueModName, detail);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make planet detail [{detail.path}] for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            if (config.Props.Geysers != null)
            {
                foreach (var geyserInfo in config.Props.Geysers)
                {
                    try
                    {
                        GeyserBuilder.Make(go, sector, geyserInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make geyser for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            if (Main.HasDLC && config.Props.Rafts != null)
            {
                foreach (var raftInfo in config.Props.Rafts)
                {
                    try
                    {
                        RaftBuilder.Make(go, sector, raftInfo, planetBody);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make raft for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            if (config.Props.Tornados != null)
            {
                foreach (var tornadoInfo in config.Props.Tornados)
                {
                    try
                    {
                        TornadoBuilder.Make(go, sector, tornadoInfo, config.Atmosphere?.Cloud != null);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make tornado for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            if (config.Props.Volcanoes != null)
            {
                foreach (var volcanoInfo in config.Props.Volcanoes)
                {
                    try
                    {
                        VolcanoBuilder.Make(go, sector, volcanoInfo);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make volcano for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            // Reminder that dialogue has to be built after props if they're going to be using CharacterAnimController stuff
            if (config.Props.Dialogue != null)
            {
                foreach (var dialogueInfo in config.Props.Dialogue)
                {
                    try
                    {
                        DialogueBuilder.Make(go, sector, dialogueInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make dialogue [{dialogueInfo.xmlFile}] for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            if (config.Props.Reveal != null)
            {
                foreach (var revealInfo in config.Props.Reveal)
                {
                    try
                    {
                        RevealBuilder.Make(go, sector, revealInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make reveal location [{revealInfo.reveals}] for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            if (config.Props.EntryLocation != null)
            {
                foreach (var entryLocationInfo in config.Props.EntryLocation)
                {
                    try
                    {
                        EntryLocationBuilder.Make(go, sector, entryLocationInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make entry location [{entryLocationInfo.id}] for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
            if (config.Props.NomaiText != null)
            {
                foreach (var nomaiTextInfo in config.Props.NomaiText)
                {
                    try
                    {
                        NomaiTextBuilder.Make(go, sector, nomaiTextInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make text [{nomaiTextInfo.xmlFile}] for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }

                }
            }
            if (Main.HasDLC && config.Props.SlideShows != null)
            {
                foreach (var slideReelInfo in config.Props.SlideShows)
                {
                    try
                    {
                        ProjectionBuilder.Make(go, sector, slideReelInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Couldn't make slide reel for [{go.name}] : {ex.Message}, {ex.StackTrace}");
                    }
                }
            }
        }
    }
}
