using NewHorizons.External.Modules;
using NewHorizons.Components;
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
        private static Material[] _qmCloudMaterials;
        private static GameObject _lightningPrefab;
        private static Texture2D _colorRamp;
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int RampTex = Shader.PropertyToID("_RampTex");
        private static readonly int CapTex = Shader.PropertyToID("_CapTex");

        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo, bool cloaked, IModBehaviour mod)
        {
            if (_lightningPrefab == null) _lightningPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Clouds_GD/LightningGenerator_GD");
            if (_colorRamp == null) _colorRamp = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Clouds_Bottom_ramp.png");

            GameObject cloudsMainGO = new GameObject("Clouds");
            cloudsMainGO.SetActive(false);
            cloudsMainGO.transform.parent = sector?.transform ?? planetGO.transform;

            MakeTopClouds(cloudsMainGO, atmo, mod);

            GameObject cloudsBottomGO = new GameObject("BottomClouds");
            cloudsBottomGO.SetActive(false);
            cloudsBottomGO.transform.parent = cloudsMainGO.transform;
            cloudsBottomGO.transform.localScale = Vector3.one * atmo.clouds.innerCloudRadius;

            TessellatedSphereRenderer bottomTSR = cloudsBottomGO.AddComponent<TessellatedSphereRenderer>();
            bottomTSR.tessellationMeshGroup = SearchUtilities.Find("CloudsBottomLayer_QM").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            var bottomTSRMaterials = SearchUtilities.Find("CloudsBottomLayer_QM").GetComponent<TessellatedSphereRenderer>().sharedMaterials;

            // If they set a colour apply it to all the materials else keep the default QM one
            if (atmo.clouds.tint != null)
            {
                var bottomColor = atmo.clouds.tint.ToColor();

                var bottomTSRTempArray = new Material[2];

                bottomTSRTempArray[0] = new Material(bottomTSRMaterials[0]);
                bottomTSRTempArray[0].SetColor(Color, bottomColor);
                bottomTSRTempArray[0].SetTexture(ColorRamp, ImageUtilities.TintImage(_colorRamp, bottomColor));

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

            if (cloaked)
                cloudsBottomGO.AddComponent<CloakedTessSphereSectorToggle>()._sector = sector;
            else
                cloudsBottomGO.AddComponent<TessSphereSectorToggle>()._sector = sector;

            GameObject cloudsFluidGO = new GameObject("CloudsFluid");
            cloudsFluidGO.SetActive(false);
            cloudsFluidGO.layer = 17;
            cloudsFluidGO.transform.parent = cloudsMainGO.transform;

            SphereCollider fluidSC = cloudsFluidGO.AddComponent<SphereCollider>();
            fluidSC.isTrigger = true;
            fluidSC.radius = atmo.size;

            OWShellCollider fluidOWSC = cloudsFluidGO.AddComponent<OWShellCollider>();
            fluidOWSC._innerRadius = atmo.size * 0.9f;

            CloudLayerFluidVolume fluidCLFV = cloudsFluidGO.AddComponent<CloudLayerFluidVolume>();
            fluidCLFV._layer = 5;
            fluidCLFV._priority = 1;
            fluidCLFV._density = 1.2f;

            var fluidType = FluidVolume.Type.CLOUD;

            try
            {
                fluidType = (FluidVolume.Type)Enum.Parse(typeof(FluidVolume.Type), Enum.GetName(typeof(CloudFluidType), atmo.clouds.fluidType).ToUpper());
            }
            catch (Exception ex)
            {
                Logger.LogError($"Couldn't parse fluid volume type [{atmo.clouds.fluidType}]:\n{ex}");
            }

            fluidCLFV._fluidType = fluidType;
            fluidCLFV._allowShipAutoroll = true;
            fluidCLFV._disableOnStart = false;

            // Fix the rotations once the rest is done
            cloudsMainGO.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(0, 0, 0));
            // For the base shader it has to be rotated idk
            if (atmo.clouds.cloudsPrefab == CloudPrefabType.Basic) cloudsMainGO.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(90, 0, 0));

            // Lightning
            if (atmo.clouds.hasLightning)
            {
                var lightning = _lightningPrefab.InstantiateInactive();
                lightning.transform.parent = cloudsMainGO.transform;
                lightning.transform.localPosition = Vector3.zero;

                var lightningGenerator = lightning.GetComponent<CloudLightningGenerator>();
                lightningGenerator._altitude = (atmo.clouds.outerCloudRadius + atmo.clouds.innerCloudRadius) / 2f;
                lightningGenerator._audioSector = sector;
                if (atmo.clouds.lightningGradient != null)
                {
                    var gradient = new GradientColorKey[atmo.clouds.lightningGradient.Length];

                    for(int i = 0; i < atmo.clouds.lightningGradient.Length; i++)
                    {
                        var pair = atmo.clouds.lightningGradient[i];
                        gradient[i] = new GradientColorKey(pair.tint.ToColor(), pair.time);
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
            Color cloudTint = atmo.clouds.tint?.ToColor() ?? UnityEngine.Color.white;

            Texture2D image, cap, ramp;

            try
            {
                // qm cloud type = should wrap, otherwise clamp like normal
                image = ImageUtilities.GetTexture(mod, atmo.clouds.texturePath, wrap: atmo.clouds.cloudsPrefab == CloudPrefabType.QuantumMoon);

                if (atmo.clouds.capPath == null) cap = ImageUtilities.ClearTexture(128, 128);
                else cap = ImageUtilities.GetTexture(mod, atmo.clouds.capPath);
                if (atmo.clouds.rampPath == null) ramp = ImageUtilities.CanvasScaled(image, 1, image.height);
                else ramp = ImageUtilities.GetTexture(mod, atmo.clouds.rampPath);
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load Cloud textures for [{atmo.clouds.texturePath}]:\n{e}");
                return null;
            }

            GameObject cloudsTopGO = new GameObject("TopClouds");
            cloudsTopGO.SetActive(false);
            cloudsTopGO.transform.parent = rootObject.transform;
            cloudsTopGO.transform.localScale = Vector3.one * atmo.clouds.outerCloudRadius;

            MeshFilter topMF = cloudsTopGO.AddComponent<MeshFilter>();
            topMF.mesh = SearchUtilities.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;

            MeshRenderer topMR = cloudsTopGO.AddComponent<MeshRenderer>();

            if (_sphereShader == null) _sphereShader = Main.NHAssetBundle.LoadAsset<Shader>("Assets/Shaders/SphereTextureWrapper.shader");
            if (_gdCloudMaterials == null) _gdCloudMaterials = SearchUtilities.Find("CloudsTopLayer_GD").GetComponent<MeshRenderer>().sharedMaterials;
            if (_qmCloudMaterials == null) _qmCloudMaterials = SearchUtilities.Find("CloudsTopLayer_QM").GetComponent<MeshRenderer>().sharedMaterials;
            Material[] prefabMaterials = atmo.clouds.cloudsPrefab == CloudPrefabType.GiantsDeep ? _gdCloudMaterials : _qmCloudMaterials;
            var tempArray = new Material[2];

            if (atmo.clouds.cloudsPrefab == CloudPrefabType.Basic)
            {
                var material = new Material(_sphereShader);
                if (atmo.clouds.unlit) material.renderQueue = 3000;
                material.name = atmo.clouds.unlit ? "BasicCloud" : "BasicShadowCloud";

                tempArray[0] = material;
            }
            else
            {
                var material = new Material(prefabMaterials[0]);
                if (atmo.clouds.unlit) material.renderQueue = 3000;
                material.name = atmo.clouds.unlit ? "AdvancedCloud" : "AdvancedShadowCloud";
                tempArray[0] = material;
            }

            // This is the stencil material for the fog under the clouds
            tempArray[1] = new Material(prefabMaterials[1]);
            topMR.sharedMaterials = tempArray;

            foreach (var material in topMR.sharedMaterials)
            {
                material.SetTexture(MainTex, image);
                material.SetTexture(RampTex, ramp);
                material.SetTexture(CapTex, cap);
            }

            if (atmo.clouds.unlit)
            {
                cloudsTopGO.layer = LayerMask.NameToLayer("IgnoreSun");
            }

            if (atmo.clouds.rotationSpeed != 0f)
            {
                RotateTransform topRT = cloudsTopGO.AddComponent<RotateTransform>();
                // Idk why but the axis is weird
                topRT._localAxis = atmo.clouds.cloudsPrefab == CloudPrefabType.Basic ? Vector3.forward : Vector3.up;
                topRT._degreesPerSecond = atmo.clouds.rotationSpeed;
                topRT._randomizeRotationRate = false;
            }

            cloudsTopGO.transform.localPosition = Vector3.zero;

            cloudsTopGO.SetActive(true);

            return cloudsTopGO;
        }
    }
}
