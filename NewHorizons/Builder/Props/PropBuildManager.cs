using NewHorizons.Builder.Body;
using NewHorizons.Builder.Props.Audio;
using NewHorizons.Builder.Props.TranslatorText;
using NewHorizons.Builder.ShipLog;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class PropBuildManager
    {
        public static void Make(GameObject go, Sector sector, OWRigidbody planetBody, NewHorizonsBody nhBody)
        {
            PlanetConfig config = nhBody.Config;
            IModBehaviour mod = nhBody.Mod;

            if (config.Props.gravityCannons != null)
            {
                foreach (var gravityCannonInfo in config.Props.gravityCannons)
                {
                    try
                    {
                        GravityCannonBuilder.Make(go, sector, gravityCannonInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make gravity cannon [{gravityCannonInfo.shuttleID}] for [{go.name}]:\n{ex}");
                    }
                }
            }
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
            if (config.Props.details != null)
            {
                foreach (var detail in config.Props.details)
                {
                    try
                    {
                        var detailGO = DetailBuilder.Make(go, sector, mod, detail);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make planet detail [{detail.path}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Props.shuttles != null)
            {
                foreach (var shuttleInfo in config.Props.shuttles)
                {
                    try
                    {
                        ShuttleBuilder.Make(go, sector, nhBody.Mod, shuttleInfo);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make shuttle [{shuttleInfo.id}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Props.geysers != null)
            {
                foreach (var geyserInfo in config.Props.geysers)
                {
                    try
                    {
                        GeyserBuilder.Make(go, sector, geyserInfo);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make geyser for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (Main.HasDLC && config.Props.rafts != null)
            {
                foreach (var raftInfo in config.Props.rafts)
                {
                    try
                    {
                        RaftBuilder.Make(go, sector, raftInfo, planetBody);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make raft for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Props.tornados != null)
            {
                foreach (var tornadoInfo in config.Props.tornados)
                {
                    try
                    {
                        TornadoBuilder.Make(go, sector, tornadoInfo, config.Atmosphere?.clouds != null);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make tornado for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Props.volcanoes != null)
            {
                foreach (var volcanoInfo in config.Props.volcanoes)
                {
                    try
                    {
                        VolcanoBuilder.Make(go, sector, volcanoInfo);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make volcano for [{go.name}]:\n{ex}");
                    }
                }
            }
            // Reminder that dialogue has to be built after props if they're going to be using CharacterAnimController stuff
            if (config.Props.dialogue != null)
            {
                foreach (var dialogueInfo in config.Props.dialogue)
                {
                    try
                    {
                        var (dialogue, trigger) = DialogueBuilder.Make(go, sector, dialogueInfo, mod);
                        if (dialogue == null)
                        {
                            NHLogger.LogVerbose($"[DIALOGUE] Failed to create dialogue [{dialogueInfo.xmlFile}]");
                        }
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"[DIALOGUE] Couldn't make dialogue [{dialogueInfo.xmlFile}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Props.entryLocation != null)
            {
                foreach (var entryLocationInfo in config.Props.entryLocation)
                {
                    try
                    {
                        EntryLocationBuilder.Make(go, sector, entryLocationInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make entry location [{entryLocationInfo.id}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            // Backwards compatibility 
#pragma warning disable 612, 618
            if (config.Props.nomaiText != null)
            {
                foreach (var nomaiTextInfo in config.Props.nomaiText)
                {
                    try
                    {
                        NomaiTextBuilder.Make(go, sector, nomaiTextInfo, nhBody.Mod);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make text [{nomaiTextInfo.xmlFile}] for [{go.name}]:\n{ex}");
                    }

                }
            }
#pragma warning restore 612, 618
            if (config.Props.translatorText != null)
            {
                foreach (var nomaiTextInfo in config.Props.translatorText)
                {
                    try
                    {
                        TranslatorTextBuilder.Make(go, sector, nomaiTextInfo, nhBody);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make text [{nomaiTextInfo.xmlFile}] for [{go.name}]:\n{ex}");
                    }

                }
            }
            if (Main.HasDLC && config.Props.slideShows != null)
            {
                foreach (var slideReelInfo in config.Props.slideShows)
                {
                    try
                    {
                        ProjectionBuilder.Make(go, sector, slideReelInfo, mod);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make slide reel for [{go.name}]:\n{ex}");
                    }
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
            if (config.Props.singularities != null)
            {
                foreach (var singularity in config.Props.singularities)
                {
                    try
                    {
                        SingularityBuilder.Make(go, sector, go.GetComponent<OWRigidbody>(), config, singularity);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make singularity \"{(string.IsNullOrEmpty(singularity.uniqueID) ? config.name : singularity.uniqueID)}\" for [{go.name}]::\n{ex}");
                    }
                }
            }
            if (config.Props.signals != null)
            {
                foreach (var signal in config.Props.signals)
                {
                    try
                    {
                        SignalBuilder.Make(go, sector, signal, mod);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make signal on planet [{config.name}] - {ex}");
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
            if (config.Props.warpReceivers != null)
            {
                foreach (var warpReceiver in config.Props.warpReceivers)
                {
                    try
                    {
                        WarpPadBuilder.Make(go, sector, nhBody.Mod, warpReceiver);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make warp receiver [{warpReceiver.frequency}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Props.warpTransmitters != null)
            {
                foreach (var warpTransmitter in config.Props.warpTransmitters)
                {
                    try
                    {
                        WarpPadBuilder.Make(go, sector, nhBody.Mod, warpTransmitter);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make warp transmitter [{warpTransmitter.frequency}] for [{go.name}]:\n{ex}");
                    }
                }
            }
            if (config.Props.audioSources != null)
            {
                foreach (var audioSource in config.Props.audioSources)
                {
                    try
                    {
                        AudioSourceBuilder.Make(go, sector, audioSource, mod);
                    }
                    catch (Exception ex)
                    {
                        NHLogger.LogError($"Couldn't make audio source [{audioSource.audio}] for [{go.name}]:\n{ex}");
                    }
                }
            }
        }
    }
}
