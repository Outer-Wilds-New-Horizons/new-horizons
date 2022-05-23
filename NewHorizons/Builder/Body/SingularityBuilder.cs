#region

using System;
using NewHorizons.Components;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;

#endregion

namespace NewHorizons.Builder.Body
{
    public static class SingularityBuilder
    {
        private static Shader blackHoleShader;
        private static Shader whiteHoleShader;
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int MaxDistortRadius = Shader.PropertyToID("_MaxDistortRadius");
        private static readonly int MassScale = Shader.PropertyToID("_MassScale");
        private static readonly int DistortFadeDist = Shader.PropertyToID("_DistortFadeDist");
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        public static void Make(GameObject go, Sector sector, OWRigidbody OWRB, PlanetConfig config)
        {
            var size = config.Singularity.size;
            var pairedSingularity = config.Singularity.pairedSingularity;

            var polarity = config.Singularity.type;

            var isWormHole = config.Singularity?.targetStarSystem != null;
            var hasHazardVolume = !isWormHole && pairedSingularity == null;

            var makeZeroGVolume = config.Singularity == null ? true : config.Singularity.makeZeroGVolume;

            var localPosition = config.Singularity?.position == null
                ? Vector3.zero
                : (Vector3) config.Singularity.position;

            GameObject newSingularity = null;
            switch (polarity)
            {
                case SingularityModule.SingularityType.BlackHole:
                    newSingularity = MakeBlackHole(go, sector, localPosition, size, hasHazardVolume,
                        config.Singularity.targetStarSystem);
                    break;
                case SingularityModule.SingularityType.WhiteHole:
                    newSingularity = MakeWhiteHole(go, sector, OWRB, localPosition, size, makeZeroGVolume);
                    break;
            }

            // Try to pair them
            if (pairedSingularity != null && newSingularity != null)
            {
                var pairedSingularityAO = AstroObjectLocator.GetAstroObject(pairedSingularity);
                if (pairedSingularityAO != null)
                    switch (polarity)
                    {
                        case SingularityModule.SingularityType.BlackHole:
                            PairSingularities(newSingularity, pairedSingularityAO.gameObject);
                            break;
                        case SingularityModule.SingularityType.WhiteHole:
                            PairSingularities(pairedSingularityAO.gameObject, newSingularity);
                            break;
                    }
            }
        }

        public static void PairSingularities(GameObject blackHole, GameObject whiteHole)
        {
            Logger.Log($"Pairing singularities {blackHole?.name}, {whiteHole?.name}");
            try
            {
                blackHole.GetComponentInChildren<BlackHoleVolume>()._whiteHole =
                    whiteHole.GetComponentInChildren<WhiteHoleVolume>();
            }
            catch (Exception)
            {
                Logger.LogError("Couldn't pair singularities");
            }
        }

        public static GameObject MakeBlackHole(GameObject planetGO, Sector sector, Vector3 localPosition, float size,
            bool hasDestructionVolume, string targetSolarSystem, bool makeAudio = true)
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
            meshFilter.mesh = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer")
                .GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            if (blackHoleShader == null)
                blackHoleShader = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer")
                    .GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat(Radius, size * 0.4f);
            meshRenderer.material.SetFloat(MaxDistortRadius, size * 0.95f);
            meshRenderer.material.SetFloat(MassScale, 1);
            meshRenderer.material.SetFloat(DistortFadeDist, size * 0.55f);

            if (makeAudio)
            {
                var blackHoleAmbience =
                    Object.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleAmbience"),
                        blackHole.transform);
                blackHoleAmbience.name = "BlackHoleAmbience";
                blackHoleAmbience.GetComponent<SectorAudioGroup>().SetSector(sector);

                var blackHoleAudioSource = blackHoleAmbience.GetComponent<AudioSource>();
                blackHoleAudioSource.maxDistance = size * 2.5f;
                blackHoleAudioSource.minDistance = size * 0.4f;
                blackHoleAmbience.transform.localPosition = Vector3.zero;

                var blackHoleOneShot =
                    Object.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleEmissionOneShot"),
                        blackHole.transform);
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

                if (hasDestructionVolume)
                {
                    destructionVolumeGO.AddComponent<BlackHoleDestructionVolume>();
                }
                else if (targetSolarSystem != null)
                {
                    var wormholeVolume = destructionVolumeGO.AddComponent<ChangeStarSystemVolume>();
                    wormholeVolume.TargetSolarSystem = targetSolarSystem;
                }
            }
            else
            {
                var blackHoleVolume =
                    Object.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleVolume"),
                        blackHole.transform);
                blackHoleVolume.name = "BlackHoleVolume";
                blackHoleVolume.GetComponent<SphereCollider>().radius = size * 0.4f;
            }

            blackHole.SetActive(true);
            return blackHole;
        }

        public static GameObject MakeWhiteHole(GameObject planetGO, Sector sector, OWRigidbody OWRB,
            Vector3 localPosition, float size, bool makeZeroGVolume = true)
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
            meshFilter.mesh = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshFilter>()
                .mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            if (whiteHoleShader == null)
                whiteHoleShader = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity")
                    .GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat(Radius, size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat(DistortFadeDist, size);
            meshRenderer.sharedMaterial.SetFloat(MaxDistortRadius, size * 2.8f);
            meshRenderer.sharedMaterial.SetFloat(MassScale, -1);
            meshRenderer.sharedMaterial.SetColor(Color1, new Color(1.88f, 1.88f, 1.88f, 1f));

            var ambientLight = Object.Instantiate(GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/AmbientLight_WH"));
            ambientLight.transform.parent = whiteHole.transform;
            ambientLight.transform.localScale = Vector3.one;
            ambientLight.transform.localPosition = Vector3.zero;
            ambientLight.name = "AmbientLight";
            ambientLight.GetComponent<Light>().range = size * 7f;

            var whiteHoleVolumeGO = Object.Instantiate(GameObject.Find("WhiteHole_Body/WhiteHoleVolume"));
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
                var zeroGVolume =
                    Object.Instantiate(GameObject.Find("WhiteHole_Body/ZeroGVolume"), whiteHole.transform);
                zeroGVolume.name = "ZeroGVolume";
                zeroGVolume.transform.localPosition = Vector3.zero;
                zeroGVolume.GetComponent<SphereCollider>().radius = size * 10f;
                zeroGVolume.GetComponent<ZeroGVolume>()._attachedBody = OWRB;

                var rulesetVolume =
                    Object.Instantiate(GameObject.Find("WhiteHole_Body/Sector_WhiteHole/RulesetVolumes_WhiteHole"),
                        planetGO.transform);
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