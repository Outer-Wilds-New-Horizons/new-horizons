using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility;
using UnityEngine;
using NewHorizons.External.Modules.VariableSize;
using Tessellation;
using NewHorizons.Components;

namespace NewHorizons.Builder.Body
{
    public static class WaterBuilder
    {
        private static MeshGroup _oceanMeshGroup;
        private static Material[] _oceanLowAltitudeMaterials;
        private static GameObject _oceanFog;
        private static GameObject _oceanAmbientLight;

        private static bool _isInit;

        internal static void InitPrefabs()
        {
            if (_isInit) return;

            _isInit = true;

            if (_oceanMeshGroup == null)
            {
                _oceanMeshGroup = ScriptableObject.CreateInstance<MeshGroup>().Rename("Ocean").DontDestroyOnLoad();
                var gdVariants = SearchUtilities.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup.variants;
                for (int i = 0; i < 16; i++)
                {
                    var mesh = new Mesh();
                    mesh.CopyPropertiesFrom(gdVariants[i]);
                    mesh.name = gdVariants[i].name;
                    mesh.DontDestroyOnLoad();
                    _oceanMeshGroup.variants[i] = mesh;
                }
            }
            if (_oceanLowAltitudeMaterials == null) _oceanLowAltitudeMaterials = SearchUtilities.Find("Ocean_GD").GetComponent<TessellatedSphereLOD>()._lowAltitudeMaterials.MakePrefabMaterials();
            if (_oceanFog == null) _oceanFog = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/OceanFog").InstantiateInactive().Rename("Prefab_GD_OceanFog").DontDestroyOnLoad();
            if (_oceanAmbientLight == null) _oceanAmbientLight = SearchUtilities.Find("Ocean_GD").GetComponent<OceanLODController>()._ambientLight.gameObject.InstantiateInactive().Rename("OceanAmbientLight").DontDestroyOnLoad();
        }

