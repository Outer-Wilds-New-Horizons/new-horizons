using NewHorizons.Builder.Body;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.Builder.ShipLog;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
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
        public static void Make(GameObject go, Sector sector, OWRigidbody planetBody, NewHorizonsBody nhBody)
        {
            PlanetConfig config = nhBody.Config;
            IModBehaviour mod = nhBody.Mod;

            // If a prop has set its parentPath and the parent cannot be found, add it to the next pass and try again later
            var nextPass = new List<Action>();

            void MakeGeneralProp<T>(GameObject go, T prop, Action<T> builder) where T : GeneralPointPropInfo
            {
                try
                {
                    if (DoesParentExist(go, prop))
                    {
                        builder(prop);
                    }
                    else
                    {
                        nextPass.Add(() => MakeGeneralProp<T>(go, prop, builder));
                    }
                }
                catch (Exception ex)
                {
                    NHLogger.LogError($"Couldn't make {typeof(T).Name} [{prop.rename}] for [{go.name}]:\n{ex}");
                }
            }

            void MakeGeneralProps<T>(GameObject go, IEnumerable<T> props, Action<T> builder) where T : GeneralPointPropInfo
            {
                if (props != null)
                {
                    foreach (var prop in props)
                    {
                        MakeGeneralProp(go, prop, builder);
                    }
                }
            }

            MakeGeneralProps(go, config.Props.gravityCannons, (cannon) => GravityCannonBuilder.Make(go, sector, cannon, mod));
            MakeGeneralProps(go, config.Props.shuttles, (shuttle) => ShuttleBuilder.Make(go, sector, nhBody.Mod, shuttle));
            MakeGeneralProps(go, config.Props.details, (detail) => DetailBuilder.Make(go, sector, mod, detail));
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
            });
            MakeGeneralProps(go, config.Props.entryLocation, (entryLocationInfo) => EntryLocationBuilder.Make(go, sector, entryLocationInfo, mod));
            // Backwards compatibility 
#pragma warning disable 612, 618
            MakeGeneralProps(go, config.Props.nomaiText, (nomaiTextInfo) => NomaiTextBuilder.Make(go, sector, nomaiTextInfo, nhBody.Mod));
#pragma warning restore 612, 618
            MakeGeneralProps(go, config.Props.translatorText, (nomaiTextInfo) => TranslatorTextBuilder.Make(go, sector, nomaiTextInfo, nhBody));
            if (Main.HasDLC) MakeGeneralProps(go, config.Props.slideShows, (slideReelInfo) => ProjectionBuilder.Make(go, sector, slideReelInfo, mod));
            MakeGeneralProps(go, config.Props.singularities, (singularity) => SingularityBuilder.Make(go, sector, go.GetComponent<OWRigidbody>(), config, singularity));
            MakeGeneralProps(go, config.Props.signals, (signal) => SignalBuilder.Make(go, sector, signal, mod));
            MakeGeneralProps(go, config.Props.warpReceivers, (warpReceiver) => WarpPadBuilder.Make(go, sector, nhBody.Mod, warpReceiver));
            MakeGeneralProps(go, config.Props.warpTransmitters, (warpTransmitter) => WarpPadBuilder.Make(go, sector, nhBody.Mod, warpTransmitter));
            MakeGeneralProps(go, config.Props.audioSources, (audioSource) => AudioSourceBuilder.Make(go, sector, audioSource, mod));

            // Try at least 10 times going through all builders to allow for parents to be built out of order
            int i = 0;
            while(nextPass.Any())
            {
                var count = nextPass.Count;
                var passClone = nextPass.ToList();
                nextPass.Clear();
                passClone.ForEach((x) => x.Invoke());

                if (nextPass.Count >= count)
                {
                    NHLogger.LogError("Couldn't find any parents");
                    break;
                }

                if (i++ > 10)
                {
                    NHLogger.LogError("Went through more than 10 passes of trying to find parents, stopping");
                    break;
                }
            }

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

            if (config.Props.remotes != null)
            {
                foreach (var remoteInfo in config.Props.remotes)
                {
                    try
                    {
                        RemoteBuilder.Make(go, sector, remoteInfo, nhBody);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make remote [{remoteInfo.id}] for [{go.name}]:\n{ex}");
                    }
                }
            }
        }

        private static bool DoesParentExist(GameObject go, GeneralPointPropInfo prop)
        {
            if (string.IsNullOrEmpty(prop.parentPath))
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
