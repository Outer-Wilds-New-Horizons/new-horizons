using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NewHorizons.External.Configs;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    static class SingularityBuilder
    {
        enum Polarity
        {
            BlackHole,
            WhiteHole
        }

        private static Shader blackHoleShader = null;
        private static Shader whiteHoleShader = null;

        public static void Make(GameObject go, Sector sector, OWRigidbody OWRB, IPlanetConfig config)
        {
            // Backwards compatibility
            if(config.Singularity == null)
            {
                if(config.Base.BlackHoleSize != 0)
                {
                    MakeBlackHole(go, sector, Vector3.zero, config.Base.BlackHoleSize, true, null);
                }
                return;
            }

            var size = config.Singularity.Size;
            var pairedSingularity = config.Singularity.PairedSingularity;

            var polarity = Polarity.BlackHole;
            if (config.Singularity.Type != null && config.Singularity.Type.ToUpper().Equals("WHITEHOLE"))
            {
                polarity = Polarity.WhiteHole;
            }

            bool isWormHole = config.Singularity?.TargetStarSystem != null;
            bool hasHazardVolume = !isWormHole && (pairedSingularity == null);

            bool makeZeroGVolume = config.Singularity == null ? true : config.Singularity.MakeZeroGVolume;

            Vector3 localPosition = config.Singularity?.Position == null ? Vector3.zero : (Vector3)config.Singularity.Position;

            GameObject newSingularity = null;
            switch (polarity)
            {
                case Polarity.BlackHole:
                    newSingularity = MakeBlackHole(go, sector, localPosition, size, hasHazardVolume, config.Singularity.TargetStarSystem);
                    break;
                case Polarity.WhiteHole:
                    newSingularity = MakeWhiteHole(go, sector, OWRB, localPosition, size, makeZeroGVolume);
                    break;
            }

            // Try to pair them
            if(pairedSingularity != null && newSingularity != null)
            {
                var pairedSingularityAO = AstroObjectLocator.GetAstroObject(pairedSingularity);
                if(pairedSingularityAO != null)
                {
                    switch (polarity)
                    {
                        case Polarity.BlackHole:
                            PairSingularities(newSingularity, pairedSingularityAO.gameObject);
                            break;
                        case Polarity.WhiteHole:
                            PairSingularities(pairedSingularityAO.gameObject, newSingularity);
                            break;
                    }
                }
            }
        }

        public static void PairSingularities(GameObject blackHole, GameObject whiteHole)
        {
            Logger.Log($"Pairing singularities {blackHole?.name}, {whiteHole?.name}");
            try
            {
                blackHole.GetComponentInChildren<BlackHoleVolume>()._whiteHole = whiteHole.GetComponentInChildren<WhiteHoleVolume>();
            }
            catch (Exception)
            {
                Logger.LogError($"Couldn't pair singularities");
            }
        }

        public static GameObject MakeBlackHole(GameObject go, Sector sector, Vector3 localPosition, float size, bool hasDestructionVolume, string targetSolarSystem, bool makeAudio = true)
        {
            var blackHole = new GameObject("BlackHole");
            blackHole.SetActive(false);
            blackHole.transform.parent = sector?.transform ?? go.transform;
            blackHole.transform.localPosition = localPosition;

            var blackHoleRender = new GameObject("BlackHoleRender");
            blackHoleRender.transform.parent = blackHole.transform;
            blackHoleRender.transform.localPosition = Vector3.zero;
            blackHoleRender.transform.localScale = Vector3.one * size;

            var meshFilter = blackHoleRender.AddComponent<MeshFilter>();
            meshFilter.mesh = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshFilter>().mesh;

            var meshRenderer = blackHoleRender.AddComponent<MeshRenderer>();
            if (blackHoleShader == null) blackHoleShader = GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleRenderer").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(blackHoleShader);
            meshRenderer.material.SetFloat("_Radius", size * 0.4f);
            meshRenderer.material.SetFloat("_MaxDistortRadius", size * 0.95f);
            meshRenderer.material.SetFloat("_MassScale", 1);
            meshRenderer.material.SetFloat("_DistortFadeDist", size * 0.55f);

            if(makeAudio)
            {
                var blackHoleAmbience = GameObject.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleAmbience"), blackHole.transform);
                blackHoleAmbience.name = "BlackHoleAmbience";
                blackHoleAmbience.GetComponent<SectorAudioGroup>().SetSector(sector);

                var blackHoleAudioSource = blackHoleAmbience.GetComponent<AudioSource>();
                blackHoleAudioSource.maxDistance = size * 2.5f;
                blackHoleAudioSource.minDistance = size * 0.4f;
                blackHoleAmbience.transform.localPosition = Vector3.zero;

                var blackHoleOneShot = GameObject.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleEmissionOneShot"), blackHole.transform);
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
                var blackHoleVolume = GameObject.Instantiate(GameObject.Find("BrittleHollow_Body/BlackHole_BH/BlackHoleVolume"), blackHole.transform);
                blackHoleVolume.name = "BlackHoleVolume";
                blackHoleVolume.GetComponent<SphereCollider>().radius = size * 0.4f;
            }

            blackHole.SetActive(true);
            return blackHole;
        }

        public static GameObject MakeWhiteHole(GameObject body, Sector sector, OWRigidbody OWRB, Vector3 localPosition, float size, bool makeZeroGVolume = true)
        {
            var whiteHole = new GameObject("WhiteHole");
            whiteHole.SetActive(false);
            whiteHole.transform.parent = sector?.transform ?? body.transform;
            whiteHole.transform.localPosition = localPosition;

            var whiteHoleRenderer = new GameObject("WhiteHoleRenderer");
            whiteHoleRenderer.transform.parent = whiteHole.transform;
            whiteHoleRenderer.transform.localPosition = Vector3.zero;
            whiteHoleRenderer.transform.localScale = Vector3.one * size * 2.8f;

            var meshFilter = whiteHoleRenderer.AddComponent<MeshFilter>();
            meshFilter.mesh = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshFilter>().mesh;

            var meshRenderer = whiteHoleRenderer.AddComponent<MeshRenderer>();
            if (whiteHoleShader == null) whiteHoleShader = GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/Singularity").GetComponent<MeshRenderer>().sharedMaterial.shader;
            meshRenderer.material = new Material(whiteHoleShader);
            meshRenderer.sharedMaterial.SetFloat("_Radius", size * 0.4f);
            meshRenderer.sharedMaterial.SetFloat("_DistortFadeDist", size);
            meshRenderer.sharedMaterial.SetFloat("_MaxDistortRadius", size * 2.8f);
            meshRenderer.sharedMaterial.SetFloat("_MassScale", -1);
            meshRenderer.sharedMaterial.SetColor("_Color", new Color(1.88f, 1.88f, 1.88f, 1f));

            var ambientLight = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/AmbientLight_WH"));
            ambientLight.transform.parent = whiteHole.transform;
            ambientLight.transform.localScale = Vector3.one;
            ambientLight.transform.localPosition = Vector3.zero;
            ambientLight.name = "AmbientLight";
            ambientLight.GetComponent<Light>().range = size * 7f;

            GameObject whiteHoleVolumeGO  = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/WhiteHoleVolume"));
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
            whiteHoleVolume._whiteHoleProxyShadowSuperGroup = body.GetComponent<ProxyShadowCasterSuperGroup>();
            whiteHoleVolume._radius = size * 0.5f;
            
            whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = size;

            whiteHoleVolume.enabled = true;
            whiteHoleFluidVolume.enabled = true;

            if(makeZeroGVolume)
            {
                var zeroGVolume = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/ZeroGVolume"), whiteHole.transform);
                zeroGVolume.name = "ZeroGVolume";
                zeroGVolume.transform.localPosition = Vector3.zero;
                zeroGVolume.GetComponent<SphereCollider>().radius = size * 10f;
                zeroGVolume.GetComponent<ZeroGVolume>()._attachedBody = OWRB;

                var rulesetVolume = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/Sector_WhiteHole/RulesetVolumes_WhiteHole"), body.transform);
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
