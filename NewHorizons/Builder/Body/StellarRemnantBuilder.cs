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
        public static void Make(GameObject go, OWRigidbody rb, PlanetConfig config, IModBehaviour mod, float sphereOfInfluence)
        {
            Logger.Log($"Creating stellar remnant for [{config.name}]");
            try
            {
                var starModule = config.Star;
                var size = starModule.size;
                var sector = SectorBuilder.Make(go, rb, sphereOfInfluence);
                sector.name = "StellarRemnant";
                var ss = sector.GetComponent<SphereShape>();

                var stellarRemnantController = sector.gameObject.AddComponent<StellarRemnantController>();
                var starEvolutionController = go.GetComponentInChildren<StarEvolutionController>(true);
                stellarRemnantController.SetStarEvolutionController(starEvolutionController);
                starEvolutionController.SetStellarRemnantController(stellarRemnantController);

                sector.gameObject.SetActive(false);

                if (starModule.stellarRemnant != null)
                {
                    var srConfig = starModule.stellarRemnant.ConvertToPlanetConfig(config);
                    var srBody = new NewHorizonsBody(srConfig, mod);
                    stellarRemnantController.SetRemnantType(StellarRemnantType.Custom);
                    stellarRemnantController.SetSurfaceSize(starModule.stellarRemnant.Base.surfaceSize);
                    stellarRemnantController.SetSurfaceGravity(starModule.stellarRemnant.Base.surfaceGravity);
                    stellarRemnantController.SetSiderealPeriod(starModule.stellarRemnant.siderealPeriod);
                    var srSphereOfInfluence = PlanetCreationHandler.GetSphereOfInfluence(srBody);
                    stellarRemnantController.SetSphereOfInfluence(srSphereOfInfluence);
                    ss.radius = srSphereOfInfluence + 10;
                    var alignmentRadius = srBody.Config.Atmosphere?.clouds?.outerCloudRadius ?? 1.5f * srBody.Config.Base.surfaceSize;
                    if (srBody.Config.Base.surfaceGravity == 0) alignmentRadius = 0;
                    stellarRemnantController.SetAlignmentRadius(alignmentRadius);
                    PlanetCreationHandler.SharedGenerateBody(srBody, go, sector, rb, true);
                }
                else
                {
                    var remnantType = starModule.stellarRemnantType;
                    if (remnantType == StellarRemnantType.Default)
                    {
                        if (size > 4000)
                            remnantType = StellarRemnantType.BlackHole;
                        else if (2000 < size && size < 3000)
                            remnantType = StellarRemnantType.NeutronStar;
                        else
                            remnantType = StellarRemnantType.WhiteDwarf;
                    }
                    stellarRemnantController.SetRemnantType(starModule.stellarRemnantType);
                    switch (starModule.stellarRemnantType)
                    {
                        case StellarRemnantType.WhiteDwarf:
                            var wdSurfaceSize = size / 10;
                            stellarRemnantController.SetSurfaceSize(wdSurfaceSize);
                            stellarRemnantController.SetSurfaceGravity(config.Base.surfaceGravity * 1.4f);
                            stellarRemnantController.SetSphereOfInfluence(wdSurfaceSize * 2);
                            ss.radius = (wdSurfaceSize * 2) + 10;
                            stellarRemnantController.SetAlignmentRadius(wdSurfaceSize * 1.5f);
                            stellarRemnantController.SetSiderealPeriod(config.Orbit.siderealPeriod);
                            stellarRemnantController.SetStarController(StarBuilder.Make(go, sector, new StarModule
                            {
                                size = wdSurfaceSize,
                                tint = MColor.white,
                                endTint = MColor.black
                            }, mod, true));
                            break;
                        case StellarRemnantType.NeutronStar:
                            var nsSurfaceSize = size / 50;
                            stellarRemnantController.SetSurfaceSize(nsSurfaceSize);
                            stellarRemnantController.SetSurfaceGravity(config.Base.surfaceGravity * 2);
                            stellarRemnantController.SetSphereOfInfluence(nsSurfaceSize * 2);
                            ss.radius = (nsSurfaceSize * 2) + 10;
                            stellarRemnantController.SetAlignmentRadius(nsSurfaceSize * 1.5f);
                            stellarRemnantController.SetSiderealPeriod(1);
                            stellarRemnantController.SetStarController(StarBuilder.Make(go, sector, new StarModule
                            {
                                size = nsSurfaceSize,
                                tint = MColor.cyan
                            }, mod, true));
                            break;
                        case StellarRemnantType.Pulsar:
                            var psSurfaceSize = size / 50;
                            stellarRemnantController.SetSurfaceSize(psSurfaceSize);
                            stellarRemnantController.SetSurfaceGravity(config.Base.surfaceGravity * 2);
                            stellarRemnantController.SetSphereOfInfluence(psSurfaceSize * 2);
                            ss.radius = (psSurfaceSize * 2) + 10;
                            stellarRemnantController.SetAlignmentRadius(psSurfaceSize * 1.5f);
                            stellarRemnantController.SetSiderealPeriod(0.5f);
                            stellarRemnantController.SetStarController(StarBuilder.Make(go, sector, new StarModule
                            {
                                size = psSurfaceSize,
                                tint = MColor.cyan
                            }, mod, true));
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
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Couldn't make stellar remnant for [{config.name}]:\n{ex}");
            }
        }
    }
}
