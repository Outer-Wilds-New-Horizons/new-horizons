using Marshmallow.External;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.Atmosphere
{
    static class MakeAtmosphere
    {
        public static void Make(GameObject body, IPlanetConfig config)
        {
            GameObject atmoGO = new GameObject();
            atmoGO.SetActive(false);
            atmoGO.name = "Atmosphere";
            atmoGO.transform.parent = body.transform;

            if (config.HasFog)
            {
                GameObject fogGO = new GameObject();
                fogGO.SetActive(false);
                fogGO.name = "FogSphere";
                fogGO.transform.parent = atmoGO.transform;
                fogGO.transform.localScale = new Vector3((config.TopCloudSize / 2) + 10, (config.TopCloudSize / 2) + 10, (config.TopCloudSize / 2) + 10);

                MeshFilter MF = fogGO.AddComponent<MeshFilter>();
                MF.mesh = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<MeshFilter>().mesh;

                MeshRenderer MR = fogGO.AddComponent<MeshRenderer>();
                MR.materials = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<MeshRenderer>().materials;
                MR.allowOcclusionWhenDynamic = true;

                PlanetaryFogController PFC = fogGO.AddComponent<PlanetaryFogController>();
                PFC.fogLookupTexture = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<PlanetaryFogController>().fogLookupTexture;
                PFC.fogRadius = (config.TopCloudSize / 2) + 10;
                PFC.fogDensity = config.FogDensity;
                PFC.fogExponent = 1f;
                PFC.fogColorRampTexture = GameObject.Find("Atmosphere_GD/FogSphere").GetComponent<PlanetaryFogController>().fogColorRampTexture;
                PFC.fogColorRampIntensity = 1f;
                PFC.fogTint = config.FogTint.ToColor32();

                fogGO.SetActive(true);
            }

            Logger.Log("Re-add LOD atmosphere!", Logger.LogType.Todo);

            /*
            GameObject atmo = new GameObject();
            atmo.SetActive(false);
            atmo.transform.parent = atmoM.transform;
            atmo.transform.localScale = new Vector3(topCloudScale + 100, topCloudScale + 100, topCloudScale + 100);

            Material mat = GameObject.Find("Atmosphere_LOD0").GetComponent<MeshRenderer>().material;

            GameObject lod0 = new GameObject();
            lod0.transform.parent = atmo.transform;
            MeshFilter f0 = lod0.AddComponent<MeshFilter>();
            f0.mesh = GameObject.Find("Atmosphere_LOD0").GetComponent<MeshFilter>().mesh;
            MeshRenderer r0 = lod0.AddComponent<MeshRenderer>();
            r0.material = mat;

            GameObject lod1 = new GameObject();
            lod0.transform.parent = atmo.transform;
            MeshFilter f1 = lod1.AddComponent<MeshFilter>();
            f1.mesh = GameObject.Find("Atmosphere_LOD1").GetComponent<MeshFilter>().mesh;
            MeshRenderer r1 = lod1.AddComponent<MeshRenderer>();
            r1.material = mat;

            GameObject lod2 = new GameObject();
            lod2.transform.parent = atmo.transform;
            MeshFilter f2 = lod2.AddComponent<MeshFilter>();
            f2.mesh = GameObject.Find("Atmosphere_LOD2").GetComponent<MeshFilter>().mesh;
            MeshRenderer r2 = lod2.AddComponent<MeshRenderer>();
            r2.material = mat;

            GameObject lod3 = new GameObject();
            lod3.transform.parent = atmo.transform;
            MeshFilter f3 = lod3.AddComponent<MeshFilter>();
            f3.mesh = GameObject.Find("Atmosphere_LOD3").GetComponent<MeshFilter>().mesh;
            MeshRenderer r3 = lod3.AddComponent<MeshRenderer>();
            r3.material = mat;

            // THIS FUCKING THING. do NOT ask why i have done this. IT WORKS.
            // This creates an LOD group in the worst way possible. i am so sorry.
            LODGroup lodg = atmo.AddComponent<LODGroup>();
            
            LOD[] lodlist = new LOD[4];
            Renderer[] t0 = { r0 };
            Renderer[] t1 = { r1 };
            Renderer[] t2 = { r2 };
            Renderer[] t3 = { r3 };
            LOD one = new LOD(1, t0);
            LOD two = new LOD(0.7f, t1);
            LOD three = new LOD(0.27f, t2);
            LOD four = new LOD(0.08f, t3);
            lodlist[0] = one;
            lodlist[1] = two;
            lodlist[2] = three;
            lodlist[3] = four;

            lodg.SetLODs(lodlist);
            lodg.fadeMode = LODFadeMode.None;
            */

            //atmo.SetActive(true);
            atmoGO.SetActive(true);
        }
    }
}
