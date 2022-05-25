using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility;
using UnityEngine;
using NewHorizons.External.Modules.VariableSize;
using Tessellation;

namespace NewHorizons.Builder.Body
{
    public static class WaterBuilder
    {
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int Radius2 = Shader.PropertyToID("_Radius2");

        public static void Make(GameObject planetGO, Sector sector, OWRigidbody rb, WaterModule module)
        {
            var waterSize = module.size;

            GameObject waterGO = new GameObject("Water");
            waterGO.SetActive(false);
            waterGO.layer = 15;
            waterGO.transform.parent = sector?.transform ?? planetGO.transform;
            waterGO.transform.localScale = new Vector3(waterSize, waterSize, waterSize);

            var GDTSR = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>();

            TessellatedSphereRenderer TSR = waterGO.AddComponent<TessellatedSphereRenderer>();
            TSR.tessellationMeshGroup = ScriptableObject.CreateInstance<MeshGroup>();
            for (int i = 0; i < 16; i++)
            {
                var mesh = new Mesh();
                mesh.CopyPropertiesFrom(GDTSR.tessellationMeshGroup.variants[i]);
                TSR.tessellationMeshGroup.variants[i] = mesh;
            }

            var GDSharedMaterials = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereLOD>()._lowAltitudeMaterials;
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

            var GDOLC = GDTSR.GetComponent<OceanLODController>();
            var OLC = waterGO.AddComponent<OceanLODController>();
            OLC._sector = sector;
            OLC._ambientLight = GDOLC._ambientLight; // this needs to be set or else is black
            
            // trigger sector enter
            Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
            {
                OEC._active = true;
                OEC.enabled = true;

                OLC.enabled = true;
            });

            //Buoyancy
            var buoyancyObject = new GameObject("WaterVolume");
            buoyancyObject.transform.parent = waterGO.transform;
            buoyancyObject.transform.localScale = Vector3.one;
            buoyancyObject.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphereCollider = buoyancyObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 1;
            sphereCollider.isTrigger = true;

            var owCollider = buoyancyObject.AddComponent<OWCollider>();
            owCollider._parentBody = rb;
            owCollider._collider = sphereCollider;

            var buoyancyTriggerVolume = buoyancyObject.AddComponent<OWTriggerVolume>();
            buoyancyTriggerVolume._owCollider = owCollider;

            var fluidVolume = buoyancyObject.AddComponent<NHFluidVolume>();
            fluidVolume._fluidType = FluidVolume.Type.WATER;
            fluidVolume._attachedBody = rb;
            fluidVolume._triggerVolume = buoyancyTriggerVolume;
            fluidVolume._radius = waterSize;
            fluidVolume._layer = LayerMask.NameToLayer("BasicEffectVolume");

            var fogGO = GameObject.Instantiate(GameObject.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/OceanFog"), waterGO.transform);
            fogGO.name = "OceanFog";
            fogGO.transform.localPosition = Vector3.zero;
            fogGO.transform.localScale = Vector3.one;

            if (module.tint != null)
            {
                var adjustedColour = module.tint.ToColor() / 4f;
                adjustedColour.a *= 4f;
                fogGO.GetComponent<MeshRenderer>().material.color = adjustedColour;
            }

            if (module.curve != null)
            {
                var sizeController = waterGO.AddComponent<WaterSizeController>();
                var curve = new AnimationCurve();
                foreach (var pair in module.curve)
                {
                    curve.AddKey(new Keyframe(pair.time, pair.value));
                }
                sizeController.scaleCurve = curve;
                sizeController.oceanFogMaterial = fogGO.GetComponent<MeshRenderer>().material;
                sizeController.size = module.size;
            }
            else
            {
                fogGO.GetComponent<MeshRenderer>().material.SetFloat(Radius, module.size);
                fogGO.GetComponent<MeshRenderer>().material.SetFloat(Radius2, module.size / 2f);
            }

            // TODO: fix ruleset making the sand bubble pop up

            waterGO.transform.position = planetGO.transform.position;
            waterGO.SetActive(true);
        }
    }
}
