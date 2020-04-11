using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Marshmallow
{
    public class Main : ModBehaviour
    {
        GameObject generatedPlanet;
        public static OWRigidbody OWRB;
        public static Sector SECTOR;
        public static SpawnPoint SPAWN;

        public static IModHelper helper;

        void Start()
        {
            base.ModHelper.Events.Subscribe<Flashlight>(Events.AfterStart);
            IModEvents events = base.ModHelper.Events;
            events.OnEvent = (Action<MonoBehaviour, Events>)Delegate.Combine(events.OnEvent, new Action<MonoBehaviour, Events>(this.OnEvent));

            helper = base.ModHelper;
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev)
        {
            bool flag = behaviour.GetType() == typeof(Flashlight) && ev == Events.AfterStart;
            if (flag)
            {
                PlanetStructure inputStructure = new PlanetStructure
                {
                    name = "invisibleplanet",

                    primaryBody = Locator.GetAstroObject(AstroObject.Name.Sun),
                    aoType = AstroObject.Type.Planet,
                    aoName = AstroObject.Name.InvisiblePlanet,

                    position = new Vector3(0, 0, 30000),

                    makeSpawnPoint = true,

                    hasClouds = true,
                    topCloudSize = 650f,
                    bottomCloudSize = 600f,
                    cloudTint = new Color32(0, 75, 15, 128),

                    hasWater = true,
                    waterSize = 401f,

                    hasRain = true,

                    hasGravity = true,
                    surfaceAccel = 12f,

                    hasMapMarker = true,

                    hasFog = true,
                    fogTint = new Color32(0, 75, 15, 128),
                    fogDensity = 0.75f,

                    hasOrbit = true
                };

                generatedPlanet = GenerateBody(inputStructure);

                if (inputStructure.primaryBody = Locator.GetAstroObject(AstroObject.Name.Sun))
                {
                    generatedPlanet.transform.parent = Locator.GetRootTransform();
                }
                else
                {
                    generatedPlanet.transform.parent = inputStructure.primaryBody.transform;
                }

                generatedPlanet.transform.position = inputStructure.position;
                generatedPlanet.SetActive(true);
            }
        }

        private GameObject GenerateBody(PlanetStructure planet)
        {
            float groundScale = 400f;

            GameObject body;

            body = new GameObject();
            body.name = planet.name;
            body.SetActive(false);

            Body.MakeGeometry.Make(body, groundScale);

            General.MakeOrbitingAstroObject.Make(body, planet.primaryBody, 0.02f, planet.hasGravity, planet.surfaceAccel, groundScale);
            General.MakeRFVolume.Make(body);

            if (planet.hasMapMarker)
            {
                General.MakeMapMarker.Make(body);
            }

            SECTOR = Body.MakeSector.Make(body, planet.topCloudSize.Value);

            if (planet.hasClouds)
            {
                Atmosphere.MakeClouds.Make(body, planet.topCloudSize.Value, planet.bottomCloudSize.Value, planet.cloudTint.Value);
                Atmosphere.MakeSunOverride.Make(body, planet.topCloudSize.Value, planet.bottomCloudSize.Value, planet.waterSize.Value);
            }

            Atmosphere.MakeAir.Make(body, planet.topCloudSize.Value / 2, planet.hasRain);

            if (planet.hasWater)
            {
                Body.MakeWater.Make(body, planet.waterSize.Value);
            }

            Atmosphere.MakeBaseEffects.Make(body);
            Atmosphere.MakeVolumes.Make(body, groundScale, planet.topCloudSize.Value);
            General.MakeAmbientLight.Make(body);
            Atmosphere.MakeAtmosphere.Make(body, planet.topCloudSize.Value, planet.hasFog, planet.fogDensity, planet.fogTint);

            if (planet.makeSpawnPoint)
            {
                SPAWN = General.MakeSpawnPoint.Make(body, new Vector3(0, groundScale+10, 0));
            }

            return body;
        }

        public static void Log(string text)
        {
            helper.Console.WriteLine("[Marshmallow] : " + text);
        }
    }
}
