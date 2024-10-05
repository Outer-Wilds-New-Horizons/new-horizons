using NewHorizons.Components;
using NewHorizons.Components.Sectored;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Tessellation;
using UnityEngine;

namespace NewHorizons.Builder.Atmosphere
{
    public static class CloudsBuilder
    {
        private static Material[] _gdCloudMaterials, _gdBottomMaterials;
        private static Mesh _gdTopCloudMesh;

        private static Material[] _qmCloudMaterials, _qmBottomMaterials;
        private static MeshGroup _qmBottomMeshGroup;

        private static Material _transparentCloud;
        private static GameObject _lightningPrefab;
        private static Texture2D _colorRamp;
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int ColorRamp = Shader.PropertyToID("_ColorRamp");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int RampTex = Shader.PropertyToID("_RampTex");
        private static readonly int CapTex = Shader.PropertyToID("_CapTex");
        private static readonly int Smoothness = Shader.PropertyToID("_Glossiness");

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_colorRamp == null) _colorRamp = ImageUtilities.GetTexture(Main.Instance, "Assets/textures/Clouds_Bottom_ramp.png");

            if (_isInit) return;

            _isInit = true;

            if (_lightningPrefab == null) _lightningPrefab = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Clouds_GD/LightningGenerator_GD").InstantiateInactive().Rename("LightningGenerator").DontDestroyOnLoad();
            
            if (_gdTopCloudMesh == null) _gdTopCloudMesh = SearchUtilities.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh.DontDestroyOnLoad();
            if (_gdCloudMaterials == null) _gdCloudMaterials = SearchUtilities.Find("CloudsTopLayer_GD").GetComponent<MeshRenderer>().sharedMaterials.MakePrefabMaterials();
            if (_gdBottomMaterials == null) _gdBottomMaterials = SearchUtilities.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>().sharedMaterials.MakePrefabMaterials();
            
            if (_qmCloudMaterials == null) _qmCloudMaterials = SearchUtilities.Find("CloudsTopLayer_QM").GetComponent<MeshRenderer>().sharedMaterials.MakePrefabMaterials();
            if (_qmBottomMaterials == null) _qmBottomMaterials = SearchUtilities.Find("CloudsBottomLayer_QM").GetComponent<TessellatedSphereRenderer>().sharedMaterials.MakePrefabMaterials();
            
