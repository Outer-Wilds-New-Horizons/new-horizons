using NewHorizons.Components;
using NewHorizons.External.Configs;
using NewHorizons.Utility;
using System;
using NewHorizons.External.Modules.VariableSize;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using System.Collections.Generic;
using System.Linq;

namespace NewHorizons.Builder.Body
{
    public static class SingularityBuilder
    {

        private static Shader blackHoleShader = null;
        private static Shader whiteHoleShader = null;
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int MaxDistortRadius = Shader.PropertyToID("_MaxDistortRadius");
        private static readonly int MassScale = Shader.PropertyToID("_MassScale");
        private static readonly int DistortFadeDist = Shader.PropertyToID("_DistortFadeDist");
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        private static Dictionary<string, GameObject> _singularitiesByID;

        public static void Make(GameObject go, Sector sector, OWRigidbody OWRB, PlanetConfig config, SingularityModule singularity)
        {
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
                    newSingularity = MakeBlackHole(go, sector, localPosition, size, hasHazardVolume, singularity.targetStarSystem);
                    break;
                case SingularityModule.SingularityType.WhiteHole:
                    newSingularity = MakeWhiteHole(go, sector, OWRB, localPosition, size, makeZeroGVolume);
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

        public static GameObject MakeBlackHole(GameObject planetGO, Sector sector, Vector3 localPosition, float size, bool hasDestructionVolume, string targetSolarSystem, bool makeAudio = true)
        {
            var blackHole = new GameObject("BlackHole");
            blackHole.SetActive(false);
            blackHole.transform.parent = sector?.transform ?? planetGO.transform;
            blackHole.transform.position = planetGO.transform.TransformPoint(localPosition);

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = blackHole.transform;
            blackHoleRender.transform.localPosition = Vector3.zero;
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            if (blackHoleShader == null) blackHoleShader = SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat(Radius, size * 0.4f);
            meshRenderer.material.SetFloat(MaxDistortRadius, size * 0.95f);
            meshRenderer.material.SetFloat(MassScale, 1);
            meshRenderer.material.SetFloat(DistortFadeDist, size * 0.55f);

            if (makeAudio)
            {
                var blackHoleAmbience = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleAmbience"), blackHole.transform);
                blackHoleAmbience.name = "BlackHoleAmbience";
                blackHoleAmbience.GetComponent<SectorAudioGroup>().SetSector(sector);

                var blackHoleAudioSource = blackHoleAmbience.GetComponent<AudioSource>();
                blackHoleAudioSource.maxDistance = size * 2.5f;
                blackHoleAudioSource.minDistance = size * 0.4f;
                blackHoleAmbience.transform.localPosition = Vector3.zero;

                var blackHoleOneShot = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleEmissionOneShot"), blackHole.transform);
                var oneShotAudioSource = blackHoleOneShot.GetComponent<AudioSource>();
                oneShotAudioSource.maxDistance = size * 3f;
                oneShotAudioSource.minDistance = size * 0.4f;
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

                if (hasDestructionVolume) destructionVolumeGO.AddComponent<BlackHoleDestructionVolume>();
                else if (targetSolarSystem != null)
                {
                    var wormholeVolume = destructionVolumeGO.AddComponent<ChangeStarSystemVolume>();
                    wormholeVolume.TargetSolarSystem = targetSolarSystem;
                }
            }
            else
            {
                var blackHoleVolume = GameObject.Instantiate(SearchUtilities.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleVolume"), blackHole.transform);
                blackHoleVolume.name = "BlackHoleVolume";
                blackHoleVolume.GetComponent<SphereCollider>().radius = size * 0.4f;
            }

            blackHole.SetActive(true);
            return blackHole;
        }

        public static GameObject MakeWhiteHole(GameObject planetGO, Sector sector, OWRigidbody OWRB, Vector3 localPosition, float size, bool makeZeroGVolume = true)
        {
            var whiteHole = new GameObject("WhiteHole");
            whiteHole.SetActive(false);
            whiteHole.transform.parent = sector?.transform ?? planetGO.transform;
            whiteHole.transform.position = planetGO.transform.TransformPoint(localPosition);

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = whiteHole.transform;
            whiteHoleRenderer.transform.localPosition = Vector3.zero;
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = SearchUtilities.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshFilter>().mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            if (whiteHoleShader == null) whiteHoleShader = SearchUtilities.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat(Radius, size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat(DistortFadeDist, size);
            meshRenderer.sharedMaterial.SetFloat(MaxDistortRadius, size * 2.8f);
            meshRenderer.sharedMaterial.SetFloat(MassScale, -1);
            meshRenderer.sharedMaterial.SetColor(Color1, new Color(1.88f, 1.88f, 1.88f, 1f));

            var ambientLight = GameObject.Instantiate(SearchUtilities.Find("WhiteHole_Body/WhiteHoleVisuals/AmbientLight_WH"));
            ambientLight.transform.parent = whiteHole.transform;
            ambientLight.transform.localScale = Vector3.one;
            ambientLight.transform.localPosition = Vector3.zero;
            ambientLight.name = "AmbientLight";
            ambientLight.GetComponent<Light>().range = size * 7f;

            GameObject whiteHoleVolumeGO = GameObject.Instantiate(SearchUtilities.Find("WhiteHole_Body/WhiteHoleVolume"));
            whiteHoleVolumeGO.transform.parent = whiteHole.transform;
            whiteHoleVolumeGO.transform.localPosition = Vector3.zero;
            whiteHoleVolumeGO.transform.localScale = Vector3.one;
            whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = size;
            whiteHoleVolumeGO.name = "WhiteHoleVolume";

            var whiteHoleFluidVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleFluidVolume>();
            whiteHoleFluidVolume._innerRadius = size * 0.5f;
            whiteHoleFluidVolume._outerRadius = size;
            whiteHoleFluidVolume._attachedBody = OWRB;

            var whiteHoleVolume = whiteHoleVolumeGO.GetComponent<WhiteHoleVolume>();
            whiteHoleVolume._debrisDistMax = size * 6.5f;
            whiteHoleVolume._debrisDistMin = size * 2f;
            whiteHoleVolume._whiteHoleSector = sector;
            whiteHoleVolume._fluidVolume = whiteHoleFluidVolume;
            whiteHoleVolume._whiteHoleBody = OWRB;
            whiteHoleVolume._whiteHoleProxyShadowSuperGroup = planetGO.GetComponent<ProxyShadowCasterSuperGroup>();
            whiteHoleVolume._radius = size * 0.5f;

            whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = size;

            whiteHoleVolume.enabled = true;
            whiteHoleFluidVolume.enabled = true;

            if (makeZeroGVolume)
            {
                var zeroGVolume = GameObject.Instantiate(SearchUtilities.Find("WhiteHole_Body/ZeroGVolume"), whiteHole.transform);
                zeroGVolume.name = "ZeroGVolume";
                zeroGVolume.transform.localPosition = Vector3.zero;
                zeroGVolume.GetComponent<SphereCollider>().radius = size * 10f;
                zeroGVolume.GetComponent<ZeroGVolume>()._attachedBody = OWRB;

                var rulesetVolume = GameObject.Instantiate(SearchUtilities.Find("WhiteHole_Body/Sector_WhiteHole/RulesetVolumes_WhiteHole"), planetGO.transform);
                rulesetVolume.name = "RulesetVolume";
                rulesetVolume.transform.localPosition = Vector3.zero;
                rulesetVolume.transform.localScale = Vector3.one * size / 100f;
                rulesetVolume.GetComponent<SphereShape>().enabled = true;
            }

            whiteHole.SetActive(true);
            return whiteHole;
        }
    }
}
