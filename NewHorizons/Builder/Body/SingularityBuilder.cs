using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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

        public static void Make(GameObject body, Sector sector, OWRigidbody OWRB, IPlanetConfig config)
        {

            var size = config.Base.BlackHoleSize;
            string pairedSingularity = null;
            var polarity = Polarity.BlackHole;

            if (config.Singularity != null)
            {
                size = config.Singularity.Size;
                pairedSingularity = config.Singularity.PairedSingularity;
                if(config.Singularity.Type != null && config.Singularity.Type.ToUpper().Equals("WHITEHOLE"))
                {
                    polarity = Polarity.WhiteHole;
                }
            }
            bool hasHazardVolume = pairedSingularity == null;

            GameObject newSingularity = null;
            switch (polarity)
            {
                case Polarity.BlackHole:
                    newSingularity = MakeBlackHole(body, sector, size, hasHazardVolume);
                    break;
                case Polarity.WhiteHole:
                    newSingularity = MakeWhiteHole(body, sector, OWRB, size);
                    break;
            }

            // Try to pair them
            if(pairedSingularity != null && newSingularity != null)
            {
                var pairedSingularityAO = AstroObjectLocator.GetAstroObject(pairedSingularity);
                if(pairedSingularityAO != null)
                {
                    Logger.Log($"Pairing singularities {pairedSingularity}, {config.Name}");
                    try
                    {
                        switch (polarity)
                        {
                            case Polarity.BlackHole:
                                newSingularity.GetComponentInChildren<BlackHoleVolume>()._whiteHole = pairedSingularityAO.GetComponentInChildren<WhiteHoleVolume>();
                                break;
                            case Polarity.WhiteHole:
                                pairedSingularityAO.GetComponentInChildren<BlackHoleVolume>()._whiteHole = newSingularity.GetComponentInChildren<WhiteHoleVolume>();
                                break;
                        }
                    }
                    catch(Exception)
                    {
                        Logger.LogError($"Couldn't pair singularities {pairedSingularity}, {config.Name}");
                    }
                }
            }
        }

        private static GameObject MakeBlackHole(GameObject body, Sector sector, float size, bool hasDestructionVolume)
        {
            var blackHole = new GameObject("BlackHole");
            blackHole.SetActive(false);
            blackHole.transform.parent = body.transform;
            blackHole.transform.localPosition = Vector3.zero;

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

            if(hasDestructionVolume)
            {
                var destructionVolumeGO = new GameObject("DestructionVolume");
                destructionVolumeGO.layer = LayerMask.NameToLayer("BasicEffectVolume");
                destructionVolumeGO.transform.parent = blackHole.transform;
                destructionVolumeGO.transform.localScale = Vector3.one;
                destructionVolumeGO.transform.localPosition = Vector3.zero;

                var sphereCollider = destructionVolumeGO.AddComponent<SphereCollider>();
                sphereCollider.radius = size * 0.4f;
                sphereCollider.isTrigger = true;

                destructionVolumeGO.AddComponent<BlackHoleDestructionVolume>();
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

        private static GameObject MakeWhiteHole(GameObject body, Sector sector, OWRigidbody OWRB, float size)
        {
            var whiteHole = new GameObject("WhiteHole");
            whiteHole.SetActive(false);
            whiteHole.transform.parent = body.transform;
            whiteHole.transform.localPosition = Vector3.zero;

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
            meshRenderer.sharedMaterial.SetColor("_Color", new Color(1.88f, 1.88f, 1.88f, 1f));

            var ambientLight = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/WhiteHoleVisuals/AmbientLight_WH"));
            ambientLight.transform.parent = whiteHole.transform;
            ambientLight.transform.localScale = Vector3.one;
            ambientLight.transform.localPosition = Vector3.zero;
            ambientLight.name = "AmbientLight";
            ambientLight.GetComponent<Light>().range = size * 7f;

            var proxyShadow = sector.gameObject.AddComponent<ProxyShadowCasterSuperGroup>();

            // it's going to complain 
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
            whiteHoleVolume._whiteHoleProxyShadowSuperGroup = proxyShadow;

            whiteHoleVolumeGO.GetComponent<SphereCollider>().radius = size;

            whiteHoleVolume.enabled = true;
            whiteHoleFluidVolume.enabled = true;

            var zeroGVolume = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/ZeroGVolume"), whiteHole.transform);
            zeroGVolume.name = "ZeroGVolume";
            zeroGVolume.GetComponent<SphereCollider>().radius = size * 10f;
            zeroGVolume.GetComponent<ZeroGVolume>()._attachedBody = OWRB;

            var rulesetVolume = GameObject.Instantiate(GameObject.Find("WhiteHole_Body/Sector_WhiteHole/RulesetVolumes_WhiteHole"), sector.transform);
            rulesetVolume.name = "RulesetVolume";
            rulesetVolume.transform.localPosition = Vector3.zero;
            rulesetVolume.transform.localScale = Vector3.one * size / 100f;
            rulesetVolume.GetComponent<SphereShape>().enabled = true;

            whiteHole.SetActive(true);
            return whiteHole;
        }
    }
}
