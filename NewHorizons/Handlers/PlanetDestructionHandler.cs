using NewHorizons.Components;
using NewHorizons.Components.Stars;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class PlanetDestructionHandler
    {
        public static readonly string[] _suspendBlacklist = new string[] { "Player_Body", "Probe_Body", "Ship_Body" };

        public static void RemoveStockPlanets()
        {
            if (Main.Instance.CurrentStarSystem != "EyeOfTheUniverse")
            {
                // For some reason disabling planets immediately ruins the creation of everything else
                // However, the sun breaks a lot of stuff sometimes (literally destroys it in its volumes I imagine)
                // Eg, vision torch in Mindscapes
                // TODO: Fix it better by disabling destruction volumes the first few frames maybe
                // Until then
                // I am become death, the destroyer of worlds
                SearchUtilities.Find("Sun_Body").transform.position = Vector3.left * 1000000000f;
            }

            // Adapted from EOTS thanks corby
            var toDisable = new List<GameObject>();

            // Collect all rigid bodies and proxies
            foreach (var rigidbody in CenterOfTheUniverse.s_rigidbodies)
            {
                if (!_suspendBlacklist.Contains(rigidbody.name))
                {
                    toDisable.Add(rigidbody.gameObject);
                }
            }

            Delay.FireInNUpdates(() =>
            {
                foreach (var gameObject in toDisable)
                {
                    // The gameObject can be null, seems to only happen if they don't have the DLC installed
                    // null coalesence doesn't work with game objects so don't use it here
                    if (gameObject != null)
                    {
                        gameObject.SetActive(false);
                    }
                }
                // Kill all non nh proxies
                foreach (var proxy in GameObject.FindObjectsOfType<ProxyBody>())
                {
                    if (proxy is not NHProxy)
                    {
                        proxy.gameObject.SetActive(false);
                    }
                }
                GameObject.FindObjectOfType<SunProxy>().gameObject.SetActive(false);
                
                if (Main.Instance.CurrentStarSystem != "EyeOfTheUniverse")
                {
                    // Since we didn't call RemoveBody on the all planets there are some we have to call here
                    TimberHearthRemoved();
                    GiantsDeepRemoved();
                    SunRemoved();

                    if (Main.HasDLC)
                    {
                        StrangerRemoved();
                        DreamWorldRemoved();
                    }

                    // Put it back at the center of the universe after banishing it else there are weird graphical bugs
                    SearchUtilities.Find("Sun_Body").gameObject.transform.position = Locator._centerOfTheUniverse._staticReferenceFrame.transform.position;
                }

            }, 2); // Have to wait or shit goes wild
        }

        #region Planet specific removals
        public static void StrangerRemoved()
        {
            CloakHandler.FlagStrangerDisabled = true;

            if (Locator._cloakFieldController?.GetComponentInParent<AstroObject>()?.GetAstroObjectName() == AstroObject.Name.RingWorld)
            {
                Locator._cloakFieldController = null;
            }
        }

        private static void DreamWorldRemoved()
        {
            // No you didn't
            // Needs to stay alive so that custom Dreamworlds can use its Dreamworld controller
            // We had a separate dreamworld controller solution before, but that broke Eyes of the Past somehow
            Locator.GetAstroObject(AstroObject.Name.DreamWorld).gameObject.SetActive(true);
            // We thought of disabling the children for consistency: However this broke the tronworld for some reason
            // Basically, leaving the real Dreamworld in is fine since as long as you don't place your own custom dreamworld on top
            // of it, you'll never have it appear when you dont want it to
        }

        private static void SunRemoved()
        {
            var sun = SearchUtilities.Find("Sun_Body").GetComponent<AstroObject>();

            var starController = sun.gameObject.GetComponent<StarController>();
            SunLightEffectsController.RemoveStar(starController);
            SunLightEffectsController.RemoveStarLight(sun.transform.Find("Sector_SUN/Effects_SUN/SunLight").GetComponent<Light>());
            UnityEngine.Object.Destroy(starController);

            var audio = sun.GetComponentInChildren<SunSurfaceAudioController>();
            UnityEngine.Object.Destroy(audio);

            foreach (var owAudioSource in sun.GetComponentsInChildren<OWAudioSource>())
            {
                owAudioSource.Stop();
                UnityEngine.Object.Destroy(owAudioSource);
            }

            foreach (var audioSource in sun.GetComponentsInChildren<AudioSource>())
            {
                audioSource.Stop();
                UnityEngine.Object.Destroy(audioSource);
            }

            foreach (var sunProxy in UnityEngine.Object.FindObjectsOfType<SunProxy>())
            {
                NHLogger.LogVerbose($"Destroying SunProxy {sunProxy.gameObject.name}");
                UnityEngine.Object.Destroy(sunProxy.gameObject);
            }

            // Stop the sun from breaking stuff when the supernova gets triggered
            GlobalMessenger.RemoveListener("TriggerSupernova", sun.GetComponent<SunController>().OnTriggerSupernova);

            // Just to be safe
            SunLightEffectsController.Instance.Update();
        }

        private static void TimberHearthRemoved()
        {
            // Always just fucking kill this one to stop THE WARP BUG!!!
            GameObject.Destroy(SearchUtilities.Find("StreamingGroup_TH").gameObject);

            var timberHearth = SearchUtilities.Find("TimberHearth_Body");
            foreach (var obj in timberHearth.GetComponentsInChildren<DayNightTracker>())
            {
                GameObject.Destroy(obj.gameObject);
            }
            foreach (var obj in timberHearth.GetComponentsInChildren<VillageMusicVolume>())
            {
                GameObject.Destroy(obj.gameObject);
            }
        }

        private static void GiantsDeepRemoved()
        {
            foreach (var jelly in UnityEngine.Object.FindObjectsOfType<JellyfishController>())
            {
                if (jelly.GetSector().GetRootSector().GetName() == Sector.Name.GiantsDeep)
                {
                    DisableGameObject(jelly.gameObject);
                }
            }
        }
        #endregion

        public static void DisableAstroObject(AstroObject ao, List<AstroObject> toDisable = null)
        {
            if (ao.gameObject == null || !ao.gameObject.activeInHierarchy)
            {
                NHLogger.LogVerbose($"[{ao?.name}] was already removed");
                return;
            }

            NHLogger.LogVerbose($"Removing [{ao.name}]");

            toDisable ??= new List<AstroObject>();

            if (toDisable.Contains(ao))
            {
                NHLogger.LogVerbose($"Possible infinite recursion in RemoveBody: {ao.name} might be it's own primary body?");
                return;
            }

            toDisable.Add(ao);

            try
            {
                switch(ao._name)
                {
                    case AstroObject.Name.BrittleHollow:
                        DisableAstroObject(AstroObjectLocator.GetAstroObject(AstroObject.Name.WhiteHole.ToString()), toDisable);
                        // Might prevent leftover fragments from existing
                        // Might also prevent people from using their own detachable fragments however
                        foreach(var fragment in UnityEngine.Object.FindObjectsOfType<DetachableFragment>())
                        {
                            DisableGameObject(fragment.gameObject);
                        }
                        break;

                    case AstroObject.Name.CaveTwin:
                    case AstroObject.Name.TowerTwin:
                        DisableGameObject(SearchUtilities.Find("FocalBody"));
                        DisableGameObject(SearchUtilities.Find("SandFunnel_Body", false));
                        break;

                    case AstroObject.Name.GiantsDeep:
                        GiantsDeepRemoved();
                        break;
                    case AstroObject.Name.TimberHearth:
                        TimberHearthRemoved();
                        break;
                    case AstroObject.Name.Sun:
                        SunRemoved();
                        break;
                    case AstroObject.Name.RingWorld:
                        StrangerRemoved();
                        break;
                    case AstroObject.Name.DreamWorld:
                        DreamWorldRemoved();
                        break;
                }

                // Always delete the children
                NHLogger.LogVerbose($"Removing Children of [{ao._name}], [{ao._customName}]");
                foreach (var child in AstroObjectLocator.GetChildren(ao))
                {
                    if (child == null) continue;

                    NHLogger.LogVerbose($"Removing child [{child.name}] of [{ao._name}]");

                    // Ship starts as a child of TH but obvious we want to keep that
                    if (child.name == "Ship_Body") continue;

                    // Some children might be astro objects and as such can have children of their own
                    var childAO = child.GetComponent<AstroObject>();
                    if (childAO != null)
                    {
                        DisableAstroObject(childAO, toDisable);
                    }
                    else
                    {
                        DisableGameObject(child);
                    }
                }

                // Always delete moons
                foreach (var obj in AstroObjectLocator.GetMoons(ao))
                {
                    if (obj == null) continue;

                    DisableAstroObject(obj.GetComponent<AstroObject>(), toDisable);
                }
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Exception thrown when trying to delete bodies related to [{ao.name}]:\n{e}");
            }

            DisableGameObject(ao.gameObject);
            RemoveProxy(ao);
        }

        private static bool CanSuspend(OWRigidbody rigidbody, string name)
        {
            if (rigidbody.transform.name == name) return true;
            if (rigidbody._origParentBody == null) return false;
            return CanSuspend(rigidbody._origParentBody, name);
        }

        internal static void DisableGameObject(GameObject go)
        {
            if (go == null) return;

            NHLogger.LogVerbose($"Removing [{go.name}]");

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

            go.SetActive(false);
            var ol = go.GetComponentInChildren<OrbitLine>();
            if (ol)
            {
                ol.enabled = false;
            }
        }

        private static void RemoveProxy(AstroObject ao)
        {
            ProxyHandler.GetVanillaProxyBody(ao.transform)?.gameObject?.SetActive(false);
            ProxyHandler.GetVanillaProxyOrbiter(ao.transform)?.gameObject?.SetActive(false);
        }
    }
}
