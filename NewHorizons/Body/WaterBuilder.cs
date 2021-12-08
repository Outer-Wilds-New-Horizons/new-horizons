using NewHorizons.External;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Body
{
    static class WaterBuilder
    {
        public static void Make(GameObject body, Sector sector, IPlanetConfig config)
        {
            GameObject waterGO = new GameObject("Water");
            waterGO.SetActive(false);
            waterGO.layer = 15;
            waterGO.transform.parent = body.transform;
            waterGO.transform.localScale = new Vector3(config.WaterSize, config.WaterSize, config.WaterSize);
            waterGO.DestroyAllComponents<SphereCollider>();

            TessellatedSphereRenderer TSR = waterGO.AddComponent<TessellatedSphereRenderer>();
            TSR.tessellationMeshGroup = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>().tessellationMeshGroup;
            TSR.sharedMaterials = GameObject.Find("Ocean_GD").GetComponent<TessellatedSphereRenderer>().sharedMaterials;
            TSR.maxLOD = 7;
            TSR.LODBias = 2;
            TSR.LODRadius = 2f;

            TessSphereSectorToggle TSST = waterGO.AddComponent<TessSphereSectorToggle>();
            TSST.SetValue("_sector", sector);

            OceanEffectController OEC = waterGO.AddComponent<OceanEffectController>();
            OEC.SetValue("_sector", sector);
            OEC.SetValue("_ocean", TSR);

            // Because assetbundles were a bitch...
            /*
            GameObject fog1 = new GameObject();
            fog1.transform.parent = waterBase.transform;
            fog1.transform.localScale = new Vector3(1, 1, 1);
            fog1.AddComponent<MeshFilter>().mesh = GameObject.Find("CloudsTopLayer_GD").GetComponent<MeshFilter>().mesh;
            fog1.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            fog1.GetComponent<MeshRenderer>().material.color = new Color32(0, 75, 50, 5);

            GameObject fog2 = new GameObject();
            fog2.transform.parent = waterBase.transform;
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
            */

            waterGO.SetActive(true);

            Logger.Log("Finished building water", Logger.LogType.Log);
        }
    }
}
