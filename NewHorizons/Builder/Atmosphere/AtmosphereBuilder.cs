using NewHorizons.External;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Atmosphere
{
    public static class AtmosphereBuilder
    {
        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmosphereModule, float surfaceSize)
        {
            GameObject atmoGO = new GameObject("Atmosphere");
            atmoGO.SetActive(false);
            atmoGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (atmosphereModule.HasAtmosphere)
            {
                GameObject atmo = GameObject.Instantiate(GameObject.Find("TimberHearth_Body/Atmosphere_TH/AtmoSphere"));
                atmo.transform.parent = atmoGO.transform;
                atmo.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
                atmo.transform.localScale = Vector3.one * atmosphereModule.Size * 1.2f;
                foreach(var meshRenderer in atmo.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material.SetFloat("_InnerRadius", atmosphereModule.Cloud != null ? atmosphereModule.Size : surfaceSize);
                    meshRenderer.material.SetFloat("_OuterRadius", atmosphereModule.Size * 1.2f);
                    if(atmosphereModule.AtmosphereTint != null)
                        meshRenderer.material.SetColor("_SkyColor", atmosphereModule.AtmosphereTint.ToColor());
                }

                atmo.SetActive(true);
            }

            atmoGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            atmoGO.SetActive(true);
        }
    }
}