        public static void Make(GameObject planetGO, Sector sector, OWRigidbody rb, WaterModule module)
        {
            InitPrefabs();

            var waterSize = module.size;

            GameObject waterGO = new GameObject("Water");
            waterGO.SetActive(false);
            waterGO.layer = 15;
            waterGO.transform.parent = sector?.transform ?? planetGO.transform;
            waterGO.transform.localScale = new Vector3(waterSize, waterSize, waterSize);

            TessellatedSphereRenderer TSR = waterGO.AddComponent<TessellatedSphereRenderer>();
            TSR.tessellationMeshGroup = ScriptableObject.CreateInstance<MeshGroup>();
            for (int i = 0; i < 16; i++)
            {
                var mesh = new Mesh();
                mesh.CopyPropertiesFrom(_oceanMeshGroup.variants[i]);
                TSR.tessellationMeshGroup.variants[i] = mesh;
            }

            var GDSharedMaterials = _oceanLowAltitudeMaterials;
            var tempArray = new Material[GDSharedMaterials.Length];
            for (int i = 0; i < GDSharedMaterials.Length; i++)
            {
                tempArray[i] = new Material(GDSharedMaterials[i]);
                if (module.tint != null)
                {
                    tempArray[i].color = module.tint.ToColor();
                    tempArray[i].SetColor("_FogColor", module.tint.ToColor());
                }
            }

            TSR.sharedMaterials = tempArray;

            // stuff is black without this crap
            OceanEffectController OEC = waterGO.AddComponent<OceanEffectController>();
            OEC._sector = sector;
            OEC._ocean = TSR;

            var OLC = waterGO.AddComponent<OceanLODController>();
            OLC._sector = sector;
            OLC._ambientLight = _oceanAmbientLight.GetComponent<Light>(); // this needs to be set or else is black
            
            // trigger sector enter
            Delay.FireOnNextUpdate(() =>
            {
                OEC._active = true;
                OEC.enabled = true;

                OLC.enabled = true;
            });

            //Buoyancy
            var buoyancyObject = new GameObject("WaterVolume");
            buoyancyObject.transform.parent = waterGO.transform;
            buoyancyObject.transform.localScale = Vector3.one * 1.02f;
            buoyancyObject.layer = LayerMask.NameToLayer("BasicEffectVolume");
            buoyancyObject.SetActive(false);

            var sphereCollider = buoyancyObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 1;
            sphereCollider.isTrigger = true;

            var owCollider = buoyancyObject.AddComponent<OWCollider>();
            owCollider._parentBody = rb;
            owCollider._collider = sphereCollider;

            var waveHeightCalculator = buoyancyObject.AddComponent<WaveHeightCalculator>();
            waveHeightCalculator._oceanMaterial = TSR.sharedMaterials[1];
            waveHeightCalculator._oceanTransform = waterGO.transform;

            var owOceanCollider = buoyancyObject.AddComponent<OWOceanCollider>();
            owOceanCollider._owCollider = owCollider;
            owOceanCollider._wavelessRadius = module.size * 1.004f;
            owOceanCollider._collider = sphereCollider;
            owOceanCollider._waveHeightCalculator = waveHeightCalculator;

            var buoyancyTriggerVolume = buoyancyObject.AddComponent<OWTriggerVolume>();
            buoyancyTriggerVolume._owCollider = owCollider;
            buoyancyTriggerVolume._owCustomCollider = owOceanCollider;

            var fluidVolume = buoyancyObject.AddComponent<NHFluidVolume>();
            fluidVolume._fluidType = FluidVolume.Type.WATER;
            fluidVolume._attachedBody = rb;
            fluidVolume._triggerVolume = buoyancyTriggerVolume;
            fluidVolume._buoyancyDensity = 1.1f;
            fluidVolume._radius = module.size * 1.004f;
            fluidVolume._allowShipAutoroll = true;
            fluidVolume._density = 30;
            fluidVolume._layer = 5;
            fluidVolume._priority = 3;
            fluidVolume._waveHeightCalculator = waveHeightCalculator;

            buoyancyObject.SetActive(true);

            var fogGO = GameObject.Instantiate(_oceanFog, waterGO.transform);
            fogGO.name = "OceanFog";
            fogGO.transform.localPosition = Vector3.zero;
            fogGO.transform.localScale = Vector3.one;
            fogGO.SetActive(true);

            if (module.tint != null)
            {
                var adjustedColour = module.tint.ToColor() / 4f;
                adjustedColour.a *= 4f;
                fogGO.GetComponent<MeshRenderer>().material.color = adjustedColour;
            }

            if (module.curve != null)
            {
                var sizeController = waterGO.AddComponent<WaterSizeController>();
                sizeController.SetScaleCurve(module.curve);
                sizeController.oceanFogMaterial = fogGO.GetComponent<MeshRenderer>().material;
                sizeController.size = module.size;
            }
            else
            {
                fogGO.GetComponent<MeshRenderer>().material.SetFloat("_Radius", module.size);
                fogGO.GetComponent<MeshRenderer>().material.SetFloat("_Radius2", 0);
            }

            var oceanGravity = new GameObject("Ocean_Gravity");
            oceanGravity.transform.parent = waterGO.transform;
            oceanGravity.transform.localPosition = Vector3.zero;
            oceanGravity.transform.localScale = Vector3.one * 0.86f;
            oceanGravity.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphereColliderGravity = oceanGravity.AddComponent<SphereCollider>();
            sphereColliderGravity.radius = 1;
            sphereColliderGravity.isTrigger = true;

            var owColliderGravity = oceanGravity.AddComponent<OWCollider>();
            owColliderGravity._parentBody = rb;
            owColliderGravity._collider = sphereColliderGravity;

            var triggerVolumeGravity = oceanGravity.AddComponent<OWTriggerVolume>();
            triggerVolumeGravity._owCollider = owColliderGravity;

            var polarForceVolume = oceanGravity.AddComponent<PolarForceVolume>();
            polarForceVolume._attachedBody = rb;
            polarForceVolume._triggerVolume = triggerVolumeGravity;
            polarForceVolume._inheritable = false;
            polarForceVolume._layer = 5;
            polarForceVolume._priority = 1;
            polarForceVolume._alignmentPriority = 1;
            polarForceVolume._acceleration = 1;
            polarForceVolume._localAxis = Vector3.up;
            polarForceVolume._fieldMode = PolarForceVolume.ForceMode.Polar;

            // TODO: fix ruleset making the sand bubble pop up

            waterGO.transform.position = planetGO.transform.position;
            waterGO.SetActive(true);
        }
    }
}
