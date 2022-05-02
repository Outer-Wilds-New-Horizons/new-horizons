using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Atmosphere
{
    static class CloudsBuilder
    {
        private static Shader _sphereShader = null;
        public static void Make(GameObject body, Sector sector, AtmosphereModule atmo, IModBehaviour mod)
        {
            Texture2D image, cap, ramp;

            try
            {
                image = ImageUtilities.GetTexture(mod, atmo.Cloud);

                if (atmo.CloudCap == null) cap = ImageUtilities.ClearTexture(128, 128);
                else cap = ImageUtilities.GetTexture(mod, atmo.CloudCap);
                if (atmo.CloudRamp == null) ramp = ImageUtilities.CanvasScaled(image, 1, image.height);
                else ramp = ImageUtilities.GetTexture(mod, atmo.CloudRamp);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load Cloud textures, {e.Message}, {e.StackTrace}");
                return;
            }

            Color cloudTint = atmo.CloudTint == null ? Color.white : (Color)atmo.CloudTint.ToColor32();

            GameObject cloudsMainGO = new GameObject("Clouds");
            cloudsMainGO.SetActive(false);
            cloudsMainGO.transform.parent = body.transform;

            GameObject cloudsTopGO = new GameObject("TopClouds");
            cloudsTopGO.SetActive(false);
            cloudsTopGO.transform.parent = cloudsMainGO.transform;
            cloudsTopGO.transform.localScale = Vector3.one * atmo.Size;

            MeshFilter topMF = cloudsTopGO.AddComponent<MeshFilter>();
            topMF.mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;

            MeshRenderer topMR = cloudsTopGO.AddComponent<MeshRenderer>();
            if (!atmo.UseBasicCloudShader)
            {
                var tempArray = new Material[2];
                for (int i = 0; i < 2; i++)
                {
                    var mat = new Material(GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshRenderer>().sharedMaterials[i]);
                    if (!atmo.ShadowsOnClouds) mat.renderQueue = 2550;
                    mat.name = atmo.ShadowsOnClouds ? "AdvancedShadowCloud" : "AdvancedCloud";
                    tempArray[i] = mat;
                }
                topMR.sharedMaterials = tempArray;
            }
            else
            {
                if (_sphereShader == null) _sphereShader = Main.ShaderBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapper.shader");
                topMR.material = new Material(_sphereShader);
                if (!atmo.ShadowsOnClouds) topMR.material.renderQueue = 2550;
                topMR.material.name = atmo.ShadowsOnClouds ? "BasicShadowCloud" : "BasicCloud";
            }

            foreach (var material in topMR.sharedMaterials)
            {
                material.SetColor("_Color", cloudTint);
                material.SetColor("_TintColor", cloudTint);

                material.SetTexture("_MainTex", image);
                material.SetTexture("_RampTex", ramp);
                material.SetTexture("_CapTex", cap);
            }

            if(!atmo.ShadowsOnClouds)
            {
                cloudsTopGO.layer = LayerMask.NameToLayer("IgnoreSun");
            }


            RotateTransform topRT = cloudsTopGO.AddComponent<RotateTransform>();
            topRT._localAxis = Vector3.up;
            topRT._degreesPerSecond = 10;
            topRT._randomizeRotationRate = false;

            GameObject cloudsBottomGO = new GameObject("BottomClouds");
            cloudsBottomGO.SetActive(false);
            cloudsBottomGO.transform.parent = cloudsMainGO.transform;
            cloudsBottomGO.transform.localScale = Vector3.one * (atmo.Size * 0.9f);

            TessellatedSphereRenderer bottomTSR = cloudsBottomGO.AddComponent<TessellatedSphereRenderer>();
            bottomTSR.tessellationMeshGroup = GameObject.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            var bottomTSRMaterials = GameObject.Find("CloudsBottomLayer_QM").GetComponent<TessellatedSphereRenderer>().sharedMaterials;
            var bottomTSRTempArray = new Material[bottomTSRMaterials.Length];
            
            // It's a bit too green
            var bottomColor = atmo.CloudTint.ToColor32();
            bottomColor.g = (byte)(bottomColor.g * 0.5f);
            for (int i = 0; i < bottomTSRMaterials.Length; i++) 
            {
                bottomTSRTempArray[i] = new Material(bottomTSRMaterials[i]);
                bottomTSRTempArray[i].SetColor("_Color", bottomColor);
                bottomTSRTempArray[i].SetColor("_TintColor", bottomColor);
            }
            bottomTSR.sharedMaterials = bottomTSRTempArray;
            bottomTSR.maxLOD = 6;
            bottomTSR.LODBias = 0;
            bottomTSR.LODRadius = 1f;

            TessSphereSectorToggle bottomTSST = cloudsBottomGO.AddComponent<TessSphereSectorToggle>();
            bottomTSST._sector = sector;

            GameObject cloudsFluidGO = new GameObject("CloudsFluid");
            cloudsFluidGO.SetActive(false);
            cloudsFluidGO.layer = 17;
            cloudsFluidGO.transform.parent = cloudsMainGO.transform;

            SphereCollider fluidSC = cloudsFluidGO.AddComponent<SphereCollider>();
            fluidSC.isTrigger = true;
            fluidSC.radius = atmo.Size;

            OWShellCollider fluidOWSC = cloudsFluidGO.AddComponent<OWShellCollider>();
            fluidOWSC._innerRadius = atmo.Size * 0.9f;

            CloudLayerFluidVolume fluidCLFV = cloudsFluidGO.AddComponent<CloudLayerFluidVolume>();
            fluidCLFV._layer = 5;
            fluidCLFV._priority = 1;
            fluidCLFV._density = 1.2f;
            fluidCLFV._fluidType = FluidVolume.Type.CLOUD;
            fluidCLFV._allowShipAutoroll = true;
            fluidCLFV._disableOnStart = false;

            // Fix the rotations once the rest is done
            cloudsMainGO.transform.localRotation = Quaternion.Euler(0, 0, 0);
            // For the base shader it has to be rotated idk
            if(atmo.UseBasicCloudShader) cloudsMainGO.transform.localRotation = Quaternion.Euler(90, 0, 0);

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
