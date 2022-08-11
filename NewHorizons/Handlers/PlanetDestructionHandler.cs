using NewHorizons.Components;
using NewHorizons.Utility;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly string[] _eyeOfTheUniverseBodies = new string[]
        {
            "Eye Of The Universe",
            "Vessel"
        };

        private static readonly string[] _suspendBlacklist = new string[]
        {
            "Player_Body",
            "Ship_Body"
        };

        public static void RemoveStockPlanets()
        {
            if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse")
                RemoveEyeOfTheUniverse();
            else
                RemoveSolarSystem();
        }

        public static void RemoveSolarSystem()
        {
            // Stop the sun from killing the player
            var sunVolumes = SearchUtilities.Find("Sun_Body/Sector_SUN/Volumes_SUN");
            sunVolumes.SetActive(false);

            foreach (var name in _solarSystemBodies)
            {
                var ao = AstroObjectLocator.GetAstroObject(name);
                if (ao != null) Delay.FireInNUpdates(() => RemoveBody(ao, false), 2);
                else Logger.LogError($"Couldn't find [{name}]");
            }

            // Bring the sun back because why not
            Delay.FireInNUpdates(() => { if (Locator.GetAstroObject(AstroObject.Name.Sun).gameObject.activeInHierarchy) { sunVolumes.SetActive(true); } }, 3);
        }

        public static void RemoveEyeOfTheUniverse()
        {
            foreach (var name in _eyeOfTheUniverseBodies)
            {
                var ao = AstroObjectLocator.GetAstroObject(name);
                if (ao != null) Delay.FireInNUpdates(() => RemoveBody(ao, false), 2);
                else Logger.LogError($"Couldn't find [{name}]");
            }
        }

        public static void RemoveBody(AstroObject ao, bool delete = false, List<AstroObject> toDestroy = null)
        {
            Logger.LogVerbose($"Removing [{ao.name}]");

            if (ao.gameObject == null || !ao.gameObject.activeInHierarchy)
            {
                Logger.LogVerbose($"[{ao.name}] was already removed");
                return;
            }

            if (toDestroy == null) toDestroy = new List<AstroObject>();

            if (toDestroy.Contains(ao))
            {
                Logger.LogVerbose($"Possible infinite recursion in RemoveBody: {ao.name} might be it's own primary body?");
                return;
            }

            toDestroy.Add(ao);

            try
            {
                switch(ao._name)
                {
                    case AstroObject.Name.BrittleHollow:
                        RemoveBody(AstroObjectLocator.GetAstroObject(AstroObject.Name.WhiteHole.ToString()), delete, toDestroy);
                        // Might prevent leftover fragments from existing
                        // Might also prevent people from using their own detachable fragments however
                        foreach(var fragment in GameObject.FindObjectsOfType<DetachableFragment>())
                        {
                            DisableBody(fragment.gameObject, delete);
                        }
                        break;
                    case AstroObject.Name.CaveTwin:
                    case AstroObject.Name.TowerTwin:
                        DisableBody(SearchUtilities.Find("FocalBody"), delete);
                        DisableBody(SearchUtilities.Find("SandFunnel_Body", false), delete);
                        break;
                    case AstroObject.Name.GiantsDeep:
                        // Might prevent leftover jellyfish from existing
                        // Might also prevent people from using their own jellyfish however
                        foreach (var jelly in GameObject.FindObjectsOfType<JellyfishController>())
                        {
                            DisableBody(jelly.gameObject, delete);
                        }
                        // Else it will re-eanble the pieces
                        // ao.GetComponent<OrbitalProbeLaunchController>()._realDebrisSectorProxies = null;
                        break;
                    case AstroObject.Name.TimberHearth:
                        // Always just fucking kill this one to stop THE WARP BUG!!!
                        DisableBody(SearchUtilities.Find("StreamingGroup_TH"), true);

                        foreach (var obj in GameObject.FindObjectsOfType<DayNightTracker>())
                        {
                            DisableBody(obj.gameObject, true);
                        }
                        foreach (var obj in GameObject.FindObjectsOfType<VillageMusicVolume>())
                        {
                            DisableBody(obj.gameObject, true);
                        }
                        break;
                    case AstroObject.Name.Sun:
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
                            Logger.LogVerbose($"Destroying SunProxy {sunProxy.gameObject.name}");
                            GameObject.Destroy(sunProxy.gameObject);
                        }

                        // Stop the sun from breaking stuff when the supernova gets triggered
                        GlobalMessenger.RemoveListener("TriggerSupernova", ao.GetComponent<SunController>().OnTriggerSupernova);
                        break;
                }

                // Always delete the children
                Logger.LogVerbose($"Removing Children of [{ao._name}], [{ao._customName}]");
                foreach (var child in AstroObjectLocator.GetChildren(ao))
                {
                    if (child == null) continue;

                    Logger.LogVerbose($"Removing child [{child.name}] of [{ao._name}]");

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
                Logger.LogError($"Exception thrown when trying to delete bodies related to [{ao.name}]:\n{e}");
            }

            // Deal with proxies
            foreach (var p in GameObject.FindObjectsOfType<ProxyOrbiter>())
            {
                if (p._originalBody == ao.gameObject)
                {
                    DisableBody(p.gameObject, true);
                    break;
                }
            }
            RemoveProxy(ao.name.Replace("_Body", ""));

            Delay.RunWhen(() => Main.IsSystemReady, () => DisableBody(ao.gameObject, delete));

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

        private static bool CanSuspend(OWRigidbody rigidbody, string name)
        {
            if (rigidbody.transform.name == name) return true;
            if (rigidbody._origParentBody == null) return false;
            return CanSuspend(rigidbody._origParentBody, name);
        }

        internal static void DisableBody(GameObject go, bool delete)
        {
            if (go == null) return;

            Logger.LogVerbose($"Removing [{go.name}]");

            OWRigidbody rigidbody = go.GetComponent<OWRigidbody>();
            if (rigidbody != null)
            {
                string name = rigidbody.transform.name;
                foreach (var ow in CenterOfTheUniverse.s_rigidbodies)
                {
                    if (_suspendBlacklist.Contains(ow.transform.name)) continue;
                    if (ow.GetComponent<AstroObject>() != null) continue;
                    if (ow._origParentBody != null)
                    {
                        if (CanSuspend(ow, name))
                        {
                            ow.Suspend();
                        }
                    }
                    else if (ow._simulateInSector != null)
                    {
                        if (CanSuspend(ow._simulateInSector.GetAttachedOWRigidbody(), name))
                        {
                            ow.Suspend();
                        }
                    }
                }
            }

            if (delete)
            {
                GameObject.Destroy(go);
            }
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
            var distantProxy = SearchUtilities.Find(name + "_DistantProxy", false);
            var distantProxyClone = SearchUtilities.Find(name + "_DistantProxy(Clone)", false);

            if (distantProxy != null) GameObject.Destroy(distantProxy.gameObject);
            if (distantProxyClone != null) GameObject.Destroy(distantProxyClone.gameObject);

            if (distantProxy == null && distantProxyClone == null)
                Logger.LogVerbose($"Couldn't find proxy for {name}");
        }
    }
}
