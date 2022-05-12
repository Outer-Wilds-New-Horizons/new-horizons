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

namespace NewHorizons.Handlers
{
    public static class PlanetDestructionHandler
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

            foreach (var name in _solarSystemBodies)
            {
                var ao = AstroObjectLocator.GetAstroObject(name);
                if (ao != null) Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => RemoveBody(ao, false), 2);
                else Logger.LogError($"Couldn't find [{name}]");
            }

            // Bring the sun back because why not
            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => { if (Locator.GetAstroObject(AstroObject.Name.Sun).gameObject.activeInHierarchy) { sunVolumes.SetActive(true); } }, 3);
        }

        public static void RemoveBody(AstroObject ao, bool delete = false, List<AstroObject> toDestroy = null)
        {
            Logger.Log($"Removing [{ao.name}]");

            if (ao.gameObject == null || !ao.gameObject.activeInHierarchy)
            {
                Logger.Log($"[{ao.name}] was already removed");
                return;
            }

            if (toDestroy == null) toDestroy = new List<AstroObject>();

            if (toDestroy.Contains(ao))
            {
                Logger.LogError($"Possible infinite recursion in RemoveBody: {ao.name} might be it's own primary body?");
                return;
            }

            toDestroy.Add(ao);

            try
            {
                if (ao.GetAstroObjectName() == AstroObject.Name.BrittleHollow)
                {
                    RemoveBody(AstroObjectLocator.GetAstroObject(AstroObject.Name.WhiteHole.ToString()), delete, toDestroy);
                }
                else if (ao.GetAstroObjectName() == AstroObject.Name.CaveTwin || ao.GetAstroObjectName() == AstroObject.Name.TowerTwin)
                {
                    DisableBody(GameObject.Find("FocalBody"), delete);
                }
                else if (ao.GetAstroObjectName() == AstroObject.Name.MapSatellite)
                {
                    DisableBody(GameObject.Find("MapSatellite_Body"), delete);
                }
                else if (ao.GetAstroObjectName() == AstroObject.Name.GiantsDeep)
                {
                    foreach (var jelly in GameObject.FindObjectsOfType<JellyfishController>())
                    {
                        DisableBody(jelly.gameObject, delete);
                    }
                }
                else if (ao.GetAstroObjectName() == AstroObject.Name.TimberHearth)
                {
                    // Always just fucking kill this one to stop THE WARP BUG!!!
                    DisableBody(GameObject.Find("StreamingGroup_TH"), true);

                    foreach (var obj in GameObject.FindObjectsOfType<DayNightTracker>())
                    {
                        DisableBody(obj.gameObject, true);
                    }
                    foreach (var obj in GameObject.FindObjectsOfType<VillageMusicVolume>())
                    {
                        DisableBody(obj.gameObject, true);
                    }
                }
                else if (ao.GetAstroObjectName() == AstroObject.Name.Sun)
                {
                    var starController = ao.gameObject.GetComponent<StarController>();
                    StarLightController.RemoveStar(starController);
                    GameObject.Destroy(starController);

                    var audio = ao.GetComponentInChildren<SunSurfaceAudioController>();
                    GameObject.Destroy(audio);

                    foreach (var owAudioSource in ao.GetComponentsInChildren<OWAudioSource>())
                    {
                        owAudioSource.Stop();
                        GameObject.Destroy(owAudioSource);
                    }

                    foreach (var audioSource in ao.GetComponentsInChildren<AudioSource>())
                    {
                        audioSource.Stop();
                        GameObject.Destroy(audioSource);
                    }

                    foreach (var sunProxy in GameObject.FindObjectsOfType<SunProxy>())
                    {
                        Logger.Log($"Destroying SunProxy {sunProxy.gameObject.name}");
                        GameObject.Destroy(sunProxy.gameObject);
                    }
                }

                // Just delete the children
                foreach (var child in AstroObjectLocator.GetChildren(ao))
                {
                    if (child.name == "Ship_Body") continue;
                    DisableBody(child, true);
                }

                // Delete moons
                foreach (var obj in AstroObjectLocator.GetMoons(ao))
                {
                    RemoveBody(obj.GetComponent<AstroObject>(), false, toDestroy);
                }
            }
            catch (Exception e)
            {
                Logger.LogWarning($"Exception thrown when trying to delete bodies related to [{ao.name}]: {e.Message}, {e.StackTrace}");
            }

            // Deal with proxies
            foreach (var p in GameObject.FindObjectsOfType<ProxyOrbiter>())
            {
                if (p.GetValue<AstroObject>("_originalBody") == ao.gameObject)
                {
                    DisableBody(p.gameObject, true);
                    break;
                }
            }
            RemoveProxy(ao.name.Replace("_Body", ""));

            Main.Instance.ModHelper.Events.Unity.RunWhen(() => Main.IsSystemReady, () => DisableBody(ao.gameObject, delete));

            foreach (ProxyBody proxy in GameObject.FindObjectsOfType<ProxyBody>())
            {
                if (proxy?._realObjectTransform?.gameObject == ao.gameObject)
                {
                    GameObject.Destroy(proxy.gameObject);
                }
            }
        }

        public static void RemoveAllProxies()
        {
            GameObject.Destroy(GameObject.FindObjectOfType<DistantProxyManager>().gameObject);

            foreach (var name in _solarSystemBodies)
            {
                RemoveProxy(name.Replace(" ", "").Replace("'", ""));
            }
        }

        private static void DisableBody(GameObject go, bool delete)
        {
            if (go == null) return;

            if (delete) GameObject.Destroy(go);
            else
            {
                go.SetActive(false);
                var ol = go.GetComponentInChildren<OrbitLine>();
                if (ol)
                {
                    ol.enabled = false;
                }
            }
        }

        private static void RemoveProxy(string name)
        {
            if (name.Equals("TowerTwin")) name = "AshTwin";
            if (name.Equals("CaveTwin")) name = "EmberTwin";
            var distantProxy = GameObject.Find(name + "_DistantProxy");
            var distantProxyClone = GameObject.Find(name + "_DistantProxy(Clone)");

            if (distantProxy != null) GameObject.Destroy(distantProxy.gameObject);
            if (distantProxyClone != null) GameObject.Destroy(distantProxyClone.gameObject);

            if (distantProxy == null && distantProxyClone == null)
                Logger.Log($"Couldn't find proxy for {name}");
        }
    }
}
