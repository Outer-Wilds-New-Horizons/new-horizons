using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Atmosphere
{
    static class CloudsBuilder
    {
        public static void Make(GameObject body, Sector sector, AtmosphereModule atmo, IModAssets assets)
        {
            Texture2D image, cap, ramp;

            try
            {
                image = assets.GetTexture(atmo.Cloud);

                if (atmo.CloudCap == null) cap = ImageUtilities.ClearTexture(128, 128);
                else cap = assets.GetTexture(atmo.CloudCap);
                if(atmo.CloudRamp == null) ramp = ImageUtilities.CanvasScaled(image, 1, image.height);
                else ramp = assets.GetTexture(atmo.CloudRamp);
            }
            catch(Exception e)
            {
                Logger.LogError($"Couldn't load Cloud textures, {e.Message}, {e.StackTrace}");
                return;
            }

            Color cloudTint = atmo.CloudTint == null ? Color.white : (Color)atmo.CloudTint.ToColor32();

            GameObject cloudsMainGO = new GameObject();
            cloudsMainGO.SetActive(false);
            cloudsMainGO.transform.parent = body.transform;
            cloudsMainGO.name = "Clouds";

            GameObject cloudsTopGO = new GameObject();
            cloudsTopGO.SetActive(false);
            cloudsTopGO.transform.parent = cloudsMainGO.transform;
            cloudsTopGO.transform.localScale = Vector3.one * atmo.Size;
            cloudsTopGO.name = "TopClouds";

            MeshFilter topMF = cloudsTopGO.AddComponent<MeshFilter>();
            topMF.mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;

            var tempArray = new Material[2];
            MeshRenderer topMR = cloudsTopGO.AddComponent<MeshRenderer>();
            for (int i = 0; i < 2; i++)
            {
                tempArray[i] = GameObject.Instantiate(GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshRenderer>().sharedMaterials[i]);
            }
            topMR.sharedMaterials = tempArray;

            
            foreach (var material in topMR.sharedMaterials)
            {
                material.SetColor("_Color", cloudTint);
                material.SetColor("_TintColor", cloudTint);

                material.SetTexture("_MainTex", image);
                material.SetTexture("_RampTex", ramp);
                material.SetTexture("_CapTex", cap);
            }
            

            RotateTransform topRT = cloudsTopGO.AddComponent<RotateTransform>();
            topRT.SetValue("_localAxis", Vector3.up);
            topRT.SetValue("degreesPerSecond", 10);
            topRT.SetValue("randomizeRotationRate", false);

            GameObject cloudsBottomGO = new GameObject();
            cloudsBottomGO.SetActive(false);
            cloudsBottomGO.transform.parent = cloudsMainGO.transform;
            cloudsBottomGO.transform.localScale = Vector3.one * (atmo.Size * 0.9f);
            cloudsBottomGO.name = "BottomClouds";

            TessellatedSphereRenderer bottomTSR = cloudsBottomGO.AddComponent<TessellatedSphereRenderer>();
            bottomTSR.tessellationMeshGroup = GameObject.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            bottomTSR.sharedMaterials = GameObject.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().sharedMaterials;
            bottomTSR.maxLOD = 6;
            bottomTSR.LODBias = 0;
            bottomTSR.LODRadius = 1f;

            // It's always more green than expected
            var bottomCloudTint = cloudTint;
            bottomCloudTint.g = (byte)(bottomCloudTint.g * 0.8f);
            foreach (Material material in bottomTSR.sharedMaterials)
            {
                material.SetColor("_Color", bottomCloudTint);
                material.SetColor("_TintColor", bottomCloudTint);
            }

            TessSphereSectorToggle bottomTSST = cloudsBottomGO.AddComponent<TessSphereSectorToggle>();
            bottomTSST.SetValue("_sector", sector);

            GameObject cloudsFluidGO = new GameObject();
            cloudsFluidGO.SetActive(false);
            cloudsFluidGO.layer = 17;
            cloudsFluidGO.transform.parent = cloudsMainGO.transform;
            cloudsFluidGO.name = "CloudsFluid";

            SphereCollider fluidSC = cloudsFluidGO.AddComponent<SphereCollider>();
            fluidSC.isTrigger = true;
            fluidSC.radius = atmo.Size;

            OWShellCollider fluidOWSC = cloudsFluidGO.AddComponent<OWShellCollider>();
            fluidOWSC.SetValue("_innerRadius", atmo.Size * 0.9f);

            CloudLayerFluidVolume fluidCLFV = cloudsFluidGO.AddComponent<CloudLayerFluidVolume>();
            fluidCLFV.SetValue("_layer", 5);
            fluidCLFV.SetValue("_priority", 1);
            fluidCLFV.SetValue("_density", 1.2f);
            fluidCLFV.SetValue("_fluidType", FluidVolume.Type.CLOUD);
            fluidCLFV.SetValue("_allowShipAutoroll", true);
            fluidCLFV.SetValue("_disableOnStart", false);

            // Fix the rotations once the rest is done
            cloudsMainGO.transform.localRotation = Quaternion.Euler(0, 0, 0);

            cloudsMainGO.transform.localPosition = Vector3.zero;
            cloudsBottomGO.transform.localPosition = Vector3.zero;
            cloudsFluidGO.transform.localPosition = Vector3.zero;
            cloudsTopGO.transform.localPosition = Vector3.zero;

            cloudsTopGO.SetActive(true);
            cloudsBottomGO.SetActive(true);
            cloudsFluidGO.SetActive(true);
            cloudsMainGO.SetActive(true);
        }
    }
}
