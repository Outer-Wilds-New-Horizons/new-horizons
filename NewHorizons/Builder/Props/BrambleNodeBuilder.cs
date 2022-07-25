using NewHorizons.Builder.Body;
using NewHorizons.Components;
using NewHorizons.External.Configs;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NewHorizons.External.Modules.BrambleModule;
using static NewHorizons.External.Modules.SignalModule;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    // TODO
    //3) support for existing dimensions?
    //5) test whether nodes can lead to vanilla dimensions

    public static class BrambleNodeBuilder
    {
        // keys are all dimension names that have been referenced by at least one node but do not (yet) exist
        // values are all nodes' warp controllers that link to a given dimension
        // unpairedNodes[name of dimension that doesn't exist yet] => List{warp controller for node that links to that dimension, ...}
        private static readonly Dictionary<string, List<InnerFogWarpVolume>> _unpairedNodes = new();
        private static readonly Dictionary<string, List<SignalInfo>> _propagatedSignals = new();

        public static readonly Dictionary<string, InnerFogWarpVolume> namedNodes = new();
        public static readonly Dictionary<BrambleNodeInfo, GameObject> builtBrambleNodes = new();

        private static string _brambleSeedPrefabPath = "DB_PioneerDimension_Body/Sector_PioneerDimension/Interactables_PioneerDimension/SeedWarp_ToPioneer (1)";
        private static string _brambleNodePrefabPath = "DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster";

        public static void Init(PlanetConfig[] dimensionConfigs)
        {
            _unpairedNodes.Clear();
            _propagatedSignals.Clear();
            namedNodes.Clear();
            builtBrambleNodes.Clear();

            PropagateSignals(dimensionConfigs);
        }

        public static void FinishPairingNodesForDimension(string dimensionName, AstroObject dimensionAO = null)
        {
            Logger.LogVerbose($"Pairing missed for {dimensionName}");
            if (!_unpairedNodes.ContainsKey(dimensionName)) return;

            Logger.LogVerbose("proceeding");
            foreach (var nodeWarpController in _unpairedNodes[dimensionName])
            {
                Logger.LogVerbose($"Pairing node {nodeWarpController.gameObject.name} links to {dimensionName}");
                PairEntrance(nodeWarpController, dimensionName, dimensionAO);
            }

            _unpairedNodes.Remove(dimensionName);
        }

        private static void RecordUnpairedNode(InnerFogWarpVolume warpVolume, string linksTo)
        {
            if (!_unpairedNodes.ContainsKey(linksTo)) _unpairedNodes[linksTo] = new();

            Logger.LogVerbose($"Recording node {warpVolume.gameObject.name} links to {linksTo}");

            _unpairedNodes[linksTo].Add(warpVolume);
        }

        private static OuterFogWarpVolume GetOuterFogWarpVolumeFromAstroObject(GameObject go)
        {
            var outerWarpGO = go.FindChild("Sector/OuterWarp");
            if (outerWarpGO == null) return null;

            var outerFogWarpVolume = outerWarpGO.GetComponent<OuterFogWarpVolume>();
            return outerFogWarpVolume;
        }

        // Makes signals inside dimensions appear on the nodes as well
        // Runs Floyd-Warshall algorithm on dimensions and nodes.
        private static void PropagateSignals(PlanetConfig[] dimensionConfigs)
        {
            // Access will be our final answer - if access[i, j], then nodes linking to dimension i should display all of dimension j's signals
            var access = new bool[dimensionConfigs.Count(), dimensionConfigs.Count()];

            var dimensionNameToIndex = new Dictionary<string, int>();
            for (int dimensionIndex = 0; dimensionIndex < dimensionConfigs.Count(); dimensionIndex++)
            {
                dimensionNameToIndex[dimensionConfigs[dimensionIndex].name] = dimensionIndex;
            }

            // Set up the direct links (ie, if dimension 0 contains a node that links to dimension 3, set access[0, 3] = true)
            for (int dimensionIndex = 0; dimensionIndex < dimensionConfigs.Count(); dimensionIndex++)
            {
                var dimension = dimensionConfigs[dimensionIndex];
                if (dimension.Bramble.nodes == null) continue;
                foreach (var node in dimension.Bramble.nodes)
                {
                    var destinationDimensionIndex = dimensionNameToIndex[node.linksTo];
                    access[dimensionIndex, destinationDimensionIndex] = true;
                }
            }

            // A node that links to dimension A should display all of dimension A's signals, so for the purposes of our function,
            // we need to say that dimension A links to dimension A
            for (int dimensionIndex = 0; dimensionIndex < dimensionConfigs.Count(); dimensionIndex++)
            {
                access[dimensionIndex, dimensionIndex] = true;
            }

            // The actual Floyd-Warshall - determine whether each pair of dimensions link indirectly (eg if A->B->C,
            // then after this step, access[A, C] = true)
            for (int k = 0; k < dimensionConfigs.Count(); k++)
                for (int i = 0; i < dimensionConfigs.Count(); i++)
                    for (int j = 0; j < dimensionConfigs.Count(); j++)
                        if (access[i, k] && access[k, j])
                            access[i, j] = true;

            // This dictionary lists all the signals a given node should have, depending on the dimension it links to
            // ie, if a node links to "dimension1", then that node should spawn all of the signals in the list propagatedSignals["dimension1"]
            foreach (var dimension in dimensionConfigs)
            {
                _propagatedSignals[dimension.name] = new();
                var dimensionIndex = dimensionNameToIndex[dimension.name];

                foreach (var destinationDimension in dimensionConfigs)
                {
                    if (destinationDimension.Props?.signals == null) continue;

                    var destinationIndex = dimensionNameToIndex[destinationDimension.name];
                    if (access[dimensionIndex, destinationIndex])
                    {
                        _propagatedSignals[dimension.name].AddRange(destinationDimension.Props.signals);
                    }
                }
            }
        }

        // Returns ture or false depending on if it succeeds 
        private static bool PairEntrance(InnerFogWarpVolume nodeWarp, string destinationName, AstroObject dimensionAO = null)
        {
            Logger.LogVerbose($"Pairing node {nodeWarp.gameObject.name} to {destinationName}");

            var destinationAO = dimensionAO ?? AstroObjectLocator.GetAstroObject(destinationName);
            if (destinationAO == null) return false;

            Logger.LogVerbose($"Found {destinationName} as gameobject {destinationAO.gameObject.name} (was passed in: {dimensionAO != null})");

            // link the node's warp volume to the destination's
            var destination = GetOuterFogWarpVolumeFromAstroObject(destinationAO.gameObject);
            if (destination == null) return false;

            Logger.LogVerbose($"Proceeding with pairing node {nodeWarp.gameObject.name} to {destinationName}. Path to outer fog warp volume: {destination.transform.GetPath()}");

            nodeWarp._linkedOuterWarpVolume = destination;
            destination.RegisterSenderWarp(nodeWarp);

            return true;
        }

        public static void Make(GameObject go, Sector sector, BrambleNodeInfo[] configs, IModBehaviour mod)
        {
            foreach (var config in configs)
            {
                Make(go, sector, config, mod);
            }
        }

        public static GameObject Make(GameObject go, Sector sector, BrambleNodeInfo config, IModBehaviour mod)
        {
            var prefab = SearchUtilities.Find(config.isSeed ? _brambleSeedPrefabPath : _brambleNodePrefabPath);

            // Spawn the bramble node
            var brambleNode = prefab.InstantiateInactive();
            foreach (var collider in brambleNode.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = true; 
            }

            var innerFogWarpVolume = brambleNode.GetComponent<InnerFogWarpVolume>();
            var fogLight = brambleNode.GetComponent<FogLight>();

            brambleNode.transform.parent = sector.transform;
            brambleNode.transform.position = go.transform.TransformPoint(config.position ?? Vector3.zero);
            brambleNode.transform.rotation = go.transform.TransformRotation(Quaternion.Euler(config.rotation ?? Vector3.zero));
            brambleNode.name = "Bramble Node to " + config.linksTo;

            // This node comes with Feldspar's signal, we don't want that though
            GameObject.Destroy(brambleNode.FindChild("Signal_Harmonica"));

            // Fix some components
            fogLight._parentBody = go.GetComponent<OWRigidbody>();
            fogLight._sector = sector;
            fogLight._linkedSector = null;
            fogLight._innerWarp = innerFogWarpVolume;
            fogLight._linkedFogLights = new List<FogLight>();
            fogLight._linkedLightData = new List<FogLight.LightData>();

            sector.RegisterFogLight(fogLight);
        
            // If the config says only certain exits are allowed, enforce that
            if (config.possibleExits != null)
            {
                var exits = innerFogWarpVolume._exits;
                var newExits = new List<SphericalFogWarpExit>();
                foreach (var index in config.possibleExits)
                {
                    if(index is < 0 or > 5) continue;
                    newExits.Add(exits[index]);
                }
                innerFogWarpVolume._exits = newExits.ToArray();
            }

            // Set up screen fog effect 
            var fogEffectRuleset = sector.gameObject.GetAddComponent<EffectRuleset>();
            fogEffectRuleset._attachedBody = sector.GetAttachedOWRigidbody();
            fogEffectRuleset._triggerVolume = sector.GetTriggerVolume();
            fogEffectRuleset._type = EffectRuleset.BubbleType.FogWarp;
            fogEffectRuleset._underwaterDistortScale = 0.001f;
            fogEffectRuleset._underwaterMaxDistort = 0.1f;
            fogEffectRuleset._underwaterMinDistort = 0.005f;
            fogEffectRuleset._material = GameObject.Find("DB_PioneerDimension_Body/Sector_PioneerDimension").GetComponent<EffectRuleset>()._material;

            // TODO: replace InnerFogWarpVolume with NHInnerFogWarpVolume, which overrides GetFogDensity to
            // account for scale (this will fix the issue with screen fog caused by scaled down nodes)

            // Set the scale
            brambleNode.transform.localScale = Vector3.one * config.scale;
            innerFogWarpVolume._warpRadius *= config.scale;
            innerFogWarpVolume._exitRadius *= config.scale;

            // Seed fog works differently, so it doesn't need to be fixed
            // (it's also located on a different child path, so the below FindChild calls wouldn't work)
            if (!config.isSeed)
            {
                var fog = brambleNode.FindChild("Effects/InnerWarpFogSphere");
                var fogMaterial = fog.GetComponent<MeshRenderer>().material;
                fog.transform.localScale /= config.scale;
                fogMaterial.SetFloat("_Radius", fogMaterial.GetFloat("_Radius") * config.scale);
                fogMaterial.SetFloat("_Density", fogMaterial.GetFloat("_Density") / config.scale);
            }

            // Change the colors
            if (config.isSeed) SetSeedColors(brambleNode, config.fogTint?.ToColor(), config.lightTint?.ToColor());
            else SetNodeColors(brambleNode, config.fogTint?.ToColor(), config.lightTint?.ToColor());

            innerFogWarpVolume._useFarFogColor = false;
            if (config.farFogTint != null)
            {
                innerFogWarpVolume._useFarFogColor = true;
                innerFogWarpVolume._farFogColor = config.farFogTint.ToColor();
            }

            // Set up warps
            innerFogWarpVolume._sector = sector;
            innerFogWarpVolume._attachedBody = go.GetComponent<OWRigidbody>();

            // the OuterFogWarpVolume of the dimension this node is inside of
            // (null if this node is not inside of a bramble dimension, eg it's sitting on a planet or something)
            innerFogWarpVolume._containerWarpVolume = GetOuterFogWarpVolumeFromAstroObject(go); 

            var success = PairEntrance(innerFogWarpVolume, config.linksTo);
            if (!success) RecordUnpairedNode(innerFogWarpVolume, config.linksTo);

            // Cleanup for dimension exits
            if (config.name != null)
            {
                namedNodes[config.name] = innerFogWarpVolume;
                BrambleDimensionBuilder.FinishPairingDimensionsForExitNode(config.name);
            }

            // Make signals
            if (_propagatedSignals.TryGetValue(config.linksTo, out var connectedSignals))
            {
                foreach (var signalConfig in connectedSignals)
                {
                    var signalGO = SignalBuilder.Make(go, sector, signalConfig, mod);
                    signalGO.GetComponent<AudioSignal>()._identificationDistance = 0;
                    signalGO.GetComponent<AudioSignal>()._sourceRadius = 1;
                    signalGO.transform.position = brambleNode.transform.position;
                    signalGO.transform.parent = brambleNode.transform;
                }
            }

            // Done!
            brambleNode.SetActive(true);
            return brambleNode;
        }

        public static void SetNodeColors(GameObject brambleNode, Color? fogTint, Color? lightTint)
        {
            if (fogTint != null)
            {
                var fogRenderer = brambleNode.GetComponent<InnerFogWarpVolume>();

                fogRenderer._fogColor = fogTint.Value;

                var fogBackdrop = brambleNode.FindChild("Terrain_DB_BrambleSphere_Inner_v2/fogbackdrop_v2");
                if (fogBackdrop != null) 
                    fogBackdrop.GetComponent<MeshRenderer>().material.color = fogTint.Value;
            }

            if (lightTint != null)
            {
                var lightShafts = brambleNode.FindChild("Effects/DB_BrambleLightShafts");

                var lightShaft1 = lightShafts.FindChild("BrambleLightShaft1");
                var mat = lightShaft1.GetComponent<MeshRenderer>().material;
                mat.color = lightTint.Value;

                for (int i = 1; i <= 6; i++)
                {
                    var lightShaft = lightShafts.FindChild($"BrambleLightShaft{i}");
                    lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }
        }

        public static void SetSeedColors(GameObject brambleSeed, Color? fogTint, Color? lightTint)
        {
            if (fogTint != null)
            {
                var fogRenderer = brambleSeed.FindChild("VolumetricFogSphere (2)");

                var fogMeshRenderer = fogRenderer.GetComponent<MeshRenderer>();
                fogMeshRenderer.material.color = fogTint.Value;
            }

            if (lightTint != null)
            {
                var lightShafts = brambleSeed.FindChild("Terrain_DB_BrambleSphere_Seed_V2 (2)/DB_SeedLightShafts");

                var lightShaft1 = lightShafts.FindChild("DB_SeedLightShafts1");
                var mat = lightShaft1.GetComponent<MeshRenderer>().material;
                mat.color = lightTint.Value;

                for (int i = 1; i <= 6; i++)
                {
                    var lightShaft = lightShafts.FindChild($"DB_SeedLightShafts{i}");
                    lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
                }
            }
        }
    }
}
