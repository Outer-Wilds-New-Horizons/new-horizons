using NewHorizons.External.Configs;
using NewHorizons.Utility;
using System;
using NewHorizons.External.Modules.VariableSize;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using System.Collections.Generic;
using System.Linq;
using NewHorizons.Components.SizeControllers;
using System.Drawing;
using Color = UnityEngine.Color;
using NewHorizons.Components.Volumes;

namespace NewHorizons.Builder.Body
{
    public static class SingularityBuilder
    {
        private static readonly string _blackHoleProxyPath = "TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_TT/Prefab_NOM_WarpTransmitter (1)/BlackHole/BlackHoleSingularity";
        private static readonly string _whiteHoleProxyPath = "TowerTwin_Body/Sector_TowerTwin/Sector_Tower_HGT/Interactables_Tower_HGT/Interactables_Tower_CT/Prefab_NOM_WarpTransmitter/WhiteHole/WhiteHoleSingularity";
        private static GameObject _blackHoleProxyPrefab, _whiteHoleProxyPrefab;

        private static Shader blackHoleShader = null;
        private static Shader whiteHoleShader = null;

        public static readonly int Radius = Shader.PropertyToID("_Radius");
        public static readonly int MaxDistortRadius = Shader.PropertyToID("_MaxDistortRadius");
        public static readonly int MassScale = Shader.PropertyToID("_MassScale");
        public static readonly int DistortFadeDist = Shader.PropertyToID("_DistortFadeDist");
        public static readonly int Color1 = Shader.PropertyToID("_Color");

        private static Dictionary<string, GameObject> _singularitiesByID;

        private static Mesh _blackHoleMesh;
        private static GameObject _blackHoleAmbience;
        private static GameObject _blackHoleEmissionOneShot;
        private static GameObject _blackHoleVolume;

        private static Mesh _whiteHoleMesh;
        private static GameObject _whiteHoleAmbientLight;
        private static GameObject _whiteHoleAmbience;
        private static GameObject _whiteHoleZeroGVolume;
        private static GameObject _whiteHoleRulesetVolume;
        private static GameObject _whiteHoleVolume;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_blackHoleProxyPrefab == null) _blackHoleProxyPrefab = SearchUtilities.Find(_blackHoleProxyPath).InstantiateInactive().Rename("BlackHoleSingularity").DontDestroyOnLoad();
            if (_whiteHoleProxyPrefab == null) _whiteHoleProxyPrefab = SearchUtilities.Find(_whiteHoleProxyPath).InstantiateInactive().Rename("WhiteHoleSingularity").DontDestroyOnLoad();

            if (blackHoleShader == null) blackHoleShader = SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshRenderer>().sharedMaterial.shader;
            if (whiteHoleShader == null) whiteHoleShader = SearchUtilities.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshRenderer>().sharedMaterial.shader;

            if (_blackHoleMesh == null) _blackHoleMesh = SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshFilter>().mesh.DontDestroyOnLoad();
            if (_blackHoleAmbience == null) _blackHoleAmbience = SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleAmbience").InstantiateInactive().Rename("BlackHoleAmbience").DontDestroyOnLoad();
            if (_blackHoleEmissionOneShot == null) _blackHoleEmissionOneShot = SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleEmissionOneShot").InstantiateInactive().Rename("BlackHoleEmissionOneShot").DontDestroyOnLoad();
            if (_blackHoleVolume == null) _blackHoleVolume = SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleVolume").InstantiateInactive().Rename("BlackHoleVolume").DontDestroyOnLoad();

