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
            lightGO.transform.position = config.position ?? planetGO.transform.position;
            lightGO.name = "AmbientLight";

            var light = lightGO.GetComponent<Light>();
            /* R is related to the inner radius of the ambient light volume
             * G is if its a shell or not. 1.0f for shell else 0.0f.
             * B is just 1.0 always I think, altho no because changing it changes the brightness so idk
             * A is the intensity and its like square rooted and squared and idgi
             */

            light.intensity = config.intensity;
            light.range = config.outerRadius ?? surfaceSize * 2;
            var innerRadius = config.innerRadius ?? surfaceSize;
            innerRadius = Mathf.Clamp01(innerRadius / light.range);
            var shell = config.isShell ? 1f : 0f;
            light.color = new Color(innerRadius, shell, 0.8f, 0.0225f);

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
                        var grey = sourceColors[j].grayscale;
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
