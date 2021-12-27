using NewHorizons.External;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Atmosphere
{
    static class AtmosphereBuilder
    {
        public static void Make(GameObject body, AtmosphereModule atmosphereModule)
        {
            GameObject atmoGO = new GameObject("Atmosphere");
            atmoGO.SetActive(false);
            atmoGO.transform.parent = body.transform;

            if (atmosphereModule.HasAtmosphere)
            {
                var mat = GameObject.Find("TimberHearth_Body/Atmosphere_TH/AtmoSphere/Atmosphere_LOD0").GetComponent<MeshRenderer>().material;

                GameObject atmo = GameObject.Instantiate(GameObject.Find("Atmosphere_TH/AtmoSphere"));
                atmo.transform.parent = atmoGO.transform;
                atmo.transform.localPosition = Vector3.zero;
                atmo.transform.localScale = Vector3.one * atmosphereModule.Size;
                atmo.SetActive(true);

                /*
                GameObject lod0 = new GameObject();
                lod0.transform.parent = atmo.transform;
                lod0.transform.localPosition = Vector3.zero;
                MeshFilter f0 = lod0.AddComponent<MeshFilter>();
                f0.mesh = GameObject.Find("Atmosphere_LOD0").GetComponent<MeshFilter>().mesh;
                MeshRenderer r0 = lod0.AddComponent<MeshRenderer>();
                r0.material = mat;

                GameObject lod1 = new GameObject();
                lod1.transform.parent = atmo.transform;
                lod1.transform.localPosition = Vector3.zero;
                MeshFilter f1 = lod1.AddComponent<MeshFilter>();
                f1.mesh = GameObject.Find("Atmosphere_LOD1").GetComponent<MeshFilter>().mesh;
                MeshRenderer r1 = lod1.AddComponent<MeshRenderer>();
                r1.material = mat;

                GameObject lod2 = new GameObject();
                lod2.transform.parent = atmo.transform;
                lod2.transform.localPosition = Vector3.zero;
                MeshFilter f2 = lod2.AddComponent<MeshFilter>();
                f2.mesh = GameObject.Find("Atmosphere_LOD2").GetComponent<MeshFilter>().mesh;
                MeshRenderer r2 = lod2.AddComponent<MeshRenderer>();
                r2.material = mat;

                GameObject lod3 = new GameObject();
                lod3.transform.parent = atmo.transform;
                lod3.transform.localPosition = Vector3.zero;
                MeshFilter f3 = lod3.AddComponent<MeshFilter>();
                f3.mesh = GameObject.Find("Atmosphere_LOD3").GetComponent<MeshFilter>().mesh;
                MeshRenderer r3 = lod3.AddComponent<MeshRenderer>();
                r3.material = mat;

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
            }

            atmoGO.transform.localPosition = Vector3.zero;
            atmoGO.SetActive(true);
        }
    }
}
