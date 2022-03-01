using NewHorizons.Components;
using NewHorizons.Utility;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    static class PlanetDestroyer
    {
        private static readonly string[] _solarSystemBodies = new string[]
        {
            "Ash Twin",
            "Attlerock",
            "Brittle Hollow",
            "Dark Bramble",
            "DreamWorld",
            "Ember Twin",
            "Giant's Deep",
            "Hollow's Lantern",
            "Interloper",
            "Map Satellite",
            "Orbital Probe Cannon",
            "Quantum Moon",
            "RingWorld",
            "Sun",
            "Sun Station",
            "Timber Hearth",
            "White Hole"
        };

        public static void RemoveSolarSystem()
        {
            // Stop the sun from killing the player
            var sunVolumes = GameObject.Find("Sun_Body/Sector_SUN/Volumes_SUN");
            sunVolumes.SetActive(false);

            foreach(var name in _solarSystemBodies)
            {
                var ao = AstroObjectLocator.GetAstroObject(name);
                if (ao != null) Main.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() => RemoveBody(ao));
            }

            // Bring the sun back because why not
            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => { if (Locator.GetAstroObject(AstroObject.Name.Sun).gameObject.activeInHierarchy) { sunVolumes.SetActive(true); } }, 2);
        }

        public static void RemoveBody(AstroObject ao, List<AstroObject> toDestroy = null)
        {
            Logger.Log($"Removing {ao.name}");

            if (ao.gameObject == null || !ao.gameObject.activeInHierarchy) return;

            if (toDestroy == null) toDestroy = new List<AstroObject>();

            if (toDestroy.Contains(ao))
            {
                Logger.LogError($"Possible infinite recursion in RemoveBody: {ao.name} might be it's own primary body?");
                return;
            }

            toDestroy.Add(ao);

            if (ao.GetAstroObjectName() == AstroObject.Name.BrittleHollow)
                RemoveBody(AstroObjectLocator.GetAstroObject(AstroObject.Name.WhiteHole), toDestroy);

            // Check if any other objects depend on it and remove them too
            var aoArray = AstroObjectLocator.GetAllAstroObjects();
            foreach (AstroObject obj in aoArray)
            {
                if (obj?.gameObject == null || !obj.gameObject.activeInHierarchy)
                {
                    AstroObjectLocator.RemoveAstroObject(obj);
                    continue;
                }
                if (ao.Equals(obj.GetPrimaryBody()))
                {
                    AstroObjectLocator.RemoveAstroObject(obj);
                    RemoveBody(obj, toDestroy);
                }
            }

            if (ao.GetAstroObjectName() == AstroObject.Name.CaveTwin || ao.GetAstroObjectName() == AstroObject.Name.TowerTwin)
            {
                if (ao.GetAstroObjectName() == AstroObject.Name.TowerTwin)
                    GameObject.Find("TimeLoopRing_Body").SetActive(false);
                var focalBody = GameObject.Find("FocalBody");
                if (focalBody != null) focalBody.SetActive(false);
            }
            else if (ao.GetAstroObjectName() == AstroObject.Name.MapSatellite)
            {
                var msb = GameObject.Find("MapSatellite_Body");
                if (msb != null) msb.SetActive(false);
            }
            else if(ao.GetAstroObjectName() == AstroObject.Name.ProbeCannon)
            {
                GameObject.Find("NomaiProbe_Body").SetActive(false);
                GameObject.Find("CannonMuzzle_Body").SetActive(false);
                GameObject.Find("FakeCannonMuzzle_Body (1)").SetActive(false);
                GameObject.Find("CannonBarrel_Body").SetActive(false);
                GameObject.Find("FakeCannonBarrel_Body (1)").SetActive(false);
                GameObject.Find("Debris_Body (1)").SetActive(false);
            }
            else if(ao.GetAstroObjectName() == AstroObject.Name.SunStation)
            {
                GameObject.Find("SS_Debris_Body").SetActive(false);
            }
            else if(ao.GetAstroObjectName() == AstroObject.Name.GiantsDeep)
            {
                GameObject.Find("BrambleIsland_Body").SetActive(false);
                GameObject.Find("GabbroIsland_Body").SetActive(false);
                GameObject.Find("QuantumIsland_Body").SetActive(false);
                GameObject.Find("StatueIsland_Body").SetActive(false);
                GameObject.Find("ConstructionYardIsland_Body").SetActive(false);
                GameObject.Find("GabbroShip_Body").SetActive(false);
                foreach(var jelly in GameObject.FindObjectsOfType<JellyfishController>())
                {
                    jelly.gameObject.SetActive(false);
                }
            }
            else if(ao.GetAstroObjectName() == AstroObject.Name.WhiteHole)
            {
                GameObject.Find("WhiteholeStation_Body").SetActive(false);
                GameObject.Find("WhiteholeStationSuperstructure_Body").SetActive(false);
            }
            else if(ao.GetAstroObjectName() == AstroObject.Name.TimberHearth)
            {
                GameObject.Find("MiningRig_Body").SetActive(false);

                foreach(var obj in GameObject.FindObjectsOfType<DayNightTracker>())
                {
                    GameObject.Destroy(obj);
                }
                foreach (var obj in GameObject.FindObjectsOfType<VillageMusicVolume>())
                {
                    GameObject.Destroy(obj);
                }
            }
            else if(ao.GetAstroObjectName() == AstroObject.Name.Sun)
            {
                var starController = ao.gameObject.GetComponent<StarController>();
                StarLightController.RemoveStar(starController);
                GameObject.Destroy(starController);

                var audio = ao.GetComponentInChildren<SunSurfaceAudioController>();
                GameObject.Destroy(audio);

                foreach(var owAudioSource in ao.GetComponentsInChildren<OWAudioSource>())
                {
                    owAudioSource.Stop();
                    GameObject.Destroy(owAudioSource);
                }

                foreach (var audioSource in ao.GetComponentsInChildren<AudioSource>())
                {
                    audioSource.Stop();
                    GameObject.Destroy(audioSource);
                }

                foreach(var sunProxy in GameObject.FindObjectsOfType<SunProxy>())
                {
                    Logger.Log($"Destroying SunProxy {sunProxy.gameObject.name}");
                    GameObject.Destroy(sunProxy.gameObject);
                }
            }
            else if(ao.GetAstroObjectName() == AstroObject.Name.DreamWorld)
            {
                GameObject.Find("BackRaft_Body").SetActive(false);
                GameObject.Find("SealRaft_Body").SetActive(false);
            }

            // Deal with proxies
            foreach (var p in GameObject.FindObjectsOfType<ProxyOrbiter>())
            {
                if (p.GetValue<AstroObject>("_originalBody") == ao.gameObject)
                {
                    p.gameObject.SetActive(false);
                    break;
                }
            }
            RemoveProxy(ao.name.Replace("_Body", ""));

            ao.transform.root.gameObject.SetActive(false);

            foreach (ProxyBody proxy in GameObject.FindObjectsOfType<ProxyBody>())
            {
                if (proxy?._realObjectTransform?.gameObject == ao.gameObject)
                {
                    GameObject.Destroy(proxy.gameObject);
                }
            }

            HeavenlyBodyBuilder.Remove(ao);
        }

        public static void RemoveDistantProxyClones()
        {
            GameObject.Destroy(GameObject.FindObjectOfType<DistantProxyManager>().gameObject);

        }

        private static void RemoveProxy(string name)
        {
            if (name.Equals("TowerTwin")) name = "AshTwin";
            if (name.Equals("CaveTwin")) name = "EmberTwin";
            var distantProxy = GameObject.Find(name + "_DistantProxy");
            var distantProxyClone = GameObject.Find(name + "_DistantProxy(Clone)");

            if (distantProxy != null) GameObject.Destroy(distantProxy.gameObject);
            if (distantProxyClone != null) GameObject.Destroy(distantProxyClone.gameObject);
        }
    }
}
