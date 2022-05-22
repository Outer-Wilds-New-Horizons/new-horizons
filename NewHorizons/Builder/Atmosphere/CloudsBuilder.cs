using NewHorizons.External.Modules;
using NewHorizons.Utility;
using OWML.Common;
using System;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
namespace NewHorizons.Builder.Atmosphere
{
    public static class CloudsBuilder
    {
        private static Shader _sphereShader = null;
        private static Material[] _gdCloudMaterials;
        private static GameObject _lightningPrefab;
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private static readonly int TintColor = Shader.PropertyToID("_TintColor");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int RampTex = Shader.PropertyToID("_RampTex");
        private static readonly int CapTex = Shader.PropertyToID("_CapTex");

        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo, IModBehaviour mod)
        {
            if (_lightningPrefab == null) _lightningPrefab = GameObject.Find("GiantsDeep_Body/Sector_GD/Clouds_GD/LightningGenerator_GD");

            GameObject cloudsMainGO = new GameObject("Clouds");
            cloudsMainGO.SetActive(false);
            cloudsMainGO.transform.parent = sector?.transform ?? planetGO.transform;

            MakeTopClouds(cloudsMainGO, atmo, mod);

            GameObject cloudsBottomGO = new GameObject("BottomClouds");
            cloudsBottomGO.SetActive(false);
            cloudsBottomGO.transform.parent = cloudsMainGO.transform;
            cloudsBottomGO.transform.localScale = Vector3.one * atmo.Clouds.InnerCloudRadius;

            TessellatedSphereRenderer bottomTSR = cloudsBottomGO.AddComponent<TessellatedSphereRenderer>();
            bottomTSR.tessellationMeshGroup = GameObject.Find("CloudsBottomLayer_QM").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            var bottomTSRMaterials = GameObject.Find("CloudsBottomLayer_QM").GetComponent<TessellatedSphereRenderer>().sharedMaterials;

            // If they set a colour apply it to all the materials else keep the default QM one
            if (atmo.Clouds.Tint != null)
            {
                var bottomColor = atmo.Clouds.Tint.ToColor();

                var bottomTSRTempArray = new Material[2];

                bottomTSRTempArray[0] = new Material(bottomTSRMaterials[0]);
                bottomTSRTempArray[0].SetColor(Color1, bottomColor);
                bottomTSRTempArray[0].SetColor(TintColor, bottomColor);

                bottomTSRTempArray[1] = new Material(bottomTSRMaterials[1]);

                bottomTSR.sharedMaterials = bottomTSRTempArray;
            }
            else
            {
                bottomTSR.sharedMaterials = bottomTSRMaterials;
            }

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

            var fluidType = FluidVolume.Type.CLOUD;
            if (!string.IsNullOrEmpty(atmo.Clouds.FluidType))
            {
                try
                {
                    fluidType = (FluidVolume.Type)Enum.Parse(typeof(FluidVolume.Type), atmo.Clouds.FluidType.ToUpper());
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Couldn't parse fluid volume type [{atmo.Clouds.FluidType}]: {ex.Message}, {ex.StackTrace}");
                }
            }

            fluidCLFV._fluidType = fluidType;
            fluidCLFV._allowShipAutoroll = true;
            fluidCLFV._disableOnStart = false;

            // Fix the rotations once the rest is done
            cloudsMainGO.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(0, 0, 0));
            // For the base shader it has to be rotated idk
            if (atmo.Clouds.UseBasicCloudShader) cloudsMainGO.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));

            // Lightning
            if (atmo.Clouds.HasLightning)
            {
                var lightning = _lightningPrefab.InstantiateInactive();
                lightning.transform.parent = cloudsMainGO.transform;
                lightning.transform.localPosition = Vector3.zero;

                var lightningGenerator = lightning.GetComponent<CloudLightningGenerator>();
                lightningGenerator._altitude = (atmo.Clouds.OuterCloudRadius + atmo.Clouds.InnerCloudRadius) / 2f;
                lightningGenerator._audioSector = sector;
                if (atmo.Clouds.LightningGradient != null)
                {
                    var gradient = new GradientColorKey[atmo.Clouds.LightningGradient.Length];

                    for(int i = 0; i < atmo.Clouds.LightningGradient.Length; i++)
                    {
                        var pair = atmo.Clouds.LightningGradient[i];
                        gradient[i] = new GradientColorKey(pair.Tint.ToColor(), pair.Time);
                    }

                    lightningGenerator._lightColor.colorKeys = gradient;
                }
                lightning.SetActive(true);
            }

            cloudsMainGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            cloudsBottomGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            cloudsFluidGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);

            cloudsBottomGO.SetActive(true);
            cloudsFluidGO.SetActive(true);
            cloudsMainGO.SetActive(true);
        }

        public static GameObject MakeTopClouds(GameObject rootObject, AtmosphereModule atmo, IModBehaviour mod)
        {
            Color cloudTint = atmo.Clouds.Tint?.ToColor() ?? Color.white;

            Texture2D image, cap, ramp;

            try
            {
                image = ImageUtilities.GetTexture(mod, atmo.Clouds.TexturePath);

                if (atmo.Clouds.CapPath == null) cap = ImageUtilities.ClearTexture(128, 128);
                else cap = ImageUtilities.GetTexture(mod, atmo.Clouds.CapPath);
                if (atmo.Clouds.RampPath == null) ramp = ImageUtilities.CanvasScaled(image, 1, image.height);
                else ramp = ImageUtilities.GetTexture(mod, atmo.Clouds.RampPath);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load Cloud textures for [{rootObject.name}], {e.Message}, {e.StackTrace}");
                return null;
            }

            GameObject cloudsTopGO = new GameObject("TopClouds");
            cloudsTopGO.SetActive(false);
            cloudsTopGO.transform.parent = rootObject.transform;
            cloudsTopGO.transform.localScale = Vector3.one * atmo.Clouds.OuterCloudRadius;

            MeshFilter topMF = cloudsTopGO.AddComponent<MeshFilter>();
            topMF.mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;

            MeshRenderer topMR = cloudsTopGO.AddComponent<MeshRenderer>();

            if (_sphereShader == null) _sphereShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapper.shader");
            if (_gdCloudMaterials == null) _gdCloudMaterials = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshRenderer>().sharedMaterials;
            var tempArray = new Material[2];

            if (atmo.Clouds.UseBasicCloudShader)
            {
                var material = new Material(_sphereShader);
                if (atmo.Clouds.Unlit) material.renderQueue = 2550;
                material.name = atmo.Clouds.Unlit ? "BasicCloud" : "BasicShadowCloud";

                tempArray[0] = material;
            }
            else
            {
                var material = new Material(_gdCloudMaterials[0]);
                if (atmo.Clouds.Unlit) material.renderQueue = 2550;
                material.name = atmo.Clouds.Unlit ? "AdvancedCloud" : "AdvancedShadowCloud";
                tempArray[0] = material;
            }

            // This is the stencil material for the fog under the clouds
            tempArray[1] = new Material(_gdCloudMaterials[1]);
            topMR.sharedMaterials = tempArray;

            foreach (var material in topMR.sharedMaterials)
            {
                material.SetColor(Color1, cloudTint);
                material.SetColor(TintColor, cloudTint);

                material.SetTexture(MainTex, image);
                material.SetTexture(RampTex, ramp);
                material.SetTexture(CapTex, cap);
            }

            if (atmo.Clouds.Unlit)
            {
                cloudsTopGO.layer = LayerMask.NameToLayer("IgnoreSun");
            }

            RotateTransform topRT = cloudsTopGO.AddComponent<RotateTransform>();
            // Idk why but the axis is weird
            topRT._localAxis = atmo.Clouds.UseBasicCloudShader ? Vector3.forward : Vector3.up;
            topRT._degreesPerSecond = 10;
            topRT._randomizeRotationRate = false;

            cloudsTopGO.transform.localPosition = Vector3.zero;

            cloudsTopGO.SetActive(true);

            return cloudsTopGO;
        }
    }
}
