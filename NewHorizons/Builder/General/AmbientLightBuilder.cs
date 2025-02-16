using UnityEngine;
using NewHorizons.Utility;
using NewHorizons.External.Modules;
using NewHorizons.Utility.Files;
using NewHorizons.Components;

namespace NewHorizons.Builder.General
{
    public static class AmbientLightBuilder
    {
        public static Light Make(GameObject planetGO, Sector sector, AmbientLightModule config, float surfaceSize)
        {
            var ambientLight = Main.Instance.CurrentStarSystem == "EyeOfTheUniverse" ? SearchUtilities.Find("EyeOfTheUniverse_Body/Sector_EyeOfTheUniverse/SixthPlanet_Root/QuantumMoonProxy_Pivot/QuantumMoonProxy_Root/MoonState_Root/AmbientLight_QM") : SearchUtilities.Find("QuantumMoon_Body/AmbientLight_QM");
            if (ambientLight == null) return null;

            GameObject lightGO = Object.Instantiate(ambientLight, sector?.transform ?? planetGO.transform);
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
                var key = $"AmbientLight_QM > tint {tint}";
                if (ImageUtilities.CheckCachedTexture(key, out var existingTexture))
                {
                    light.cookie = existingTexture;
                }
                else
                {
                    var baseCubemap = AssetBundleUtilities.NHPrivateAssetBundle.LoadAsset<Cubemap>("AmbientLight_QM");
                    var cubemap = new Cubemap(baseCubemap.width, baseCubemap.format, baseCubemap.mipmapCount != 1);
                    cubemap.name = key;
                    cubemap.wrapMode = baseCubemap.wrapMode;
                    for (int i = 0; i < 6; i++)
                    {
                        var cubemapFace = (CubemapFace)i;
                        var sourceColors = baseCubemap.GetPixels(cubemapFace);
                        var newColors = new Color[sourceColors.Length];
                        for (int j = 0; j < sourceColors.Length; j++)
                        {
                            var grey = sourceColors[j].grayscale * 2; // looks nicer with multiplier
                            newColors[j] = new Color(grey, grey, grey) * tint;
                        }
                        cubemap.SetPixels(newColors, cubemapFace);
                    }
                    cubemap.Apply();
                    ImageUtilities.TrackCachedTexture(key, cubemap);

                    light.cookie = cubemap;
                }
            }

            return light;
        }
    }
}
