#region

using System;
using System.Collections.Generic;
using NewHorizons.Components;
using NewHorizons.Utility;
using OWML.Utils;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;

#endregion

namespace NewHorizons.Handlers
{
    public static class PlanetDestructionHandler
    {
        private static readonly string[] _solarSystemBodies =
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
                if (ao != null) Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() => RemoveBody(ao), 2);
                else Logger.LogError($"Couldn't find [{name}]");
            }

            // Bring the sun back because why not
            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
            {
                if (Locator.GetAstroObject(AstroObject.Name.Sun).gameObject.activeInHierarchy)
                    sunVolumes.SetActive(true);
            }, 3);
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
                Logger.LogError(
                    $"Possible infinite recursion in RemoveBody: {ao.name} might be it's own primary body?");
                return;
            }

            toDestroy.Add(ao);

            try
            {
                switch (ao._name)
                {
                    case AstroObject.Name.BrittleHollow:
                        RemoveBody(AstroObjectLocator.GetAstroObject(AstroObject.Name.WhiteHole.ToString()), delete,
                            toDestroy);
                        // Might prevent leftover fragments from existing
                        // Might also prevent people from using their own detachable fragments however
                        foreach (var fragment in Object.FindObjectsOfType<DetachableFragment>())
                            DisableBody(fragment.gameObject, delete);
                        break;
                    case AstroObject.Name.CaveTwin:
                    case AstroObject.Name.TowerTwin:
                        DisableBody(GameObject.Find("FocalBody"), delete);
                        DisableBody(GameObject.Find("SandFunnel_Body"), delete);
                        break;
                    case AstroObject.Name.MapSatellite:
                        DisableBody(GameObject.Find("MapSatellite_Body"), delete);
                        break;
                    case AstroObject.Name.GiantsDeep:
                        // Might prevent leftover jellyfish from existing
                        // Might also prevent people from using their own jellyfish however
                        foreach (var jelly in Object.FindObjectsOfType<JellyfishController>())
                            DisableBody(jelly.gameObject, delete);
                        // Else it will re-eanble the pieces
                        // ao.GetComponent<OrbitalProbeLaunchController>()._realDebrisSectorProxies = null;
                        break;
                    case AstroObject.Name.TimberHearth:
                        // Always just fucking kill this one to stop THE WARP BUG!!!
                        DisableBody(GameObject.Find("StreamingGroup_TH"), true);

                        foreach (var obj in Object.FindObjectsOfType<DayNightTracker>())
                            DisableBody(obj.gameObject, true);
                        foreach (var obj in Object.FindObjectsOfType<VillageMusicVolume>())
                            DisableBody(obj.gameObject, true);
                        break;
                    case AstroObject.Name.Sun:
                        var starController = ao.gameObject.GetComponent<StarController>();
                        StarLightController.RemoveStar(starController);
                        Object.Destroy(starController);

                        var audio = ao.GetComponentInChildren<SunSurfaceAudioController>();
                        Object.Destroy(audio);

                        foreach (var owAudioSource in ao.GetComponentsInChildren<OWAudioSource>())
                        {
                            owAudioSource.Stop();
                            Object.Destroy(owAudioSource);
                        }

                        foreach (var audioSource in ao.GetComponentsInChildren<AudioSource>())
                        {
                            audioSource.Stop();
                            Object.Destroy(audioSource);
                        }

                        foreach (var sunProxy in Object.FindObjectsOfType<SunProxy>())
                        {
                            Logger.Log($"Destroying SunProxy {sunProxy.gameObject.name}");
                            Object.Destroy(sunProxy.gameObject);
                        }

                        // Stop the sun from breaking stuff when the supernova gets triggered
                        GlobalMessenger.RemoveListener("TriggerSupernova",
                            ao.GetComponent<SunController>().OnTriggerSupernova);
                        break;
                }

                // Always delete the children
                Logger.Log($"Removing Children of [{ao._name}], [{ao._customName}]");
                foreach (var child in AstroObjectLocator.GetChildren(ao))
                {
                    if (child == null) continue;

                    Logger.Log($"Removing child [{child.name}] of [{ao._name}]");

                    // Ship starts as a child of TH but obvious we want to keep that
                    if (child.name == "Ship_Body") continue;

                    // Some children might be astro objects and as such can have children of their own
                    var childAO = child.GetComponent<AstroObject>();
                    if (childAO != null) RemoveBody(childAO, false, toDestroy);
                    else DisableBody(child, true);
                }

                // Always delete moons
                foreach (var obj in AstroObjectLocator.GetMoons(ao))
                {
                    if (obj == null) continue;

                    RemoveBody(obj.GetComponent<AstroObject>(), false, toDestroy);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(
                    $"Exception thrown when trying to delete bodies related to [{ao.name}]: {e.Message}, {e.StackTrace}");
            }

            // Deal with proxies
            foreach (var p in Object.FindObjectsOfType<ProxyOrbiter>())
                if (p.GetValue<AstroObject>("_originalBody") == ao.gameObject)
                {
                    DisableBody(p.gameObject, true);
                    break;
                }

            RemoveProxy(ao.name.Replace("_Body", ""));

            Main.Instance.ModHelper.Events.Unity.RunWhen(() => Main.IsSystemReady,
                () => DisableBody(ao.gameObject, delete));

            foreach (var proxy in Object.FindObjectsOfType<ProxyBody>())
                if (proxy?._realObjectTransform?.gameObject == ao.gameObject)
                    Object.Destroy(proxy.gameObject);
        }

        public static void RemoveAllProxies()
        {
            Object.Destroy(Object.FindObjectOfType<DistantProxyManager>().gameObject);

            foreach (var name in _solarSystemBodies) RemoveProxy(name.Replace(" ", "").Replace("'", ""));
        }

        private static void DisableBody(GameObject go, bool delete)
        {
            if (go == null) return;

            Logger.Log($"Removing [{go.name}]");

            if (delete)
            {
                Object.Destroy(go);
            }
            else
            {
                go.SetActive(false);
                var ol = go.GetComponentInChildren<OrbitLine>();
                if (ol) ol.enabled = false;
            }
        }

        private static void RemoveProxy(string name)
        {
            if (name.Equals("TowerTwin")) name = "AshTwin";
            if (name.Equals("CaveTwin")) name = "EmberTwin";
            var distantProxy = GameObject.Find(name + "_DistantProxy");
            var distantProxyClone = GameObject.Find(name + "_DistantProxy(Clone)");

            if (distantProxy != null) Object.Destroy(distantProxy.gameObject);
            if (distantProxyClone != null) Object.Destroy(distantProxyClone.gameObject);

            if (distantProxy == null && distantProxyClone == null)
                Logger.Log($"Couldn't find proxy for {name}");
        }
    }
}