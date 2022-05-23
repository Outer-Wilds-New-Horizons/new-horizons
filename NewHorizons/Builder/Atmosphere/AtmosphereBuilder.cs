using NewHorizons.External.Modules;
using UnityEngine;
namespace NewHorizons.Builder.Atmosphere
{
    public static class AtmosphereBuilder
    {
        private static readonly int InnerRadius = Shader.PropertyToID("_InnerRadius");
        private static readonly int OuterRadius = Shader.PropertyToID("_OuterRadius");
        private static readonly int SkyColor = Shader.PropertyToID("_SkyColor");

        public static void Make(GameObject planetGO, Sector sector, AtmosphereModule atmosphereModule, float surfaceSize)
        {
            GameObject atmoGO = new GameObject("Atmosphere");
            atmoGO.SetActive(false);
            atmoGO.transform.parent = sector?.transform ?? planetGO.transform;

            if (atmosphereModule.useAtmosphereShader)
            {
                GameObject atmo = GameObject.Instantiate(GameObject.Find("TimberHearth_Body/Atmosphere_TH/AtmoSphere"), atmoGO.transform, true);
                atmo.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
                atmo.transform.localScale = Vector3.one * atmosphereModule.size * 1.2f;
                foreach (var meshRenderer in atmo.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material.SetFloat(InnerRadius, atmosphereModule.clouds != null ? atmosphereModule.size : surfaceSize);
                    meshRenderer.material.SetFloat(OuterRadius, atmosphereModule.size * 1.2f);
                    if (atmosphereModule.atmosphereTint != null)
                        meshRenderer.material.SetColor(SkyColor, atmosphereModule.atmosphereTint);
                }

                atmo.SetActive(true);
            }

            atmoGO.transform.position = planetGO.transform.TransformPoint(Vector3.zero);
            atmoGO.SetActive(true);
        }
    }
}
