using NewHorizons.Components;
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
        private static GameObject _whiteHoleZeroGVolume;
        private static GameObject _whiteHoleRulesetVolume;
        private static GameObject _whiteHoleVolume;

        internal static void InitPrefabs()
        {
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
            if (_whiteHoleZeroGVolume == null) _whiteHoleZeroGVolume = SearchUtilities.Find("WhiteHole_Body/ZeroGVolume").InstantiateInactive().Rename("WhiteHoleZeroGVolume").DontDestroyOnLoad();
            if (_whiteHoleRulesetVolume == null) _whiteHoleRulesetVolume = SearchUtilities.Find("WhiteHole_Body/Sector_WhiteHole/RulesetVolumes_WhiteHole").InstantiateInactive().Rename("WhiteHoleRulesetVolume").DontDestroyOnLoad();
            if (_whiteHoleVolume == null) _whiteHoleVolume = SearchUtilities.Find("WhiteHole_Body/WhiteHoleVolume").InstantiateInactive().Rename("WhiteHoleVolume").DontDestroyOnLoad();
        }

        public static void Make(GameObject go, Sector sector, OWRigidbody OWRB, PlanetConfig config, SingularityModule singularity)
        {
            InitPrefabs();

            // If we've reloaded the first one will now be null so we have to refresh the list
            if (_singularitiesByID?.Values?.FirstOrDefault() == null) _singularitiesByID = new Dictionary<string, GameObject>();

            var size = singularity.size;
            var pairedSingularity = singularity.pairedSingularity;

            var polarity = singularity.type;

            bool isWormHole = singularity?.targetStarSystem != null;
            bool hasHazardVolume = !isWormHole && (pairedSingularity == null);

            bool makeZeroGVolume = singularity == null ? true : singularity.makeZeroGVolume;

            Vector3 localPosition = singularity?.position == null ? Vector3.zero : singularity.position;

            GameObject newSingularity = null;
            switch (polarity)
            {
                case SingularityModule.SingularityType.BlackHole:
                    newSingularity = MakeBlackHole(go, sector, localPosition, size, hasHazardVolume, singularity.targetStarSystem, singularity.curve);
                    break;
                case SingularityModule.SingularityType.WhiteHole:
                    newSingularity = MakeWhiteHole(go, sector, OWRB, localPosition, size, singularity.curve, makeZeroGVolume);
                    break;
            }

            var uniqueID = string.IsNullOrEmpty(singularity.uniqueID) ? config.name : singularity.uniqueID;
            _singularitiesByID.Add(uniqueID, newSingularity);

            // Try to pair them
            if (!string.IsNullOrEmpty(pairedSingularity) && newSingularity != null)
            {
                if (_singularitiesByID.TryGetValue(pairedSingularity, out var pairedSingularityGO))
                {
                    switch (polarity)
                    {
                        case SingularityModule.SingularityType.BlackHole:
                            PairSingularities(uniqueID, pairedSingularity, newSingularity, pairedSingularityGO);
                            break;
                        case SingularityModule.SingularityType.WhiteHole:
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

        public static GameObject MakeBlackHole(GameObject planetGO, Sector sector, Vector3 localPosition, float size, 
            bool hasDestructionVolume, string targetSolarSystem = null, TimeValuePair[] curve = null, bool makeAudio = true)
        {
            InitPrefabs();

            var blackHole = new GameObject("BlackHole");
            blackHole.SetActive(false);
            blackHole.transform.parent = sector?.transform ?? planetGO.transform;
            blackHole.transform.position = planetGO.transform.TransformPoint(localPosition);

            var blackHoleRender = MakeBlackHoleGraphics(blackHole, size);

            BlackHoleSizeController sizeController = null;
            if (curve != null)
            {
                sizeController = blackHoleRender.gameObject.AddComponent<BlackHoleSizeController>();
                sizeController.SetScaleCurve(curve);
                sizeController.size = size;
                sizeController.material = blackHoleRender.material;
            }

            if (makeAudio)
            {
                var blackHoleAmbience = GameObject.Instantiate(_blackHoleAmbience, blackHole.transform);
                blackHoleAmbience.name = "BlackHoleAmbience";
                blackHoleAmbience.GetComponent<SectorAudioGroup>().SetSector(sector);

                var blackHoleAudioSource = blackHoleAmbience.GetComponent<AudioSource>();
                blackHoleAudioSource.maxDistance = size * 2.5f;
                blackHoleAudioSource.minDistance = size * 0.4f;
                blackHoleAmbience.transform.localPosition = Vector3.zero;
                if (sizeController != null) sizeController.audioSource = blackHoleAudioSource;

                var blackHoleOneShot = GameObject.Instantiate(_blackHoleEmissionOneShot, blackHole.transform);
                blackHoleOneShot.name = "BlackHoleEmissionOneShot";
                var oneShotAudioSource = blackHoleOneShot.GetComponent<AudioSource>();
                oneShotAudioSource.maxDistance = size * 3f;
                oneShotAudioSource.minDistance = size * 0.4f;
                if (sizeController != null) sizeController.oneShotAudioSource = oneShotAudioSource;
            }

            if (hasDestructionVolume || targetSolarSystem != null)
            {
                var destructionVolumeGO = new GameObject("DestructionVolume");
                destructionVolumeGO.layer = LayerMask.NameToLayer("BasicEffectVolume");
                destructionVolumeGO.transform.parent = blackHole.transform;
                destructionVolumeGO.transform.localScale = Vector3.one;
                destructionVolumeGO.transform.localPosition = Vector3.zero;

                var sphereCollider = destructionVolumeGO.AddComponent<SphereCollider>();
                sphereCollider.radius = size * 0.4f;
                sphereCollider.isTrigger = true;
                if (sizeController != null) sizeController.sphereCollider = sphereCollider;

                if (hasDestructionVolume) destructionVolumeGO.AddComponent<BlackHoleDestructionVolume>();
                else if (targetSolarSystem != null)
                {
                    var wormholeVolume = destructionVolumeGO.AddComponent<ChangeStarSystemVolume>();
                    wormholeVolume.TargetSolarSystem = targetSolarSystem;
                }
            }
            else
            {
                var blackHoleVolume = GameObject.Instantiate(_blackHoleVolume, blackHole.transform);
                blackHoleVolume.name = "BlackHoleVolume";
                var blackHoleSphereCollider = blackHoleVolume.GetComponent<SphereCollider>();
                blackHoleSphereCollider.radius = size * 0.4f;
                if (sizeController != null) sizeController.sphereCollider = blackHoleSphereCollider;
            }

            blackHole.SetActive(true);
            return blackHole;
        }

        public static MeshRenderer MakeBlackHoleGraphics(GameObject blackHole, float size)
        {
            InitPrefabs();

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = blackHole.transform;
            blackHoleRender.transform.localPosition = Vector3.zero;
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = _blackHoleMesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat(Radius, size * 0.4f);
            meshRenderer.material.SetFloat(MaxDistortRadius, size * 0.95f);
            meshRenderer.material.SetFloat(MassScale, 1);
            meshRenderer.material.SetFloat(DistortFadeDist, size * 0.55f);

            return meshRenderer;
        }

        public static GameObject MakeWhiteHole(GameObject planetGO, Sector sector, OWRigidbody OWRB, Vector3 localPosition, float size,
            TimeValuePair[] curve, bool makeZeroGVolume = true)
        {
            InitPrefabs();

            var whiteHole = new GameObject("WhiteHole");
            whiteHole.SetActive(false);
            whiteHole.transform.parent = sector?.transform ?? planetGO.transform;
            whiteHole.transform.position = planetGO.transform.TransformPoint(localPosition);

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = whiteHole.transform;
            whiteHoleRenderer.transform.localPosition = Vector3.zero;
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            WhiteHoleSizeController sizeController = null;
            if (curve != null)
            {
                sizeController = whiteHoleRenderer.AddComponent<WhiteHoleSizeController>();
                sizeController.SetScaleCurve(curve);
                sizeController.size = size * 2.8f; // Gets goofy because of this
            }

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = _whiteHoleMesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat(Radius, size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat(DistortFadeDist, size);
            meshRenderer.sharedMaterial.SetFloat(MaxDistortRadius, size * 2.8f);
            meshRenderer.sharedMaterial.SetFloat(MassScale, -1);
            meshRenderer.sharedMaterial.SetColor(Color1, new Color(1.88f, 1.88f, 1.88f, 1f));
            if (sizeController != null) sizeController.material = meshRenderer.sharedMaterial;

            var ambientLight = GameObject.Instantiate(_whiteHoleAmbientLight);
            ambientLight.transform.parent = whiteHole.transform;
            ambientLight.transform.localScale = Vector3.one;
            ambientLight.transform.localPosition = Vector3.zero;
            ambientLight.name = "AmbientLight";
            var light = ambientLight.GetComponent<Light>();
            light.range = size * 7f;
            if (sizeController != null) sizeController.light = light;

            GameObject whiteHoleVolumeGO = GameObject.Instantiate(_whiteHoleVolume);
            whiteHoleVolumeGO.transform.parent = whiteHole.transform;
            whiteHoleVolumeGO.transform.localPosition = Vector3.zero;
            whiteHoleVolumeGO.transform.localScale = Vector3.one;
            whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = size;
            whiteHoleVolumeGO.name = "WhiteHoleVolume";
            if (sizeController != null) sizeController.sphereCollider = whiteHoleVolumeGO.GetComponent<SphereCollider>(); 

            var whiteHoleFluidVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleFluidVolume>();
            whiteHoleFluidVolume._innerRadius = size * 0.5f;
            whiteHoleFluidVolume._outerRadius = size;
            whiteHoleFluidVolume._attachedBody = OWRB;
            if (sizeController != null) sizeController.fluidVolume = whiteHoleFluidVolume;

            var whiteHoleVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleVolume>();
            whiteHoleVolume._debrisDistMax = size * 6.5f;
            whiteHoleVolume._debrisDistMin = size * 2f;
            whiteHoleVolume._whiteHoleSector = sector;
            whiteHoleVolume._fluidVolume = whiteHoleFluidVolume;
            whiteHoleVolume._whiteHoleBody = OWRB;
            whiteHoleVolume._whiteHoleProxyShadowSuperGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            whiteHoleVolume._radius = size * 0.5f;
            if (sizeController != null) sizeController.volume = whiteHoleVolume;

            whiteHoleVolume.enabled = true;
            whiteHoleFluidVolume.enabled = true;

            if (makeZeroGVolume)
            {
                var zeroGVolume = GameObject.Instantiate(_whiteHoleZeroGVolume, whiteHole.transform);
                zeroGVolume.name = "ZeroGVolume";
                zeroGVolume.transform.localPosition = Vector3.zero;
                zeroGVolume.GetComponent<SphereCollider>().radius = size * 10f;
                zeroGVolume.GetComponent<ZeroGVolume>()._attachedBody = OWRB;

                var rulesetVolume = GameObject.Instantiate(_whiteHoleRulesetVolume, planetGO.transform);
                rulesetVolume.name = "RulesetVolume";
                rulesetVolume.transform.localPosition = Vector3.zero;
                rulesetVolume.transform.localScale = Vector3.one * size / 100f;
                rulesetVolume.GetComponent<SphereShape>().enabled = true;

                if (sizeController != null)
                {
                    sizeController.zeroGSphereCollider = zeroGVolume.GetComponent<SphereCollider>();
                    sizeController.rulesetVolume = rulesetVolume;
                }
            }

            whiteHole.SetActive(true);
            return whiteHole;
        }

        public static GameObject MakeBlackHoleProxy(GameObject rootObject, MVector3 position, float size, TimeValuePair[] curve = null)
        {
            InitPrefabs();

            var blackHoleShader = _blackHoleProxyPrefab.GetComponent<MeshRenderer>().material.shader;
            if (blackHoleShader == null) blackHoleShader = _blackHoleProxyPrefab.GetComponent<MeshRenderer>().sharedMaterial.shader;

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = rootObject.transform;
            if (position != null) blackHoleRender.transform.localPosition = position;
            else blackHoleRender.transform.localPosition = Vector3.zero;
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = _blackHoleProxyPrefab.GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat(Radius, size * 0.4f);
            meshRenderer.material.SetFloat(MaxDistortRadius, size * 0.95f);
            meshRenderer.material.SetFloat(MassScale, 1);
            meshRenderer.material.SetFloat(DistortFadeDist, size * 0.55f);

            if (curve != null) AddSizeController(blackHoleRender, curve, size);

            blackHoleRender.SetActive(true);
            return blackHoleRender;
        }

        public static GameObject MakeWhiteHoleProxy(GameObject rootObject, MVector3 position, float size, TimeValuePair[] curve = null)
        {
            InitPrefabs();

            var whiteHoleShader = _whiteHoleProxyPrefab.GetComponent<MeshRenderer>().material.shader;
            if (whiteHoleShader == null) whiteHoleShader = _whiteHoleProxyPrefab.GetComponent<MeshRenderer>().sharedMaterial.shader;

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = rootObject.transform;
            if (position != null) whiteHoleRenderer.transform.localPosition = position;
            else whiteHoleRenderer.transform.localPosition = Vector3.zero;
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = _whiteHoleProxyPrefab.GetComponent<MeshFilter>().mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat(Radius, size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat(DistortFadeDist, size);
            meshRenderer.sharedMaterial.SetFloat(MaxDistortRadius, size * 2.8f);
            meshRenderer.sharedMaterial.SetColor(Color1, new Color(1.88f, 1.88f, 1.88f, 1f));

            if (curve != null) AddSizeController(whiteHoleRenderer, curve, size);

            whiteHoleRenderer.SetActive(true);
            return whiteHoleRenderer;
        }

        private static SizeController AddSizeController(GameObject go, TimeValuePair[] curve, float size)
        {
            var sizeController = go.AddComponent<SizeController>();
            sizeController.SetScaleCurve(curve);
            sizeController.size = size;
            return sizeController;
        }
    }
}
