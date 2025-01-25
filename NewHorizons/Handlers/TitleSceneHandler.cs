using NewHorizons.Builder.Body;
using NewHorizons.Builder.StarSystem;
using NewHorizons.External;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules;
using NewHorizons.External.Modules.Props;
using NewHorizons.Handlers.TitleScreen;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace NewHorizons.Handlers
{
    public static class TitleSceneHandler
    {
        public static void InitSubtitles()
        {
            GameObject subtitleContainer = SearchUtilities.Find("TitleMenu/TitleCanvas/TitleLayoutGroup/Logo_EchoesOfTheEye");
            
            if (subtitleContainer == null)
            {
                NHLogger.LogError("No subtitle container found! Failed to load subtitles.");
                return;
            }

            subtitleContainer.SetActive(true);
            subtitleContainer.AddComponent<SubtitlesHandler>();
        }

        public static void SetUp(IModBehaviour mod, TitleScreenConfig config)
        {
            if (config.menuTextTint != null)
            {
                TitleScreenColourHandler.SetColour(config.menuTextTint.ToColor());
            }

            if (config.Skybox?.destroyStarField ?? false)
            {
                Object.Destroy(SearchUtilities.Find("Skybox/Starfield"));
            }

            if (config.Skybox?.rightPath != null ||
                config.Skybox?.leftPath != null ||
                config.Skybox?.topPath != null ||
                config.Skybox?.bottomPath != null ||
                config.Skybox?.frontPath != null ||
                config.Skybox?.bottomPath != null)
            {
                SkyboxBuilder.Make(config.Skybox, mod);
            }

            if (!string.IsNullOrEmpty(config.music))
            {
                //TODO: Implement
            }

            if (config.MenuPlanet != null)
            {
                if (config.MenuPlanet.destroyMenuPlanet)
                {
                    //TODO: Implement
                }

                if (config.MenuPlanet.removeChildren != null)
                {
                    //TODO: Implement
                    //RemoveChildren(null, config.MenuPlanet.removeChildren);
                }

                if (config.MenuPlanet.details != null)
                {
                    foreach (var simplifiedDetail in config.MenuPlanet.details)
                    {
                        var detail = new DetailInfo(simplifiedDetail);
                        //TODO: Implement
                    }
                }
            }
        }

        private static void RemoveChildren(GameObject go, string[] paths)
        {
            var goPath = go.transform.GetPath();
            var transforms = go.GetComponentsInChildren<Transform>(true);
            foreach (var childPath in paths)
            {
                // Multiple children can have the same path so we delete all that match
                var path = $"{goPath}/{childPath}";

                var flag = true;
                foreach (var childObj in transforms.Where(x => x.GetPath() == path))
                {
                    flag = false;
                    // idk why we wait here but we do
                    Delay.FireInNUpdates(() =>
                    {
                        if (childObj != null && childObj.gameObject != null)
                        {
                            childObj.gameObject.SetActive(false);
                        }
                    }, 2);
                }

                if (flag) NHLogger.LogWarning($"Couldn't find \"{childPath}\".");
            }
        }

        public static void DisplayBodyOnTitleScreen(List<NewHorizonsBody> bodies)
        {
            // Try loading one planet why not
            // var eligible = BodyDict.Values.ToList().SelectMany(x => x).ToList().Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.Cloud != null) && b.Config.Star == null).ToArray();
            var eligible = bodies.Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.clouds != null) && b.Config.Star == null && b.Config.canShowOnTitle).ToArray();
            var eligibleCount = eligible.Count();
            if (eligibleCount == 0) return;

            var selectionCount = Mathf.Min(eligibleCount, 3);
            var indices = RandomUtility.GetUniqueRandomArray(0, eligible.Count(), selectionCount);

            NHLogger.LogVerbose($"Displaying {selectionCount} bodies on the title screen");

            var planetSizes = new List<(GameObject planet, float size)>();

            var bodyInfo = LoadTitleScreenBody(eligible[indices[0]]);
            bodyInfo.planet.transform.localRotation = Quaternion.Euler(15, 0, 0);
            planetSizes.Add(bodyInfo);

            if (selectionCount > 1)
            {
                bodyInfo.planet.transform.localPosition = new Vector3(0, -15, 0);
                bodyInfo.planet.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);

                var bodyInfo2 = LoadTitleScreenBody(eligible[indices[1]]);
                bodyInfo2.planet.transform.localPosition = new Vector3(7, 30, 0);
                bodyInfo2.planet.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                planetSizes.Add(bodyInfo2);
            }

            if (selectionCount > 2)
            {
                var bodyInfo3 = LoadTitleScreenBody(eligible[indices[2]]);
                bodyInfo3.planet.transform.localPosition = new Vector3(-5, 10, 0);
                bodyInfo3.planet.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                planetSizes.Add(bodyInfo3);
            }


            var lightGO = new GameObject("Light");
            lightGO.transform.parent = SearchUtilities.Find("Scene/Background").transform;
            lightGO.transform.localPosition = new Vector3(-47.9203f, 145.7596f, 43.1802f);
            lightGO.transform.localRotation = Quaternion.Euler(13.1412f, 122.8785f, 169.4302f);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.range = float.PositiveInfinity;
            light.intensity = 0.8f;

            // Resize planets relative to each other
            // If there are multiple planets shrink them down to 30% of the size
            var maxSize = planetSizes.Select(x => x.size).Max();
            var minSize = planetSizes.Select(x => x.size).Min();
            var multiplePlanets = planetSizes.Count > 1;
            foreach (var (planet, size) in planetSizes) 
            {
                var adjustedSize = size / maxSize;
                // If some planets would be too small we'll do a funny thing
                if (minSize / maxSize < 0.3f)
                {
                    var t = Mathf.InverseLerp(minSize, maxSize, size);
                    adjustedSize = Mathf.Lerp(0.3f, 1f, t * t);
                }

                planet.transform.localScale *= adjustedSize * (multiplePlanets ? 0.3f : 1f);
            }
        }

        private static (GameObject planet, float size) LoadTitleScreenBody(NewHorizonsBody body)
        {
            NHLogger.LogVerbose($"Displaying {body.Config.name} on the title screen");
            var titleScreenGO = new GameObject(body.Config.name + "_TitleScreen");

            var maxRadius = -1f;

            if (body.Config.HeightMap != null)
            {
                HeightMapBuilder.Make(titleScreenGO, null, body.Config.HeightMap, body.Mod, 30);
                maxRadius = Mathf.Max(maxRadius, body.Config.HeightMap.maxHeight, body.Config.HeightMap.minHeight);
            }

            if (body.Config.Atmosphere?.clouds?.texturePath != null && body.Config.Atmosphere?.clouds?.cloudsPrefab != CloudPrefabType.Transparent)
            {
                // Hacky but whatever I just want a textured sphere
                var cloudTextureMap = new HeightMapModule();
                cloudTextureMap.maxHeight = cloudTextureMap.minHeight = body.Config.Atmosphere.size;
                cloudTextureMap.textureMap = body.Config.Atmosphere.clouds.texturePath;
                HeightMapBuilder.Make(titleScreenGO, null, cloudTextureMap, body.Mod, 30);

                maxRadius = Mathf.Max(maxRadius, cloudTextureMap.maxHeight);
            }

            if (body.Config.Base.groundSize > 0f)
            {
                MakeSphere(titleScreenGO, body.Config.Base.groundSize * 2f);
                maxRadius = Mathf.Max(maxRadius, body.Config.Base.groundSize);
            }

            if (body.Config.Water != null)
            {
                var waterRadius = MakeWater(body, titleScreenGO);
                maxRadius = Mathf.Max(maxRadius, waterRadius);
            }

            if (body.Config.Lava != null)
            {
                var lavaRadius = MakeLava(body, titleScreenGO);
                maxRadius = Mathf.Max(maxRadius, lavaRadius);
            }

            if (body.Config.Sand != null)
            {
                var sandRadius = MakeSand(body, titleScreenGO);
                maxRadius = Mathf.Max(maxRadius, sandRadius);
            }

            if (body.Config.Rings != null && body.Config.Rings.Length > 0)
            {
                foreach (var ring in body.Config.Rings)
                {
                    RingBuilder.Make(titleScreenGO, null, ring, body.Mod);

                    maxRadius = Mathf.Max(maxRadius, ring.outerRadius);
                }
            }

            var pivot = Object.Instantiate(SearchUtilities.Find("Scene/Background/PlanetPivot"), SearchUtilities.Find("Scene/Background").transform);
            pivot.GetComponent<RotateTransform>()._degreesPerSecond = 10f;
            foreach (Transform child in pivot.transform)
            {
                Object.Destroy(child.gameObject);
            }
            pivot.name = "Pivot";

            titleScreenGO.transform.parent = pivot.transform;
            titleScreenGO.transform.localPosition = Vector3.zero;
            titleScreenGO.transform.localScale = Vector3.one * 30f / maxRadius;

            return (titleScreenGO, maxRadius);
        }

        // Returns radius
        private static float MakeWater(NewHorizonsBody body, GameObject root)
        {
            var diameter = 2f * body.Config.Water.size * (body.Config.Water.curve?.FirstOrDefault()?.value ?? 1f);

            var mr = MakeSphere(root, diameter);
            var colour = body.Config.Water.tint?.ToColor() ?? Color.blue;
            mr.material.color = new Color(colour.r, colour.g, colour.b, 0.9f);

            // Make it transparent!
            mr.material.SetOverrideTag("RenderType", "Transparent");
            mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mr.material.SetInt("_ZWrite", 0);
            mr.material.DisableKeyword("_ALPHATEST_ON");
            mr.material.DisableKeyword("_ALPHABLEND_ON");
            mr.material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mr.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            return diameter / 2f;
        }

        private static float MakeLava(NewHorizonsBody body, GameObject root)
        {
            var diameter = 2f * body.Config.Lava.size * (body.Config.Lava.curve?.FirstOrDefault()?.value ?? 1f);

            var mr = MakeSphere(root, diameter);
            mr.material.color = body.Config.Lava.tint?.ToColor() ?? Color.red;
            mr.material.SetColor("_EmissionColor", mr.material.color * 2f);

            return diameter / 2f;
        }

        private static float MakeSand(NewHorizonsBody body, GameObject root)
        {
            var diameter = 2f * body.Config.Sand.size * (body.Config.Sand.curve?.FirstOrDefault()?.value ?? 1f);

            var mr = MakeSphere(root, diameter);
            mr.material.color = body.Config.Sand.tint?.ToColor() ?? Color.yellow;
            mr.material.SetFloat("_Glossiness", 0);

            return diameter / 2f;
        }

        private static MeshRenderer MakeSphere(GameObject root, float diameter)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var meshRenderer = sphere.GetComponent<MeshRenderer>();
            sphere.transform.parent = root.transform;
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = Vector3.one * diameter;

            return meshRenderer;
        }
    }
}
