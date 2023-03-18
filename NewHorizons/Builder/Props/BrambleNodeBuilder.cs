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
        // Keys are all dimension names that have been referenced by at least one node but do not (yet) exist
        // Values are all nodes' warp controllers that link to a given dimension
        // UnpairedNodes[name of dimension that doesn't exist yet] => List{warp controller for node that links to that dimension, ...}
        private static readonly Dictionary<string, List<InnerFogWarpVolume>> _unpairedNodes = new();
        private static readonly Dictionary<string, List<SignalInfo>> _propagatedSignals = new();

        public static readonly Dictionary<string, InnerFogWarpVolume> namedNodes = new();
        public static readonly Dictionary<BrambleNodeInfo, GameObject> builtBrambleNodes = new();

        private static GameObject _brambleSeedPrefab;
        private static GameObject _brambleNodePrefab;

        public static void Init(PlanetConfig[] dimensionConfigs)
        {
            _unpairedNodes.Clear();
            _propagatedSignals.Clear();
            namedNodes.Clear();
            builtBrambleNodes.Clear();

            PropagateSignals(dimensionConfigs);
        }

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_brambleSeedPrefab == null) _brambleSeedPrefab = SearchUtilities.Find("DB_PioneerDimension_Body/Sector_PioneerDimension/Interactables_PioneerDimension/SeedWarp_ToPioneer (1)").InstantiateInactive().Rename("Prefab_DB_Seed").DontDestroyOnLoad();
            if (_brambleNodePrefab == null) _brambleNodePrefab = SearchUtilities.Find("DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension/InnerWarp_ToCluster").InstantiateInactive().Rename("Prefab_DB_Node").DontDestroyOnLoad();
        }

        public static void FinishPairingNodesForDimension(string dimensionName, AstroObject dimensionAO = null)
        {
            Logger.LogVerbose($"Pairing missed for {dimensionName}");
            if (!_unpairedNodes.ContainsKey(dimensionName)) return;

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
            InitPrefabs();

            var prefab = config.isSeed ? _brambleSeedPrefab : _brambleNodePrefab;

            // Spawn the bramble node
            var brambleNode = GeneralPropBuilder.MakeFromPrefab(prefab, config.name ?? "Bramble Node to " + config.linksTo, sector?.transform ?? go.transform, config);
            foreach (var collider in brambleNode.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = true; 
            }

            var innerFogWarpVolume = brambleNode.GetComponent<InnerFogWarpVolume>();
            var outerFogWarpVolume = GetOuterFogWarpVolumeFromAstroObject(go);
            var fogLight = brambleNode.GetComponent<FogLight>();

            // This node comes with Feldspar's signal, we don't want that though
            GameObject.Destroy(brambleNode.FindChild("Signal_Harmonica"));

            // Fix some components
            fogLight._parentBody = go.GetComponent<OWRigidbody>();
            fogLight._sector = sector;
            fogLight._linkedSector = null;
            fogLight._innerWarp = innerFogWarpVolume;
            fogLight._linkedFogLights = new List<FogLight>();
            fogLight._linkedLightData = new List<FogLight.LightData>();

            // If the config says only certain exits are allowed, enforce that
            if (config.possibleExits != null)
            {
                Delay.FireOnNextUpdate(() =>
                {
                    var exits = innerFogWarpVolume._exits;
                    var newExits = new List<SphericalFogWarpExit>();
                    foreach (var index in config.possibleExits)
                    {
                        if (index is < 0 or > 5) continue;
                        newExits.Add(exits[index]);
                    }
                    innerFogWarpVolume._exits = newExits.ToArray();
                });
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

            // Set the main scale
            brambleNode.transform.localScale = Vector3.one * config.scale;
            innerFogWarpVolume._warpRadius *= config.scale;
            innerFogWarpVolume._exitRadius *= config.scale;

            // Set the seed/node specific scales and other stuff
            if (config.isSeed)
            {
                //innerFogWarpVolume._exitRadius /= 1.8f;
                brambleNode.FindChild("PointLight_DB_FogLight").GetComponent<Light>().range *= config.scale;
                brambleNode.FindChild("Prefab_SeedPunctureVolume (2)").GetComponent<CompoundShape>().enabled = true;
                fogLight._maxVisibleDistance = float.PositiveInfinity; // Prefab does have working foglight aside from this
                fogLight._minVisibleDistance *= config.scale / 15f;
                fogLight._occlusionRange = 175f;
            }
            else
            {
                brambleNode.FindChild("Effects/PointLight_DB_FogLight").GetComponent<Light>().range *= config.scale;
                brambleNode.FindChild("Effects/FogOverrideVolume").GetComponent<FogOverrideVolume>().blendDistance *= config.scale;
                //fogLight._minVisibleDistance *= config.scale;

                // Seed fog works differently, so it doesn't need to be fixed
                // (it's also located on a different child path, so the below FindChild calls wouldn't work)
                var fog = brambleNode.FindChild("Effects/InnerWarpFogSphere");
                var fogMaterial = fog.GetComponent<MeshRenderer>().material;
                fogMaterial.SetFloat("_Radius", fogMaterial.GetFloat("_Radius") * config.scale);
                fogMaterial.SetFloat("_Density", fogMaterial.GetFloat("_Density") / config.scale);
            }

            // Set colors
            Color fogTint, farFogTint, fogLightTint, lightTint, lightShaftTint, glowTint, fogOverrideTint;

            farFogTint = config.fogTint != null ? config.fogTint.ToColor() : new Color(1f, 0.9608f, 0.851f, 1f);
            farFogTint.a = 1f;
            lightTint = config.lightTint != null ? config.lightTint.ToColor() : Color.white;

            Color.RGBToHSV(farFogTint, out var fogH, out var fogS, out var fogV);
            Color.RGBToHSV(lightTint, out var lightH, out var lightS, out var lightV);

            if (config.isSeed)
            {
                fogLightTint = lightTint;
                fogLightTint.a = config.hasFogLight != true ? 0f : lightTint.a * 0.5f;

                lightShaftTint = CalculateLightShaftTint(lightH, lightS, lightV);
                lightShaftTint.a = lightTint.a;
            }
            else
            {
                fogLightTint = lightTint;
                fogLightTint.a = config.hasFogLight == false || (outerFogWarpVolume == null && config.hasFogLight != true) ? 0f : lightTint.a * 0.5f;

                lightShaftTint = CalculateLightShaftTint(fogH, fogS, fogV);
                lightShaftTint.a = lightTint.a;
            }

            // Apply colors
            Delay.FireOnNextUpdate(() => {
                if (config.isSeed)
                {
                    SetSeedColors(brambleNode, farFogTint, fogLightTint, lightTint, lightShaftTint);
                }
                else
                {
                    // Set inner fog to destination fog tint
                    fogTint = AstroObjectLocator.GetAstroObject(config.linksTo).gameObject.FindChild("Sector/Atmosphere/FogSphere_Hub").GetComponent<PlanetaryFogController>().fogTint;

                    // Calculate glow and fog override
                    // Will work with any fog
                    Color dimFogTint;
                    if (go.GetComponentInChildren<PlanetaryFogController>())
                    {
                        dimFogTint = go.GetComponentInChildren<PlanetaryFogController>().fogTint;
                        Color.RGBToHSV(dimFogTint, out var dimH, out var dimS, out var dimV);
                        Color.RGBToHSV(lightShaftTint, out var shaftH, out var shaftS, out var shaftV);
                        glowTint = Color.HSVToRGB(shaftH, shaftS, dimV * 1.25f);
                        glowTint.a = lightTint.a;

                        // Scale glow shader properties, here due to dependencies
                        var glow = brambleNode.FindChild("Effects/InnerWarpFogGlow");
                        var glowMaterial = glow.GetComponent<MeshRenderer>().material;
                        var limit = outerFogWarpVolume?._exitRadius ?? float.PositiveInfinity;
                        glowMaterial.SetFloat("_NearFadeStart", 100f * config.scale);
                        glowMaterial.SetFloat("_NearFadeEnd", Mathf.Min(limit, 500f * config.scale * Mathf.Max(glowTint.a, 0.5f)));
                        glowMaterial.SetFloat("_FarFadeStart", Mathf.Min(limit, 500f * config.scale * Mathf.Max(glowTint.a, 0.5f)));
                        glowMaterial.SetFloat("_FarFadeEnd", Mathf.Min(limit * 1.1f, 1000f * config.scale * Mathf.Max(glowTint.a, 0.5f)));

                        fogOverrideTint = Color.HSVToRGB(fogH, Mathf.Lerp(fogS, dimS, 0.5f), Mathf.Lerp(fogV, dimV, 0.5f));
                        fogOverrideTint.a = 1f;
                    }
                    else
                    {
                        glowTint = fogOverrideTint = Color.clear;
                    }

                    SetNodeColors(brambleNode, fogTint, farFogTint, fogLightTint, lightTint, lightShaftTint, glowTint, fogOverrideTint);
                }

                // Redo the foglight data after everything is colored
                if (fogLight._linkedFogLights != null)
                {
                    Delay.FireOnNextUpdate(() =>
                    {
                        FogLightManager fogLightManager = Locator.GetFogLightManager();
                        fogLight._linkedLightData.Clear();
                        for (int i = 0; i < fogLight._linkedFogLights.Count; i++)
                        {
                            FogLight.LightData lightData = new FogLight.LightData();
                            lightData.color = fogLight._linkedFogLights[i].GetTint();
                            lightData.maxAlpha = fogLight._linkedFogLights[i]._maxAlpha;
                            fogLight._linkedLightData.Add(lightData);
                            fogLightManager.RegisterLightData(lightData);
                        }
                    });
                }
            });

            // Set up warps
            innerFogWarpVolume._sector = sector;
            innerFogWarpVolume._attachedBody = go.GetComponent<OWRigidbody>();

            // the OuterFogWarpVolume of the dimension this node is inside of
            // (null if this node is not inside of a bramble dimension, eg it's sitting on a planet or something)
            innerFogWarpVolume._containerWarpVolume = outerFogWarpVolume;

            var success = PairEntrance(innerFogWarpVolume, config.linksTo);
            if (!success) RecordUnpairedNode(innerFogWarpVolume, config.linksTo);

            if (config.preventRecursionCrash)
            {
                Delay.FireOnNextUpdate(() =>
                {
                    var destination = GetOuterFogWarpVolumeFromAstroObject(AstroObjectLocator.GetAstroObject(config.linksTo).gameObject);
                    if (destination != null) destination._senderWarps.Remove(innerFogWarpVolume);
                });
            }

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

            StreamingHandler.SetUpStreaming(brambleNode, sector);

            // Done!
            brambleNode.SetActive(true);
            return brambleNode;

            static Color CalculateLightShaftTint(float H, float S, float V)
            {
                // Sine curve approximation shifts hue to compensate for shader shenanigans
                H += -1f / 24f * Mathf.Sin(6f * Mathf.PI * H);

                // Inverted parabola is best fit for limited base game examples
                S = -Mathf.Pow(S - 1f, 2f) + 1f;

                return Color.HSVToRGB(H, S, V);
            }
        }

        public static void SetNodeColors(GameObject brambleNode, Color fogTint, Color farFogTint, Color fogLightTint, Color lightTint, Color lightShaftTint, Color glowTint, Color fogOverrideTint)
        {
            var innerFogWarpVolume = brambleNode.GetComponent<InnerFogWarpVolume>();
            innerFogWarpVolume._fogColor = fogTint;
            innerFogWarpVolume._farFogColor = farFogTint;

            var fogLight = brambleNode.GetComponent<FogLight>();
            fogLight._maxAlpha = fogLightTint.a;
            fogLight._primaryLightData.maxAlpha = fogLightTint.a;
            fogLight._tint = fogLightTint;
            fogLight._primaryLightData.color = fogLightTint;

            var light = brambleNode.FindChild("Effects/PointLight_DB_FogLight").GetComponent<Light>();
            light.intensity = lightTint.a * 0.7f;
            light.color = lightTint;

            var lightShafts = brambleNode.FindChild("Effects/DB_BrambleLightShafts");
            var lightShaft1 = lightShafts.FindChild("BrambleLightShaft1");
            var mat = lightShaft1.GetComponent<MeshRenderer>().material;
            mat.color = lightShaftTint;
            for (int i = 1; i <= 6; i++)
            {
                var lightShaft = lightShafts.FindChild($"BrambleLightShaft{i}");
                lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }

            var glow = brambleNode.FindChild("Effects/InnerWarpFogGlow");
            glow.GetComponent<MeshRenderer>().material.color = glowTint;
            glow.transform.localScale *= glowTint.a;
            if (glowTint.a == 0f) glow.SetActive(false);

            var fogOverride = brambleNode.FindChild("Effects/FogOverrideVolume");
            if (fogOverrideTint.a == 1f) // Override turns goofy if alpha isn't 1
            {
                var volume = fogOverride.GetComponent<FogOverrideVolume>();
                volume.tint = fogOverrideTint;
                volume.blendDistance *= lightTint.a;
                volume.radius *= lightTint.a;
            }
            else fogOverride.SetActive(false);
        }

        public static void SetSeedColors(GameObject brambleSeed, Color farFogTint, Color fogLightTint, Color lightTint, Color lightShaftTint)
        {
            brambleSeed.GetComponent<InnerFogWarpVolume>()._fogColor = farFogTint;
            brambleSeed.FindChild("VolumetricFogSphere (2)").GetComponent<MeshRenderer>().material.color = new Color(farFogTint.r * 10, farFogTint.g * 10, farFogTint.b * 10, farFogTint.a);

            var fogLight = brambleSeed.GetComponent<FogLight>();
            fogLight._maxAlpha = fogLightTint.a;
            fogLight._primaryLightData.maxAlpha = fogLightTint.a;
            fogLight._tint = fogLightTint;
            fogLight._primaryLightData.color = fogLightTint;

            var light = brambleSeed.FindChild("PointLight_DB_FogLight").GetComponent<Light>();
            light.intensity = lightTint.a;
            light.color = lightTint;

            var lightShafts = brambleSeed.FindChild("Terrain_DB_BrambleSphere_Seed_V2 (2)/DB_SeedLightShafts");
            var lightShaft1 = lightShafts.FindChild("DB_SeedLightShafts1");
            var mat = lightShaft1.GetComponent<MeshRenderer>().material;
            mat.color = lightShaftTint;
            for (int i = 1; i <= 6; i++)
            {
                var lightShaft = lightShafts.FindChild($"DB_SeedLightShafts{i}");
                lightShaft.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
        }
    }
}
