using NewHorizons.External;
using NewHorizons.External.VariableSize;
using OWML.Utils;
using UnityEngine;
using NewHorizons.Utility;
using Logger = NewHorizons.Utility.Logger;
using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;

namespace NewHorizons.Builder.Body
{
    static class WaterBuilder
    {
        public static void Make(GameObject body, Sector sector, OWRigidbody rb, WaterModule module)
        {
            var waterSize = module.Size;

            GameObject waterGO = new GameObject("Water");
            waterGO.SetActive(false);
            waterGO.layer = 15;
            waterGO.transform.parent = body.transform;
            waterGO.transform.localScale = new Vector3(waterSize, waterSize, waterSize);

            var GDTSR = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>();

            TessellatedSphereRenderer TSR = waterGO.AddComponent<TessellatedSphereRenderer>();
            TSR.tessellationMeshGroup = new Tessellation.MeshGroup();
            for (int i = 0; i < 16; i++)
            {
                var mesh = new Mesh();
                mesh.CopyPropertiesFrom(GDTSR.tessellationMeshGroup.variants[i]);
                TSR.tessellationMeshGroup.variants[i] = mesh;
            }

            var GDSharedMaterials = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereLOD>()._lowAltitudeMaterials;
            var tempArray = new Material[GDSharedMaterials.Length];
            for(int i = 0; i < GDSharedMaterials.Length; i++)
            {
                tempArray[i] = new Material(GDSharedMaterials[i]);
                if (module.Tint != null)
                {
                    tempArray[i].color = module.Tint.ToColor32();
                    tempArray[i].color = module.Tint.ToColor();
                    tempArray[i].SetColor("_FogColor", module.Tint.ToColor());
                }
            }

            TSR.sharedMaterials = tempArray;
            TSR.maxLOD = 0;
            TSR.LODBias = 0;
            TSR.LODRadius = 0;

            OceanEffectController OEC = waterGO.AddComponent<OceanEffectController>();
            OEC.SetValue("_sector", sector);
            OEC.SetValue("_ocean", TSR);

            //Buoyancy
            var buoyancyObject = new GameObject("WaterVolume");
            buoyancyObject.transform.parent = waterGO.transform;
            buoyancyObject.transform.localScale = Vector3.one;
            buoyancyObject.layer = LayerMask.NameToLayer("BasicEffectVolume");

            var sphereCollider = buoyancyObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 1;
            sphereCollider.isTrigger = true;

            var owCollider = buoyancyObject.AddComponent<OWCollider>();
            owCollider.SetValue("_parentBody", rb);
            owCollider.SetValue("_collider", sphereCollider);

            var buoyancyTriggerVolume = buoyancyObject.AddComponent<OWTriggerVolume>();
            buoyancyTriggerVolume.SetValue("_owCollider", owCollider);

            var fluidVolume = buoyancyObject.AddComponent<RadialFluidVolume>();
            fluidVolume.SetValue("_fluidType", FluidVolume.Type.WATER);
            fluidVolume.SetValue("_attachedBody", rb);
            fluidVolume.SetValue("_triggerVolume", buoyancyTriggerVolume);
            fluidVolume.SetValue("_radius", waterSize);
            fluidVolume.SetValue("_layer", LayerMask.NameToLayer("BassicEffectVolume"));

            var fogGO = GameObject.Instantiate(GameObject.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Effects_GDInterior/OceanFog"), waterGO.transform);
            fogGO.name = "OceanFog";
            fogGO.transform.localPosition = Vector3.zero;
            fogGO.transform.localScale = Vector3.one;
            if (module.Tint != null)
            {
                var adjustedColour = module.Tint.ToColor() / 4f;
                adjustedColour.a = adjustedColour.a * 4f;

                fogGO.GetComponent<MeshRenderer>().material.color = adjustedColour;
            }

            if (module.Curve != null)
            {
                var sizeController = waterGO.AddComponent<WaterSizeController>();
                var curve = new AnimationCurve();
                foreach (var pair in module.Curve)
                {
                    curve.AddKey(new Keyframe(pair.Time, pair.Value));
                }
                sizeController.scaleCurve = curve;
                sizeController.oceanFogMaterial = fogGO.GetComponent<MeshRenderer>().material;
                sizeController.size = module.Size;
            }

            // TODO: make LOD work 
            //waterGO.AddComponent<TessellatedSphereLOD>();
            //waterGO.AddComponent<OceanLODController>();

            // TODO: fix ruleset making the sand bubble pop up

            waterGO.transform.localPosition = Vector3.zero;
            waterGO.SetActive(true);
        }
    }
}
