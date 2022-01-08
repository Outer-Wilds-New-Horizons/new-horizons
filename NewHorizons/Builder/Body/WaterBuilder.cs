using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    static class WaterBuilder
    {
        public static void Make(GameObject body, Sector sector, OWRigidbody rb, float waterSize)
        {
            GameObject waterGO = new GameObject("Water");
            waterGO.SetActive(false);
            waterGO.layer = 15;
            waterGO.transform.parent = body.transform;
            waterGO.transform.localScale = new Vector3(waterSize, waterSize, waterSize);
            waterGO.DestroyAllComponents<SphereCollider>();

            var GDTSR = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>();

            TessellatedSphereRenderer TSR = waterGO.AddComponent<TessellatedSphereRenderer>();
            TSR.tessellationMeshGroup = GDTSR.tessellationMeshGroup;

            var GDSharedMaterials = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereLOD>()._lowAltitudeMaterials;
            var tempArray = new Material[GDSharedMaterials.Length];
            for(int i = 0; i < GDSharedMaterials.Length; i++)
            {
                tempArray[i] = new Material(GDSharedMaterials[i]);
            }
            // TODO: Make water module
            //tempArray[1].color = Color.red;

            TSR.sharedMaterials = tempArray;
            TSR.maxLOD = GDTSR.maxLOD;
            TSR.LODBias = GDTSR.LODBias;
            TSR.LODRadius = GDTSR.LODRadius;

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

            // Because assetbundles were a bitch...
            GameObject fog1 = new GameObject();
            fog1.transform.parent = waterGO.transform;
            fog1.transform.localScale = new Vector3(1, 1, 1);
            fog1.AddComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            fog1.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            fog1.GetComponent<MeshRenderer>().material.color = new Color32(0, 75, 50, 5);

            GameObject fog2 = new GameObject();
            fog2.transform.parent = waterGO.transform;
            fog2.transform.localScale = new Vector3(1.001f, 1.001f, 1.001f);
            fog2.AddComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            fog2.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            fog2.GetComponent<MeshRenderer>().material.color = new Color32(0, 75, 50, 5);

            GameObject fog3 = new GameObject();
            fog3.transform.parent = fog2.transform;
            fog3.transform.localScale = new Vector3(1.001f, 1.001f, 1.001f);
            fog3.AddComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            fog3.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            fog3.GetComponent<MeshRenderer>().material.color = new Color32(0, 75, 50, 5);

            waterGO.transform.localPosition = Vector3.zero;
            waterGO.SetActive(true);
        }
    }
}