            if (_whiteHoleMesh == null) _whiteHoleMesh = SearchUtilities.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshFilter>().mesh.DontDestroyOnLoad();
            if (_whiteHoleAmbientLight == null) _whiteHoleAmbientLight = SearchUtilities.Find("WhiteHole_Body/WhiteHoleVisuals/AmbientLight_WH").InstantiateInactive().Rename("WhiteHoleAmbientLight").DontDestroyOnLoad();
            if (_whiteHoleAmbience == null) _whiteHoleAmbience = SearchUtilities.Find("WhiteHole_Body/Ambience_WhiteHole").InstantiateInactive().Rename("WhiteHoleAmbience").DontDestroyOnLoad();
            if (_whiteHoleZeroGVolume == null) _whiteHoleZeroGVolume = SearchUtilities.Find("WhiteHole_Body/ZeroGVolume").InstantiateInactive().Rename("WhiteHoleZeroGVolume").DontDestroyOnLoad();
            if (_whiteHoleRulesetVolume == null) _whiteHoleRulesetVolume = SearchUtilities.Find("WhiteHole_Body/Sector_WhiteHole/RulesetVolumes_WhiteHole").InstantiateInactive().Rename("WhiteHoleRulesetVolume").DontDestroyOnLoad();
            if (_whiteHoleVolume == null) _whiteHoleVolume = SearchUtilities.Find("WhiteHole_Body/WhiteHoleVolume").InstantiateInactive().Rename("WhiteHoleVolume").DontDestroyOnLoad();
        }

        public static void Make(GameObject go, Sector sector, OWRigidbody OWRB, PlanetConfig config, SingularityModule singularity)
        {
            InitPrefabs();

            // If we've reloaded the first one will now be null so we have to refresh the list
            if (_singularitiesByID?.Values?.FirstOrDefault() == null) _singularitiesByID = new Dictionary<string, GameObject>();

            var horizonRadius = singularity.horizonRadius;
            var distortRadius = singularity.distortRadius != 0f ? singularity.distortRadius : horizonRadius * 2.5f;
            var pairedSingularity = singularity.pairedSingularity;

            bool polarity = singularity.type == SingularityModule.SingularityType.BlackHole;

            bool isWormHole = singularity?.targetStarSystem != null;
            bool hasHazardVolume = !isWormHole && (pairedSingularity == null);

            Vector3 localPosition = singularity?.position == null ? Vector3.zero : singularity.position;

            GameObject newSingularity = null;
            newSingularity = MakeSingularity(go, sector, localPosition, polarity, horizonRadius, distortRadius, 
                hasHazardVolume, singularity.targetStarSystem, singularity.curve, singularity.hasWarpEffects, singularity.renderQueueOverride);

            var uniqueID = string.IsNullOrEmpty(singularity.uniqueID) ? config.name : singularity.uniqueID;
            _singularitiesByID.Add(uniqueID, newSingularity);

            // Try to pair them
            if (!string.IsNullOrEmpty(pairedSingularity) && newSingularity != null)
            {
                if (_singularitiesByID.TryGetValue(pairedSingularity, out var pairedSingularityGO))
                {
                    switch (polarity)
                    {
                        case true:
                            PairSingularities(uniqueID, pairedSingularity, newSingularity, pairedSingularityGO);
                            break;
                        case false:
                            PairSingularities(pairedSingularity, uniqueID, pairedSingularityGO, newSingularity);
                            break;
                    }
                }
            }
        }

        public static void PairSingularities(string blackHoleID, string whiteHoleID, GameObject blackHole, GameObject whiteHole)
        {
            InitPrefabs();

            if (blackHole == null || whiteHole == null) return;

            Logger.LogVerbose($"Pairing singularities [{blackHoleID}], [{whiteHoleID}]");

            var whiteHoleVolume = whiteHole.GetComponentInChildren<WhiteHoleVolume>();
            var blackHoleVolume = blackHole.GetComponentInChildren<BlackHoleVolume>();

            if (whiteHoleVolume == null || blackHoleVolume == null)
            {
                Logger.LogWarning($"[{blackHoleID}] and [{whiteHoleID}] do not have compatible polarities");
                return;
            }

            blackHoleVolume._whiteHole = whiteHoleVolume;
        }

        public static GameObject MakeSingularity(GameObject planetGO, Sector sector, Vector3 position, bool polarity, float horizon, float distort,
            bool hasDestructionVolume, string targetStarSystem = null, TimeValuePair[] curve = null, bool warpEffects = true, int renderQueue = 2985)
        {
            InitPrefabs();

            // polarity true = black, false = white

            var singularity = new GameObject(polarity ? "BlackHole" : "WhiteHole");
            singularity.transform.parent = sector?.transform ?? planetGO.transform;
            singularity.transform.position = planetGO.transform.TransformPoint(position);

            var singularityRenderer = MakeSingularityGraphics(singularity, polarity, horizon, distort, renderQueue);

            SingularitySizeController sizeController = null;
            if (curve != null)
            {
                sizeController = singularityRenderer.gameObject.AddComponent<SingularitySizeController>();
                sizeController.SetScaleCurve(curve);
                sizeController.size = distort;
                sizeController.innerScale = horizon;
                sizeController.material = singularityRenderer.material;
            }

            OWAudioSource oneShotOWAudioSource = null;
            
            var singularityAmbience = GameObject.Instantiate(_blackHoleAmbience, singularity.transform);
            singularityAmbience.name = polarity ? "BlackHoleAmbience" : "WhiteHoleAmbience";
            singularityAmbience.SetActive(true);
            singularityAmbience.GetComponent<SectorAudioGroup>().SetSector(sector);

            var singularityAudioSource = singularityAmbience.GetComponent<AudioSource>();
            singularityAudioSource.maxDistance = distort * 2.5f;
            singularityAudioSource.minDistance = horizon;
            singularityAmbience.transform.localPosition = Vector3.zero;
            if (sizeController != null) sizeController.audioSource = singularityAudioSource;

            if (polarity)
            {
                if (hasDestructionVolume || targetStarSystem != null)
                {
                    var destructionVolumeGO = new GameObject("DestructionVolume");
                    destructionVolumeGO.layer = LayerMask.NameToLayer("BasicEffectVolume");
                    destructionVolumeGO.transform.parent = singularity.transform;
                    destructionVolumeGO.transform.localScale = Vector3.one;
                    destructionVolumeGO.transform.localPosition = Vector3.zero;

                    var sphereCollider = destructionVolumeGO.AddComponent<SphereCollider>();
                    sphereCollider.radius = horizon;
                    sphereCollider.isTrigger = true;
                    if (sizeController != null) sizeController.sphereCollider = sphereCollider;

                    if (hasDestructionVolume) destructionVolumeGO.AddComponent<BlackHoleDestructionVolume>();
                    else if (targetStarSystem != null)
                    {
                        var wormholeVolume = destructionVolumeGO.AddComponent<ChangeStarSystemVolume>();
                        wormholeVolume.TargetSolarSystem = targetStarSystem;
                    }
                }
                else
                {
                    var blackHoleOneShot = GameObject.Instantiate(_blackHoleEmissionOneShot, singularity.transform);
                    blackHoleOneShot.name = "BlackHoleEmissionOneShot";
                    blackHoleOneShot.SetActive(true);
                    oneShotOWAudioSource = blackHoleOneShot.GetComponent<OWAudioSource>();
                    var oneShotAudioSource = blackHoleOneShot.GetComponent<AudioSource>();
                    oneShotAudioSource.maxDistance = distort * 3f;
                    oneShotAudioSource.minDistance = horizon;
                    if (sizeController != null) sizeController.oneShotAudioSource = oneShotAudioSource;

                    var blackHoleVolume = GameObject.Instantiate(_blackHoleVolume, singularity.transform);
                    blackHoleVolume.name = "BlackHoleVolume";
                    blackHoleVolume.SetActive(true);
                    var bhVolume = blackHoleVolume.GetComponent<BlackHoleVolume>();
                    bhVolume._audioSector = sector;
                    bhVolume._emissionSource = oneShotOWAudioSource;
                    var blackHoleSphereCollider = blackHoleVolume.GetComponent<SphereCollider>();
                    blackHoleSphereCollider.radius = horizon;
                    if (sizeController != null) sizeController.sphereCollider = blackHoleSphereCollider;
                    if (!warpEffects)
                    {
                        Delay.FireOnNextUpdate(() =>
                        {
                            foreach (var renderer in blackHoleVolume.GetComponentsInChildren<ParticleSystemRenderer>(true))
                            {
                                UnityEngine.Object.Destroy(renderer);
                            } 
                        });
                    }
                }
            }
            else
            {
                /*
                var ambientLight = GameObject.Instantiate(_whiteHoleAmbientLight);
                ambientLight.transform.parent = singularity.transform;
                ambientLight.transform.localScale = Vector3.one;
                ambientLight.transform.localPosition = Vector3.zero;
                ambientLight.name = "AmbientLight";
                ambientLight.SetActive(true);
                var light = ambientLight.GetComponent<Light>();
                light.range = distort * 7f;
                if (sizeController != null) sizeController.light = light;
                */

                GameObject whiteHoleVolumeGO = GameObject.Instantiate(_whiteHoleVolume);
                whiteHoleVolumeGO.transform.parent = singularity.transform;
                whiteHoleVolumeGO.transform.localPosition = Vector3.zero;
                whiteHoleVolumeGO.transform.localScale = Vector3.one;
                whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = horizon;
                whiteHoleVolumeGO.name = "WhiteHoleVolume";
                whiteHoleVolumeGO.SetActive(true);
                if (sizeController != null) sizeController.sphereCollider = whiteHoleVolumeGO.GetComponent<SphereCollider>();

                var OWRB = planetGO.GetComponent<OWRigidbody>();

                var whiteHoleFluidVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleFluidVolume>();
                whiteHoleFluidVolume._innerRadius = horizon;
                whiteHoleFluidVolume._outerRadius = distort;
                whiteHoleFluidVolume._attachedBody = OWRB;
                if (sizeController != null) sizeController.fluidVolume = whiteHoleFluidVolume;

                var whiteHoleVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleVolume>();
                whiteHoleVolume._debrisDistMax = distort * 6.5f;
                whiteHoleVolume._debrisDistMin = distort * 2f;
                whiteHoleVolume._whiteHoleSector = sector;
                whiteHoleVolume._fluidVolume = whiteHoleFluidVolume;
                whiteHoleVolume._whiteHoleBody = OWRB;
                whiteHoleVolume._whiteHoleProxyShadowSuperGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
                whiteHoleVolume._radius = horizon;
                whiteHoleVolume._airlocksToOpen = new NomaiAirlock[0];
                if (sizeController != null) sizeController.volume = whiteHoleVolume;

                whiteHoleVolume.enabled = true;
                whiteHoleFluidVolume.enabled = true;
            }

            singularity.SetActive(true);
            return singularity;
        }
        
        public static MeshRenderer MakeSingularityGraphics(GameObject singularity, bool polarity, float horizon, float distort, int queue = 2985)
        {
            InitPrefabs();

            var singularityRenderer = new GameObject(polarity ? "BlackHoleRenderer" : "WhiteHoleRenderer");
            singularityRenderer.transform.parent = singularity.transform;
            singularityRenderer.transform.localPosition = Vector3.zero;
            singularityRenderer.transform.localScale = Vector3.one * distort;

            var meshFilter = singularityRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = polarity ? _blackHoleMesh : _whiteHoleMesh;

            var meshRenderer = singularityRenderer.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(polarity ? blackHoleShader : whiteHoleShader);
            meshRenderer.material.SetFloat(Radius, horizon);
            meshRenderer.material.SetFloat(MaxDistortRadius, distort);
            meshRenderer.material.SetFloat(MassScale, polarity ? 1f : -1f);
            meshRenderer.material.SetFloat(DistortFadeDist, distort - horizon);
            if (!polarity) meshRenderer.material.SetColor(Color1, new Color(1.88f, 1.88f, 1.88f, 1f));
            meshRenderer.material.renderQueue = queue;

            return meshRenderer;
        }

        public static GameObject MakeSingularityProxy(GameObject rootObject, MVector3 position, bool polarity, float horizon, float distort, TimeValuePair[] curve = null, int queue = 2985)
        {
            InitPrefabs();

            var singularityRenderer = MakeSingularityGraphics(rootObject, polarity, horizon, distort, queue);
            if (position != null) singularityRenderer.transform.localPosition = position;

            SingularitySizeController sizeController = null;
            if (curve != null)
            {
                sizeController = singularityRenderer.gameObject.AddComponent<SingularitySizeController>();
                sizeController.SetScaleCurve(curve);
                sizeController.size = distort;
                sizeController.innerScale = horizon;
                sizeController.material = singularityRenderer.material;
            }

            singularityRenderer.gameObject.SetActive(true);
            return singularityRenderer.gameObject;
        }
    }
}
