using NewHorizons.Builder.General;
using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using NewHorizons.External.Configs;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    public static class StellarRemnantBuilder
    {
        public static StellarRemnantController Make(NewHorizonsBody star, GameObject go, OWRigidbody rb, IModBehaviour mod, NewHorizonsBody stellarRemnant = null)
        {
            try
            {
                var currentSRC = go.GetComponentInChildren<StellarRemnantController>();
                if (currentSRC != null)
                {
                    foreach (var starEvolutionController1 in go.GetComponentsInChildren<StarEvolutionController>(true))
                    {
                        starEvolutionController1.SetStellarRemnantController(currentSRC);
                    }
                    return currentSRC;
                }

                Logger.Log($"Creating stellar remnant for [{star.Config.name}]");

                var config = star.Config;
                var starModule = star.Config.Star;
                var size = starModule.size;
                var sector = SectorBuilder.Make(go, rb, 0);
                sector.name = "StellarRemnant";
                var ss = sector.GetComponent<SphereShape>();

                var stellarRemnantController = sector.gameObject.AddComponent<StellarRemnantController>();
                var starEvolutionController = go.GetComponentInChildren<StarEvolutionController>(true);
                stellarRemnantController.SetStarEvolutionController(starEvolutionController);
                starEvolutionController.SetStellarRemnantController(stellarRemnantController);

                sector.gameObject.SetActive(false);

                var remnantType = starModule.stellarRemnantType;
                if (stellarRemnant != null)
                {
                    remnantType = StellarRemnantType.Custom;
                    stellarRemnantController.SetSurfaceSize(stellarRemnant.Config.Base.surfaceSize);
                    stellarRemnantController.SetSurfaceGravity(stellarRemnant.Config.Base.surfaceGravity);
                    stellarRemnantController.SetSiderealPeriod(stellarRemnant.Config.Orbit.siderealPeriod);
                    var srSphereOfInfluence = PlanetCreationHandler.GetSphereOfInfluence(stellarRemnant);
                    stellarRemnantController.SetSphereOfInfluence(srSphereOfInfluence);
                    ss.radius = srSphereOfInfluence + 10;
                    var alignmentRadius = stellarRemnant.Config.Atmosphere?.clouds?.outerCloudRadius ?? 1.5f * stellarRemnant.Config.Base.surfaceSize;
                    if (stellarRemnant.Config.Base.surfaceGravity == 0) alignmentRadius = 0;
                    stellarRemnantController.SetAlignmentRadius(alignmentRadius);
                    PlanetCreationHandler.SharedGenerateBody(stellarRemnant, go, sector, rb);
                    Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
                    {
                        var proxyController = ProxyHandler.GetProxy(star.Config.name);
                        if (proxyController != null)
                        {
                            GameObject proxyStellarRemnant = new GameObject("StellarRemnant");
                            proxyStellarRemnant.transform.SetParent(proxyController.transform, false);
                            proxyStellarRemnant.SetActive(false);
                            StellarRemnantProxy srp = proxyStellarRemnant.AddComponent<StellarRemnantProxy>();
                            srp.SetStellarRemnantController(stellarRemnantController);
                            proxyController._stellarRemnant = srp;
                            ProxyBuilder.SharedMake(go, proxyStellarRemnant, null, stellarRemnant, srp);
                            proxyStellarRemnant.SetActive(true);
                        }
                    }, 2);
                }
                else
                {
                    if (remnantType == StellarRemnantType.Default)
                    {
                        if (size >= 4000)
                            remnantType = StellarRemnantType.BlackHole;
                        else if (2000 < size && size < 4000)
                            remnantType = StellarRemnantType.NeutronStar;
                        else
                            remnantType = StellarRemnantType.WhiteDwarf;
                    }
                    switch (remnantType)
                    {
                        case StellarRemnantType.WhiteDwarf:
                            var wdSurfaceSize = size / 10;
                            stellarRemnantController.SetSurfaceSize(wdSurfaceSize);
                            stellarRemnantController.SetSurfaceGravity(config.Base.surfaceGravity * 1.4f);
                            stellarRemnantController.SetSphereOfInfluence(wdSurfaceSize * 2);
                            ss.radius = (wdSurfaceSize * 2) + 10;
                            stellarRemnantController.SetAlignmentRadius(wdSurfaceSize * 1.5f);
                            stellarRemnantController.SetSiderealPeriod(config.Orbit.siderealPeriod);
                            var wdModule = new StarModule
                            {
                                size = wdSurfaceSize,
                                tint = MColor.white,
                                endTint = MColor.black
                            };
                            stellarRemnantController.SetStarController(StarBuilder.Make(go, sector, wdModule, mod, true));
                            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
                            {
                                var proxyController = ProxyHandler.GetProxy(star.Config.name);
                                if (proxyController != null)
                                {
                                    GameObject proxyStellarRemnant = new GameObject("StellarRemnant");
                                    proxyStellarRemnant.transform.SetParent(proxyController.transform, false);
                                    proxyStellarRemnant.SetActive(false);
                                    StellarRemnantProxy srp = proxyStellarRemnant.AddComponent<StellarRemnantProxy>();
                                    srp.SetStellarRemnantController(stellarRemnantController);
                                    srp._realObjectDiameter = wdSurfaceSize;
                                    proxyController._stellarRemnant = srp;
                                    proxyController._star = StarBuilder.MakeStarProxy(go, proxyStellarRemnant, wdModule, mod, true);
                                    proxyStellarRemnant.SetActive(true);
                                }
                            }, 2);
                            break;
                        case StellarRemnantType.NeutronStar:
                            var nsSurfaceSize = size / 50;
                            stellarRemnantController.SetSurfaceSize(nsSurfaceSize);
                            stellarRemnantController.SetSurfaceGravity(config.Base.surfaceGravity * 2);
                            stellarRemnantController.SetSphereOfInfluence(nsSurfaceSize * 2);
                            ss.radius = (nsSurfaceSize * 2) + 10;
                            stellarRemnantController.SetAlignmentRadius(nsSurfaceSize * 1.5f);
                            stellarRemnantController.SetSiderealPeriod(1);
                            var nsModule = new StarModule
                            {
                                size = nsSurfaceSize,
                                tint = MColor.cyan
                            };
                            stellarRemnantController.SetStarController(StarBuilder.Make(go, sector, nsModule, mod, true));
                            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
                            {
                                var proxyController = ProxyHandler.GetProxy(star.Config.name);
                                if (proxyController != null)
                                {
                                    GameObject proxyStellarRemnant = new GameObject("StellarRemnant");
                                    proxyStellarRemnant.transform.SetParent(proxyController.transform, false);
                                    proxyStellarRemnant.SetActive(false);
                                    StellarRemnantProxy srp = proxyStellarRemnant.AddComponent<StellarRemnantProxy>();
                                    srp.SetStellarRemnantController(stellarRemnantController);
                                    srp._realObjectDiameter = nsSurfaceSize;
                                    proxyController._stellarRemnant = srp;
                                    proxyController._star = StarBuilder.MakeStarProxy(go, proxyStellarRemnant, nsModule, mod, true);
                                    proxyStellarRemnant.SetActive(true);
                                }
                            }, 2);
                            break;
                        case StellarRemnantType.Pulsar:
                            var psSurfaceSize = size / 50;
                            stellarRemnantController.SetSurfaceSize(psSurfaceSize);
                            stellarRemnantController.SetSurfaceGravity(config.Base.surfaceGravity * 2);
                            stellarRemnantController.SetSphereOfInfluence(psSurfaceSize * 2);
                            ss.radius = (psSurfaceSize * 2) + 10;
                            stellarRemnantController.SetAlignmentRadius(psSurfaceSize * 1.5f);
                            stellarRemnantController.SetSiderealPeriod(0.5f);
                            var psModule = new StarModule
                            {
                                size = psSurfaceSize,
                                tint = MColor.cyan
                            };
                            stellarRemnantController.SetStarController(StarBuilder.Make(go, sector, psModule, mod, true));
                            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
                            {
                                var proxyController = ProxyHandler.GetProxy(star.Config.name);
                                if (proxyController != null)
                                {
                                    GameObject proxyStellarRemnant = new GameObject("StellarRemnant");
                                    proxyStellarRemnant.transform.SetParent(proxyController.transform, false);
                                    proxyStellarRemnant.SetActive(false);
                                    StellarRemnantProxy srp = proxyStellarRemnant.AddComponent<StellarRemnantProxy>();
                                    srp.SetStellarRemnantController(stellarRemnantController);
                                    srp._realObjectDiameter = psSurfaceSize;
                                    proxyController._stellarRemnant = srp;
                                    proxyController._star = StarBuilder.MakeStarProxy(go, proxyStellarRemnant, psModule, mod, true);
                                    proxyStellarRemnant.SetActive(true);
                                }
                            }, 2);
                            break;
                        case StellarRemnantType.BlackHole:
                            var bhSurfaceSize = size / 100;
                            stellarRemnantController.SetSurfaceSize(bhSurfaceSize);
                            stellarRemnantController.SetSurfaceGravity(config.Base.surfaceGravity * 4);
                            stellarRemnantController.SetSphereOfInfluence(bhSurfaceSize * 2);
                            stellarRemnantController.SetSiderealPeriod(0.25f);
                            ss.radius = (bhSurfaceSize * 2) + 10;
                            stellarRemnantController.SetAlignmentRadius(bhSurfaceSize * 1.5f);
                            SingularityBuilder.MakeBlackHole(go, sector, Vector3.zero, bhSurfaceSize, true, string.Empty);
                            Main.Instance.ModHelper.Events.Unity.FireInNUpdates(() =>
                            {
                                var proxyController = ProxyHandler.GetProxy(star.Config.name);
                                if (proxyController != null)
                                {
                                    GameObject proxyStellarRemnant = new GameObject("StellarRemnant");
                                    proxyStellarRemnant.transform.SetParent(proxyController.transform, false);
                                    proxyStellarRemnant.SetActive(false);
                                    StellarRemnantProxy srp = proxyStellarRemnant.AddComponent<StellarRemnantProxy>();
                                    srp.SetStellarRemnantController(stellarRemnantController);
                                    srp._realObjectDiameter = bhSurfaceSize;
                                    proxyController._stellarRemnant = srp;
                                    ProxyBuilder.MakeBlackHole(proxyStellarRemnant, Vector3.zero, bhSurfaceSize);
                                    proxyStellarRemnant.SetActive(true);
                                }
                            }, 2);
                            break;
                        default:
                            break;
                    }
                }

                stellarRemnantController.SetRemnantType(remnantType);

                return stellarRemnantController;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Couldn't make stellar remnant for [{star.Config.name}]:\n{ex}");
                return null;
            }
        }
    }
}
