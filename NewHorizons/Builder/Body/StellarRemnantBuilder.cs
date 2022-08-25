using NewHorizons.Builder.General;
using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using OWML.Common;
using System;
using UnityEngine;
using Color = UnityEngine.Color;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Body
{
    public static class StellarRemnantBuilder
    {
        public static GameObject Make(GameObject go, OWRigidbody rb, float soi, IModBehaviour mod, NewHorizonsBody star)
        {
            try
            {
                Logger.Log($"Creating stellar remnant for [{star.Config.name}]");

                var sector = SectorBuilder.Make(go, rb, soi);
                sector.name = "StellarRemnant";

                sector.gameObject.SetActive(false);

                var remnantType = star.Config.Star.stellarRemnantType;

                var progenitorSize = star.Config.Star.size;

                if (remnantType == StellarRemnantType.Default)
                {
                    if (progenitorSize >= 4000) remnantType = StellarRemnantType.BlackHole;
                    else if (2000 < progenitorSize && progenitorSize < 4000) remnantType = StellarRemnantType.NeutronStar;
                    else remnantType = StellarRemnantType.WhiteDwarf;
                }

                switch (remnantType)
                {
                    case StellarRemnantType.WhiteDwarf:
                        var whiteDwarfSize = progenitorSize / 10;
                        var wdModule = new StarModule
                        {
                            size = whiteDwarfSize,
                            tint = new MColor(384, 384, 384, 255),
                            lightTint = MColor.white,
                            lightRadius = 10000,
                            solarLuminosity = 0.5f
                        };
                        StarBuilder.Make(go, sector, wdModule, mod, true);

                        break;
                    case StellarRemnantType.NeutronStar:
                        MakeNeutronStar(go, sector, mod, star.Config.Star);

                        break;
                    case StellarRemnantType.Pulsar:
                        MakeNeutronStar(go, sector, mod, star.Config.Star);

                        break;
                    case StellarRemnantType.BlackHole:
                        var blackHoleSize = progenitorSize / 100;

                        SingularityBuilder.MakeBlackHole(go, sector, Vector3.zero, blackHoleSize, true, string.Empty);

                        break;
                    default:
                        break;
                }

                return sector.gameObject;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Couldn't make stellar remnant for [{star.Config.name}]:\n{ex}");
                return null;
            }
        }

        private static GameObject MakeNeutronStar(GameObject root, Sector sector, IModBehaviour mod, StarModule progenitor)
        {
            var neutronStarSize = progenitor.size / 50;
            var neutronStarModule = new StarModule
            {
                size = neutronStarSize,
                tint = new MColor(128, 510, 510, 255),
                lightTint = new MColor(128, 255, 255, 255),
                lightRadius = 10000,
                solarLuminosity = 0.5f,
                hasAtmosphere = false
            };

            // Instead of showing the typical star surface we use a tinted singularity
            var (neutronStar, _, _) = StarBuilder.Make(root, sector, neutronStarModule, mod, true);
            neutronStar.FindChild("Surface").SetActive(false);

            // Modify solar flares
            var flares = neutronStar.GetComponentInChildren<SolarFlareEmitter>();
            flares.lifeLength = 3;
            flares._endScale = 1;
            flares.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

            // Add singularity
            var singularityRenderer = SingularityBuilder.MakeBlackHoleGraphics(root, neutronStarSize * 2.5f);
            singularityRenderer.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 2f, 2f, 1f);

            return neutronStar;
        }
    }
}
