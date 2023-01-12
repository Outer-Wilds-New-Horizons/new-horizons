using NewHorizons.Builder.Body;
using NewHorizons.External.Modules;
using NewHorizons.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class TitleSceneHandler
    {
        public static void InitSubtitles()
        {
            GameObject subtitleContainer = SearchUtilities.Find("TitleMenu/TitleCanvas/TitleLayoutGroup/Logo_EchoesOfTheEye");
            
            if (subtitleContainer == null)
            {
                Logger.LogError("No subtitle container found! Failed to load subtitles.");
                return;
            }

            subtitleContainer.SetActive(true);
            subtitleContainer.AddComponent<SubtitlesHandler>();
        }

        public static void DisplayBodyOnTitleScreen(List<NewHorizonsBody> bodies)
        {
            //Try loading one planet why not
            //var eligible = BodyDict.Values.ToList().SelectMany(x => x).ToList().Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.Cloud != null) && b.Config.Star == null).ToArray();
            var eligible = bodies.Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.clouds != null) && b.Config.Star == null && b.Config.canShowOnTitle).ToArray();
            var eligibleCount = eligible.Count();
            if (eligibleCount == 0) return;

            var selectionCount = Mathf.Min(eligibleCount, 3);
            var indices = RandomUtility.GetUniqueRandomArray(0, eligible.Count(), selectionCount);

            Logger.LogVerbose($"Displaying {selectionCount} bodies on the title screen");

            GameObject body1, body2, body3;

            body1 = LoadTitleScreenBody(eligible[indices[0]]);
            body1.transform.localRotation = Quaternion.Euler(15, 0, 0);
            if (selectionCount > 1)
            {
                body1.transform.localScale = Vector3.one * (body1.transform.localScale.x) * 0.3f;
                body1.transform.localPosition = new Vector3(0, -15, 0);
                body1.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                body2 = LoadTitleScreenBody(eligible[indices[1]]);
                body2.transform.localScale = Vector3.one * (body2.transform.localScale.x) * 0.3f;
                body2.transform.localPosition = new Vector3(7, 30, 0);
                body2.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            }
            if (selectionCount > 2)
            {
                body3 = LoadTitleScreenBody(eligible[indices[2]]);
                body3.transform.localScale = Vector3.one * (body3.transform.localScale.x) * 0.3f;
                body3.transform.localPosition = new Vector3(-5, 10, 0);
                body3.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            }

            SearchUtilities.Find("Scene/Background/PlanetPivot/Prefab_HEA_Campfire").SetActive(false);
            SearchUtilities.Find("Scene/Background/PlanetPivot/PlanetRoot").SetActive(false);

            var lightGO = new GameObject("Light");
            lightGO.transform.parent = SearchUtilities.Find("Scene/Background").transform;
            lightGO.transform.localPosition = new Vector3(-47.9203f, 145.7596f, 43.1802f);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.range = float.PositiveInfinity;
            light.intensity = 0.8f;
        }

        private static GameObject LoadTitleScreenBody(NewHorizonsBody body)
        {
            Logger.LogVerbose($"Displaying {body.Config.name} on the title screen");
            var titleScreenGO = new GameObject(body.Config.name + "_TitleScreen");

            var maxSize = -1f;

            if (body.Config.HeightMap != null)
            {
                HeightMapBuilder.Make(titleScreenGO, null, body.Config.HeightMap, body.Mod, 30);
                maxSize = body.Config.HeightMap.maxHeight;
            }
            if (body.Config.Atmosphere?.clouds?.texturePath != null && body.Config.Atmosphere?.clouds?.cloudsPrefab != CloudPrefabType.Transparent)
            {
                // Hacky but whatever I just want a sphere
                var cloudTextureMap = new HeightMapModule();
                cloudTextureMap.maxHeight = cloudTextureMap.minHeight = body.Config.Atmosphere.size;
                cloudTextureMap.textureMap = body.Config.Atmosphere.clouds.texturePath;
                HeightMapBuilder.Make(titleScreenGO, null, cloudTextureMap, body.Mod, 30);
                maxSize = Mathf.Max(maxSize, cloudTextureMap.maxHeight);
            }
            else
            {
                if (body.Config.Water != null)
                {
                    var waterGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var size = 2f * Mathf.Max(body.Config.Water.size, body.Config.Water.size * body.Config.Water.curve?.FirstOrDefault()?.value ?? 0f);

                    waterGO.transform.localScale = Vector3.one * size;

                    var mr = waterGO.GetComponent<MeshRenderer>();
                    mr.material.color = body.Config.Water.tint?.ToColor() ?? Color.blue;

                    waterGO.transform.parent = titleScreenGO.transform;
                    waterGO.transform.localPosition = Vector3.zero;

                    maxSize = Mathf.Max(maxSize, size);
                }
                if (body.Config.Lava != null)
                {
                    var lavaGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var size = 2f * Mathf.Max(body.Config.Lava.size, body.Config.Lava.size * body.Config.Lava.curve?.FirstOrDefault()?.value ?? 0f);

                    lavaGO.transform.localScale = Vector3.one * size;

                    var mr = lavaGO.GetComponent<MeshRenderer>();
                    mr.material.color = body.Config.Lava.tint?.ToColor() ?? Color.red;

                    lavaGO.transform.parent = titleScreenGO.transform;
                    lavaGO.transform.localPosition = Vector3.zero;

                    maxSize = Mathf.Max(maxSize, size);
                }
                if (body.Config.Sand != null)
                {
                    var sandGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var size = 2f * Mathf.Max(body.Config.Sand.size, body.Config.Sand.size * body.Config.Sand.curve?.FirstOrDefault()?.value ?? 0f);

                    sandGO.transform.localScale = Vector3.one * size;
                    sandGO.GetComponent<MeshRenderer>().material.color = body.Config.Sand.tint?.ToColor() ?? Color.yellow;

                    sandGO.transform.parent = titleScreenGO.transform;
                    sandGO.transform.localPosition = Vector3.zero;

                    maxSize = Mathf.Max(maxSize, size);
                }
            }

            var pivot = GameObject.Instantiate(SearchUtilities.Find("Scene/Background/PlanetPivot"), SearchUtilities.Find("Scene/Background").transform);
            pivot.GetComponent<RotateTransform>()._degreesPerSecond = 10f;
            foreach (Transform child in pivot.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            pivot.name = "Pivot";

            if (body.Config.Rings != null && body.Config.Rings.Length > 0)
            {
                foreach (var ring in body.Config.Rings)
                {
                    RingBuilder.Make(titleScreenGO, null, ring, body.Mod);

                    maxSize = Mathf.Max(maxSize, ring.outerRadius);
                }
            }

            titleScreenGO.transform.parent = pivot.transform;
            titleScreenGO.transform.localPosition = Vector3.zero;
            titleScreenGO.transform.localScale = Vector3.one * 30f / maxSize;

            return titleScreenGO;
        }
    }
}