            if (_qmBottomMeshGroup == null)
            {
                var originalMeshGroup = SearchUtilities.Find("CloudsBottomLayer_QM").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
                _qmBottomMeshGroup = ScriptableObject.CreateInstance<MeshGroup>().Rename("BottomClouds").DontDestroyOnLoad();
                var variants = new List<Mesh>();
                foreach (var variant in originalMeshGroup.variants)
                {
                    var mesh = new Mesh();
                    mesh.CopyPropertiesFrom(variant);
                    mesh.name = variant.name;
                    mesh.DontDestroyOnLoad();
                    variants.Add(mesh);
                }
                _qmBottomMeshGroup.variants = variants.ToArray();
            }
            if (_transparentCloud == null) _transparentCloud = AssetBundleUtilities.NHAssetBundle.LoadAsset<Material>("Assets/Resources/TransparentCloud.mat");
        }

        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmo, bool cloaked, IModBehaviour mod)
        {
            InitPrefabs();

            var cloudsMainGO = new GameObject("Clouds");
            cloudsMainGO.SetActive(false);
            cloudsMainGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (atmo.clouds.cloudsPrefab != CloudPrefabType.Transparent) MakeTopClouds(cloudsMainGO, atmo, mod);
            else
            {
                MakeTransparentClouds(cloudsMainGO, atmo, mod);
                if (atmo.clouds.hasLightning) MakeLightning(cloudsMainGO, sector, atmo);
                cloudsMainGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
                cloudsMainGO.SetActive(true);
                return;
            }

            var cloudsBottomGO = new GameObject("BottomClouds");
            cloudsBottomGO.SetActive(false);
            cloudsBottomGO.transform.parent = cloudsMainGO.transform;
            cloudsBottomGO.transform.localScale = Vector3.one * atmo.clouds.innerCloudRadius;

            var bottomTSR = cloudsBottomGO.AddComponent<TessellatedSphereRenderer>();
            bottomTSR.tessellationMeshGroup = _qmBottomMeshGroup;

            var bottomTSRMaterials = atmo.clouds.cloudsPrefab == CloudPrefabType.GiantsDeep ? _gdBottomMaterials : _qmBottomMaterials;

            // If they set a colour apply it to all the materials else keep the defaults
            if (atmo.clouds.tint == null)
            {
                bottomTSR.sharedMaterials = bottomTSRMaterials.Select(x => new Material(x)).ToArray();
            }
            else
            {
                var bottomColor = atmo.clouds.tint.ToColor();
                var bottomTSRTempArray = new Material[2];

                bottomTSRTempArray[0] = new Material(bottomTSRMaterials[0]);
                bottomTSRTempArray[0].SetColor(Color, bottomColor);
                bottomTSRTempArray[0].SetTexture(ColorRamp, ImageUtilities.TintImage(_colorRamp, bottomColor));

                bottomTSRTempArray[1] = new Material(bottomTSRMaterials[1]);

                bottomTSR.sharedMaterials = bottomTSRTempArray;
            }

            bottomTSR.maxLOD = 6;
            bottomTSR.LODBias = 0;
            bottomTSR.LODRadius = 1f;

            if (cloaked)
            {
                cloudsBottomGO.AddComponent<CloakedTessSphereSectorToggle>()._sector = sector;
            }
            else
            {
                cloudsBottomGO.AddComponent<TessSphereSectorToggle>()._sector = sector;
            }

            var cloudsFluidGO = new GameObject("CloudsFluid");
            cloudsFluidGO.SetActive(false);
            cloudsFluidGO.layer = Layer.BasicEffectVolume;
            cloudsFluidGO.transform.parent = cloudsMainGO.transform;

            var fluidSC = cloudsFluidGO.AddComponent<SphereCollider>();
            fluidSC.isTrigger = true;
            fluidSC.radius = atmo.clouds.outerCloudRadius;

            var fluidOWSC = cloudsFluidGO.AddComponent<OWShellCollider>();
            fluidOWSC._innerRadius = atmo.clouds.innerCloudRadius;

            // copied from gd
            var fluidCLFV = cloudsFluidGO.AddComponent<CloudLayerFluidVolume>();
            fluidCLFV._layer = 5;
            fluidCLFV._priority = 1;
            fluidCLFV._density = 1.2f;
            fluidCLFV._fluidType = atmo.clouds.fluidType.ConvertToOW(FluidVolume.Type.CLOUD);
            fluidCLFV._allowShipAutoroll = atmo.allowShipAutoroll;
            fluidCLFV._disableOnStart = false;

            // Fix the rotations once the rest is done
            cloudsMainGO.transform.rotation = planetGO.transform.TransformRotation(Quaternion.Euler(0, 0, 0));

            // Lightning
            if (atmo.clouds.hasLightning)
            {
                MakeLightning(cloudsMainGO, sector, atmo);
            }

            cloudsMainGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            cloudsBottomGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            cloudsFluidGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);

            cloudsBottomGO.SetActive(true);
            cloudsFluidGO.SetActive(true);
            cloudsMainGO.SetActive(true);
        }

        public static CloudLightningGenerator MakeLightning(GameObject rootObject, Sector sector, AtmosphereModule atmo, bool noAudio = false)
        {
            InitPrefabs();

            var lightning = _lightningPrefab.InstantiateInactive();
            lightning.name = "LightningGenerator";
            lightning.transform.parent = rootObject.transform;
            lightning.transform.localPosition = Vector3.zero;

            var lightningGenerator = lightning.GetComponent<CloudLightningGenerator>();

            lightningGenerator._altitude = atmo.clouds.cloudsPrefab switch
            {
                CloudPrefabType.GiantsDeep or CloudPrefabType.QuantumMoon => (atmo.clouds.outerCloudRadius + atmo.clouds.innerCloudRadius) / 2f,
                _ => atmo.clouds.outerCloudRadius,
            };

            if (noAudio)
            {
                lightningGenerator._audioPrefab = null;
                lightningGenerator._audioSourcePool = null;
            }

            lightningGenerator._audioSector = sector;
            if (atmo.clouds.lightningGradient != null)
            {
                var gradient = new GradientColorKey[atmo.clouds.lightningGradient.Length];

                for (int i = 0; i < atmo.clouds.lightningGradient.Length; i++)
                {
                    var pair = atmo.clouds.lightningGradient[i];
                    gradient[i] = new GradientColorKey(pair.tint.ToColor(), pair.time);
                }

                lightningGenerator._lightColor.colorKeys = gradient;
            }
            lightning.SetActive(true);

            return lightningGenerator;
        }

        public static GameObject MakeTopClouds(GameObject rootObject, AtmosphereModule atmo, IModBehaviour mod)
        {
            InitPrefabs();

            Texture2D image, cap, ramp;

            try
            {
                // qm cloud type = should wrap, otherwise clamp like normal
                image = ImageUtilities.GetTexture(mod, atmo.clouds.texturePath, wrap: atmo.clouds.cloudsPrefab == CloudPrefabType.QuantumMoon);

                if (atmo.clouds.capPath == null) cap = ImageUtilities.ClearTexture(128, 128, wrap: true);
                else cap = ImageUtilities.GetTexture(mod, atmo.clouds.capPath, wrap: true);

                if (atmo.clouds.rampPath == null) ramp = ImageUtilities.CanvasScaled(image, 1, image.height);
                else ramp = ImageUtilities.GetTexture(mod, atmo.clouds.rampPath);
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load Cloud textures for [{atmo.clouds.texturePath}]:\n{e}");
                return null;
            }

            var cloudsTopGO = new GameObject("TopClouds");
            cloudsTopGO.SetActive(false);
            cloudsTopGO.transform.parent = rootObject.transform;
            cloudsTopGO.transform.localScale = Vector3.one * atmo.clouds.outerCloudRadius;

            var topMF = cloudsTopGO.AddComponent<MeshFilter>();
            topMF.mesh = _gdTopCloudMesh;

            var topMR = cloudsTopGO.AddComponent<MeshRenderer>();

            var prefabMaterials = atmo.clouds.cloudsPrefab == CloudPrefabType.GiantsDeep ? _gdCloudMaterials : _qmCloudMaterials;
            var tempArray = new Material[2];

            if (atmo.clouds.cloudsPrefab == CloudPrefabType.Basic)
            {
                var material = new Material(Shader.Find("Standard"));
                if (atmo.clouds.unlit) material.renderQueue = 3000;
                material.name = atmo.clouds.unlit ? "BasicCloud" : "BasicShadowCloud";
                material.SetFloat(Smoothness, 0f);
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
                cloudsTopGO.layer = Layer.IgnoreSun;
            }

            if (atmo.clouds.rotationSpeed != 0f)
            {
                var topRT = cloudsTopGO.AddComponent<RotateTransform>();
                topRT._localAxis = Vector3.up;
                topRT._degreesPerSecond = atmo.clouds.rotationSpeed;
                topRT._randomizeRotationRate = false;
            }

            cloudsTopGO.transform.localPosition = Vector3.zero;

            cloudsTopGO.SetActive(true);

            return cloudsTopGO;
        }

        public static GameObject MakeTransparentClouds(GameObject rootObject, AtmosphereModule atmo, IModBehaviour mod, bool isProxy = false)
        {
            InitPrefabs();

            Texture2D image;

            try
            {
                image = ImageUtilities.GetTexture(mod, atmo.clouds.texturePath);
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load Cloud texture for [{atmo.clouds.texturePath}]:\n{e}");
                return null;
            }

            var cloudsTransparentGO = new GameObject("TransparentClouds");
            cloudsTransparentGO.SetActive(false);
            cloudsTransparentGO.transform.parent = rootObject.transform;
            cloudsTransparentGO.transform.localScale = Vector3.one * atmo.clouds.outerCloudRadius;

            MeshFilter filter = cloudsTransparentGO.AddComponent<MeshFilter>();
            filter.mesh = _gdTopCloudMesh;

            var renderer = cloudsTransparentGO.AddComponent<MeshRenderer>();
            var material = new Material(_transparentCloud);
            material.name = "TransparentClouds_" + image.name;
            material.SetTexture(MainTex, image);
            renderer.sharedMaterial = material;

            if (!isProxy)
            {
                var tcrqcGO = new GameObject("TransparentCloudRenderQueueController");
                tcrqcGO.transform.SetParent(cloudsTransparentGO.transform, false);
                tcrqcGO.layer = Layer.BasicEffectVolume;

                var shape = tcrqcGO.AddComponent<SphereShape>();
                shape.radius = 1;

                var owTriggerVolume = tcrqcGO.AddComponent<OWTriggerVolume>();
                owTriggerVolume._shape = shape;

                var tcrqc = tcrqcGO.AddComponent<TransparentCloudRenderQueueController>();
                tcrqc.renderer = renderer;
            }

            if (atmo.clouds.rotationSpeed != 0f)
            {
                var rt = cloudsTransparentGO.AddComponent<RotateTransform>();
                rt._localAxis = Vector3.up;
                rt._degreesPerSecond = atmo.clouds.rotationSpeed;
                rt._randomizeRotationRate = false;
            }

            cloudsTransparentGO.transform.localPosition = Vector3.zero;

            cloudsTransparentGO.SetActive(true);

            return cloudsTransparentGO;
        }
    }
}
