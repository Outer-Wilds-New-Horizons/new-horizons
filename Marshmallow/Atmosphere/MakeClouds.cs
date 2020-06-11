using Marshmallow.External;
using Newtonsoft.Json.Linq;
using OWML.ModHelper.Events;
using UnityEngine;

namespace Marshmallow.Atmosphere
{
    static class MakeClouds
    {
        public static void Make(GameObject body, Sector sector, IPlanetConfig config)
        {
            GameObject cloudsMainGO = new GameObject();
            cloudsMainGO.SetActive(false);
            cloudsMainGO.transform.parent = body.transform;

            GameObject cloudsTopGO = new GameObject();
            cloudsTopGO.SetActive(false);
            cloudsTopGO.transform.parent = cloudsMainGO.transform;
            cloudsTopGO.transform.localScale = new Vector3(config.TopCloudSize / 2, config.TopCloudSize / 2, config.TopCloudSize / 2);

            MeshFilter topMF = cloudsTopGO.AddComponent<MeshFilter>();
            topMF.mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;

            MeshRenderer topMR = cloudsTopGO.AddComponent<MeshRenderer>();
            topMR.materials = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshRenderer>().materials;

            foreach (var material in topMR.materials)
            {
                material.SetColor("_Color", config.TopCloudTint.ToColor32());
            }

            RotateTransform topRT = cloudsTopGO.AddComponent<RotateTransform>();
            topRT.SetValue("_localAxis", Vector3.up);
            topRT.SetValue("degreesPerSecond", 10);
            topRT.SetValue("randomizeRotationRate", false);

            /*
            SectorCullGroup scg = cloudsTop.AddComponent<SectorCullGroup>();
            scg.SetValue("_sector", MainClass.SECTOR);
            scg.SetValue("_occlusionCulling", true);
            scg.SetValue("_dynamicCullingBounds", false);
            scg.SetValue("_particleSystemSuspendMode", CullGroup.ParticleSystemSuspendMode.Pause);
            scg.SetValue("_waitForStreaming", false);
            */

            GameObject cloudsBottomGO = new GameObject();
            cloudsBottomGO.SetActive(false);
            cloudsBottomGO.transform.parent = cloudsMainGO.transform;
            cloudsBottomGO.transform.localScale = new Vector3(config.BottomCloudSize / 2, config.BottomCloudSize / 2, config.BottomCloudSize / 2);

            TessellatedSphereRenderer bottomTSR = cloudsBottomGO.AddComponent<TessellatedSphereRenderer>();
            bottomTSR.tessellationMeshGroup = GameObject.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            bottomTSR.sharedMaterials = GameObject.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().sharedMaterials;
            bottomTSR.maxLOD = 6;
            bottomTSR.LODBias = 0;
            bottomTSR.LODRadius = 1f;

            foreach (Material material in bottomTSR.sharedMaterials)
            {
                material.SetColor("_Color", config.BottomCloudTint.ToColor32());
            }

            TessSphereSectorToggle bottomTSST = cloudsBottomGO.AddComponent<TessSphereSectorToggle>();
            bottomTSST.SetValue("_sector", sector);

            GameObject cloudsFluidGO = new GameObject();
            cloudsFluidGO.SetActive(false);
            cloudsFluidGO.layer = 17;
            cloudsFluidGO.transform.parent = cloudsMainGO.transform;

            SphereCollider fluidSC = cloudsFluidGO.AddComponent<SphereCollider>();
            fluidSC.isTrigger = true;
            fluidSC.radius = config.TopCloudSize / 2;

            OWShellCollider fluidOWSC = cloudsFluidGO.AddComponent<OWShellCollider>();
            fluidOWSC.SetValue("_innerRadius", config.BottomCloudSize);

            CloudLayerFluidVolume fluidCLFV = cloudsFluidGO.AddComponent<CloudLayerFluidVolume>();
            fluidCLFV.SetValue("_layer", 5);
            fluidCLFV.SetValue("_priority", 1);
            fluidCLFV.SetValue("_density", 1.2f);
            fluidCLFV.SetValue("_fluidType", FluidVolume.Type.CLOUD);
            fluidCLFV.SetValue("_allowShipAutoroll", true);
            fluidCLFV.SetValue("_disableOnStart", false);

            cloudsTopGO.SetActive(true);
            cloudsBottomGO.SetActive(true);
            cloudsFluidGO.SetActive(true);
            cloudsMainGO.SetActive(true);
        }
    }
}
