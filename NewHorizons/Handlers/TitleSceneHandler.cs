using NewHorizons.Builder.Body;
using NewHorizons.External;
using NewHorizons.External.VariableSize;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class TitleSceneHandler
    {
        public static void DisplayBodyOnTitleScreen(List<NewHorizonsBody> bodies)
        {
            //Try loading one planet why not
            //var eligible = BodyDict.Values.ToList().SelectMany(x => x).ToList().Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.Cloud != null) && b.Config.Star == null).ToArray();
            var eligible = bodies.Where(b => (b.Config.HeightMap != null || b.Config.Atmosphere?.Cloud != null) && b.Config.Star == null && b.Config.CanShowOnTitle).ToArray();
            var eligibleCount = eligible.Count();
            if (eligibleCount == 0) return;

            var selectionCount = Mathf.Min(eligibleCount, 3);
            var indices = RandomUtility.GetUniqueRandomArray(0, eligible.Count(), selectionCount);

            Logger.Log($"Displaying {selectionCount} bodies on the title screen");

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

            GameObject.Find("Scene/Background/PlanetPivot/Prefab_HEA_Campfire").SetActive(false);
            GameObject.Find("Scene/Background/PlanetPivot/PlanetRoot").SetActive(false);

            var lightGO = new GameObject("Light");
            lightGO.transform.parent = GameObject.Find("Scene/Background").transform;
            lightGO.transform.localPosition = new Vector3(-47.9203f, 145.7596f, 43.1802f);
            var light = lightGO.AddComponent<Light>();
            light.color = new Color(1f, 1f, 1f, 1f);
            light.range = 100;
            light.intensity = 0.8f;
        }

        private static GameObject LoadTitleScreenBody(NewHorizonsBody body)
        {
            Logger.Log($"Displaying {body.Config.Name} on the title screen");
            GameObject titleScreenGO = new GameObject(body.Config.Name + "_TitleScreen");
            HeightMapModule heightMap = new HeightMapModule();
            var minSize = 15;
            var maxSize = 30;
            float size = minSize;
            if (body.Config.HeightMap != null)
            {
                size = Mathf.Clamp(body.Config.HeightMap.MaxHeight / 10, minSize, maxSize);
                heightMap.TextureMap = body.Config.HeightMap.TextureMap;
                heightMap.HeightMap = body.Config.HeightMap.HeightMap;
                heightMap.MaxHeight = size;
                heightMap.MinHeight = body.Config.HeightMap.MinHeight * size / body.Config.HeightMap.MaxHeight;
            }
            if (body.Config.Atmosphere != null && body.Config.Atmosphere.Cloud != null)
            {
                // Hacky but whatever I just want a sphere
                size = Mathf.Clamp(body.Config.Atmosphere.Size / 10, minSize, maxSize);
                heightMap.MaxHeight = heightMap.MinHeight = size + 1;
                heightMap.TextureMap = body.Config.Atmosphere.Cloud;
            }

            HeightMapBuilder.Make(titleScreenGO, heightMap, body.Mod);

            GameObject pivot = GameObject.Instantiate(GameObject.Find("Scene/Background/PlanetPivot"), GameObject.Find("Scene/Background").transform);
            pivot.GetComponent<RotateTransform>()._degreesPerSecond = 10f;
            foreach (Transform child in pivot.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            pivot.name = "Pivot";

            if (body.Config.Ring != null)
            {
                RingModule newRing = new RingModule();
                newRing.InnerRadius = size * 1.2f;
                newRing.OuterRadius = size * 2f;
                newRing.Texture = body.Config.Ring.Texture;
                var ring = RingBuilder.Make(titleScreenGO, newRing, body.Mod);
                titleScreenGO.transform.localScale = Vector3.one * 0.8f;
            }

            titleScreenGO.transform.parent = pivot.transform;
            titleScreenGO.transform.localPosition = Vector3.zero;

            return titleScreenGO;
        }
    }
}
