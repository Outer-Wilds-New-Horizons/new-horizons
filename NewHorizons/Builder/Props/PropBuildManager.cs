using NewHorizons.Builder.Body;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.Builder.Props.EchoesOfTheEye;
using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.Builder.ShipLog;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class PropBuildManager
    {
        public static string InfoToName<T>() where T : BasePropInfo
        {
            var info = typeof(T).Name;
            if (info.EndsWith("Info"))
            {
                return info.Substring(0, info.Length - 4).ToLowercaseNamingConvention();
            }
            else if (info.EndsWith("Module"))
            {
                return info.Substring(0, info.Length - 6).ToLowercaseNamingConvention();
            }
            return info.ToLowercaseNamingConvention();
        }

        public static List<Action> nextPass;

        public static void MakeGeneralProp<T>(GameObject go, T prop, Action<T> builder, Func<T, string> errorMessage = null) where T : BasePropInfo
        {
            if (prop != null)
            {
                try
                {
                    if (DoesParentExist(go, prop))
                    {
                        builder(prop);
                    }
                    else
                    {
                        nextPass.Add(() => MakeGeneralProp<T>(go, prop, builder, errorMessage));
                    }
                }
                catch (Exception ex)
                {
                    var rename = !string.IsNullOrEmpty(prop.rename) ? $" [{prop.rename}]" : string.Empty;
                    var extra = errorMessage != null ? $" [{errorMessage(prop)}]" : string.Empty;
                    NHLogger.LogError($"Couldn't make {InfoToName<T>()}{rename}{extra} for [{go.name}]:\n{ex}");
                }
            }
        }

        public static void MakeGeneralProps<T>(GameObject go, IEnumerable<T> props, Action<T> builder, Func<T, string> errorMessage = null) where T : BasePropInfo
        {
            if (props != null)
            {
                foreach (var prop in props)
                {
                    MakeGeneralProp(go, prop, builder, errorMessage);
                }
            }
        }

        public static void RunMultiPass()
        {
            // Try at least 10 times going through all builders to allow for parents to be built out of order
            int i = 0;
            while (nextPass.Any())
            {
                var count = nextPass.Count;
                var passClone = nextPass.ToList();
                nextPass.Clear();
                passClone.ForEach((x) => x.Invoke());

                if (nextPass.Count >= count || i++ > 10)
                {
                    NHLogger.LogError("Couldn't find any parents. Did you write an invalid parent path?");

                    // Ignore the parent this time so that other error handling stuff can deal with these invalid paths like it used to (backwards compat)
                    _ignoreParent = true;
                    nextPass.ForEach((x) => x.Invoke());
                    _ignoreParent = false;

                    break;
                }
            }
        }

        public static void Make(GameObject go, Sector sector, OWRigidbody planetBody, NewHorizonsBody nhBody)
        {
            PlanetConfig config = nhBody.Config;
            IModBehaviour mod = nhBody.Mod;

            // If a prop has set its parentPath and the parent cannot be found, add it to the next pass and try again later
            nextPass = new List<Action>();

            if (Main.HasDLC) MakeGeneralProps(go, config.Props.grappleTotems, (totem) => GrappleTotemBuilder.Make(go, sector, totem, mod));
            if (Main.HasDLC) MakeGeneralProps(go, config.Props.dreamCampfires, (campfire) => DreamCampfireBuilder.Make(go, sector, campfire, mod), (campfire) => campfire.id);
            if (Main.HasDLC) MakeGeneralProps(go, config.Props.dreamArrivalPoints, (point) => DreamArrivalPointBuilder.Make(go, sector, point, mod), (point) => point.id);
            MakeGeneralProps(go, config.Props.gravityCannons, (cannon) => GravityCannonBuilder.Make(go, sector, cannon, mod), (cannon) => cannon.shuttleID);
            MakeGeneralProps(go, config.Props.shuttles, (shuttle) => ShuttleBuilder.Make(go, sector, mod, shuttle), (shuttle) => shuttle.id);
            MakeGeneralProps(go, config.Props.details, (detail) => DetailBuilder.Make(go, sector, mod, detail), (detail) => detail.path);
            MakeGeneralProps(go, config.Props.geysers, (geyser) => GeyserBuilder.Make(go, sector, geyser));
            if (Main.HasDLC) MakeGeneralProps(go, config.Props.rafts, (raft) => RaftBuilder.Make(go, sector, raft, planetBody));
            MakeGeneralProps(go, config.Props.tornados, (tornado) => TornadoBuilder.Make(go, sector, tornado, config.Atmosphere?.clouds != null));
            MakeGeneralProps(go, config.Props.volcanoes, (volcano) => VolcanoBuilder.Make(go, sector, volcano));
            MakeGeneralProps(go, config.Props.dialogue, (dialogueInfo) =>
            {
                var (dialogue, trigger) = DialogueBuilder.Make(go, sector, dialogueInfo, mod);
                if (dialogue == null)
                {
                    NHLogger.LogVerbose($"[DIALOGUE] Failed to create dialogue [{dialogueInfo.xmlFile}]");
                }
            }, (dialogueInfo) => dialogueInfo.xmlFile);
            MakeGeneralProps(go, config.Props.entryLocation, (entryLocationInfo) => EntryLocationBuilder.Make(go, sector, entryLocationInfo, mod), (entryLocationInfo) => entryLocationInfo.id);
            // Backwards compatibility 
#pragma warning disable 612, 618
            MakeGeneralProps(go, config.Props.nomaiText, (nomaiTextInfo) => NomaiTextBuilder.Make(go, sector, nomaiTextInfo, mod), (nomaiTextInfo) => nomaiTextInfo.xmlFile);
#pragma warning restore 612, 618
            MakeGeneralProps(go, config.Props.translatorText, (nomaiTextInfo) => TranslatorTextBuilder.Make(go, sector, nomaiTextInfo, nhBody), (nomaiTextInfo) => nomaiTextInfo.xmlFile);
            if (Main.HasDLC) MakeGeneralProps(go, config.Props.slideShows, (slideReelInfo) => ProjectionBuilder.Make(go, sector, slideReelInfo, mod), (slideReelInfo) => slideReelInfo.type.ToString().ToCamelCase());
            MakeGeneralProps(go, config.Props.singularities, (singularity) => SingularityBuilder.Make(go, sector, go.GetComponent<OWRigidbody>(), config, singularity), (singularity) => (string.IsNullOrEmpty(singularity.uniqueID) ? config.name : singularity.uniqueID));
            MakeGeneralProps(go, config.Props.signals, (signal) => SignalBuilder.Make(go, sector, signal, mod), (signal) => signal.name);
            MakeGeneralProps(go, config.Props.warpReceivers, (warpReceiver) => WarpPadBuilder.Make(go, sector, mod, warpReceiver), (warpReceiver) => warpReceiver.frequency);
            MakeGeneralProps(go, config.Props.warpTransmitters, (warpTransmitter) => WarpPadBuilder.Make(go, sector, mod, warpTransmitter), (warpTransmitter) => warpTransmitter.frequency);
            MakeGeneralProps(go, config.Props.audioSources, (audioSource) => AudioSourceBuilder.Make(go, sector, audioSource, mod), (audioSource) => audioSource.audio);
            RemoteBuilder.MakeGeneralProps(go, sector, config.Props.remotes, nhBody);

            RunMultiPass();

            /*
             * 
             * Builders below don't inherit the same base class so if they have complicated parentPaths they might just break
             * If a prop above sets one of these as its parent path it will break (but that was always the case)
             *
             */

            if (config.Props.scatter != null)
            {
                try
                {
                    ScatterBuilder.Make(go, sector, config, mod);
                }
                catch (Exception ex)
                {
                    NHLogger.LogError($"Couldn't make planet scatter for [{go.name}]:\n{ex}");
                }
            }

            if (config.Props.quantumGroups != null)
            {
                Dictionary<string, List<GameObject>> propsByGroup = new Dictionary<string, List<GameObject>>();
                foreach (var detail in config.Props.details)
                {
                    if (detail.quantumGroupID != null)
                    {
                        if (!propsByGroup.ContainsKey(detail.quantumGroupID)) propsByGroup[detail.quantumGroupID] = new List<GameObject>();
                        propsByGroup[detail.quantumGroupID].Add(DetailBuilder.GetSpawnedGameObjectByDetailInfo(detail));
                    }
                }

                foreach (var quantumGroup in config.Props.quantumGroups)
                {
                    if (!propsByGroup.ContainsKey(quantumGroup.id)) continue;
                    var propsInGroup = propsByGroup[quantumGroup.id];

                    try
                    {
                        QuantumBuilder.Make(go, sector, config, mod, quantumGroup, propsInGroup.ToArray());
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make quantum group \"{quantumGroup.id}\" for [{go.name}]:\n{ex}");
                    }
                }
            }
        }

        private static bool _ignoreParent;

        private static bool DoesParentExist(GameObject go, BasePropInfo prop)
        {
            if (_ignoreParent)
            {
                return true;
            }
            else if (string.IsNullOrEmpty(prop.parentPath))
            {
                return true;
            }
            else
            {
                return go.transform.Find(prop.parentPath) != null;
            }
        }
    }
}
