using UnityEngine;
using NewHorizons.Utility;
using NewHorizons.External.Modules;
namespace NewHorizons.Builder.General
{
    public static class AmbientLightBuilder
    {
        public static Light Make(GameObject planetGO, Sector sector, AmbientLightModule config, float surfaceSize)
        {
            var ambientLight = Main.Instance.CurrentStarSystem == "EyeOfTheUniverse" ? SearchUtilities.Find("EyeOfTheUniverse_Body/Sector_EyeOfTheUniverse/SixthPlanet_Root/QuantumMoonProxy_Pivot/QuantumMoonProxy_Root/MoonState_Root/AmbientLight_QM") : SearchUtilities.Find("QuantumMoon_Body/AmbientLight_QM");
            if (ambientLight == null) return null;

            GameObject lightGO = GameObject.Instantiate(ambientLight, sector?.transform ?? planetGO.transform);
            lightGO.transform.position = config.position == null ? planetGO.transform.position : planetGO.transform.TransformPoint(config.position);
            lightGO.name = "AmbientLight";

            var light = lightGO.GetComponent<Light>();
            /*
             * R is inner radius
             * G is shell (1 for shell, 0 for no shell)
             * B is always 1
             * A is falloff exponent
             */

            light.intensity = config.intensity;
            light.range = config.outerRadius ?? surfaceSize * 2;
            var innerRadius = config.innerRadius ?? surfaceSize;
            innerRadius = Mathf.Sqrt(innerRadius / light.range);
            var shell = config.isShell ? 1f : 0f;
            light.color = new Color(innerRadius, shell, 1f, 0.0225f/*from timber hearth*/);

            if (config.tint != null)
            {
                var tint = config.tint.ToColor();
                var cubemap = Main.NHPrivateAssetBundle.LoadAsset<Cubemap>("AmbientLight_QM");
                var cubemapFace = CubemapFace.Unknown;
                for (int i = 0; i < 6; i++)
                {
                    switch (i)
                    {
                        case 0: cubemapFace = CubemapFace.PositiveX; break;
                        case 1: cubemapFace = CubemapFace.NegativeX; break;
                        case 2: cubemapFace = CubemapFace.PositiveY; break;
                        case 3: cubemapFace = CubemapFace.NegativeY; break;
                        case 4: cubemapFace = CubemapFace.PositiveZ; break;
                        case 5: cubemapFace = CubemapFace.NegativeZ; break;
                        default: break;
                    }
                    var sourceColors = cubemap.GetPixels(cubemapFace, 0);
                    var newColors = new Color[sourceColors.Length];
                    for (int j = 0; j < sourceColors.Length; j++)
                    {
                        var grey = sourceColors[j].grayscale * 2;
                        newColors[j] = tint * new Color(grey, grey, grey);
                    }
                    cubemap.SetPixels(newColors, cubemapFace);
                }
                cubemap.Apply();
                light.cookie = cubemap;
            }

            return light;
        }
    }
}
