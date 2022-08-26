using NewHorizons.Builder.General;
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

                if (remnantType == StellarRemnantType.Default) remnantType = GetDefault(star.Config.Star.size);

                switch (remnantType)
                {
                    case StellarRemnantType.WhiteDwarf:
                        MakeWhiteDwarf(go, sector, mod, star.Config.Star);

                        break;
                    case StellarRemnantType.NeutronStar:
                        MakeNeutronStar(go, sector, mod, star.Config.Star);

                        break;
                    case StellarRemnantType.Pulsar:
                        MakeNeutronStar(go, sector, mod, star.Config.Star);
                        // TODO: add jets, up rotation speed (use a RotateTransform on the star instead of changing sidereal period)

                        break;
                    case StellarRemnantType.BlackHole:
                        MakeBlackhole(go, sector, star.Config.Star);

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

        private static StellarRemnantType GetDefault(float progenitorSize)
        {
            if (progenitorSize >= 4000) return StellarRemnantType.BlackHole;
            else if (2000 < progenitorSize && progenitorSize < 4000) return StellarRemnantType.NeutronStar;
            else return StellarRemnantType.WhiteDwarf;
        }

        private static GameObject MakeWhiteDwarf(GameObject planetGO, Sector sector, IModBehaviour mod, StarModule progenitor, GameObject proxy = null)
        {
            var whiteDwarfSize = progenitor.size / 10;
            var whiteDwarfModule = new StarModule
            {
                size = whiteDwarfSize,
                tint = new MColor(384, 384, 384, 255),
                lightTint = MColor.white,
                lightRadius = 10000,
                solarLuminosity = 0.5f
            };
            if (proxy != null) return StarBuilder.MakeStarProxy(planetGO, proxy, whiteDwarfModule, mod, true);
            else return StarBuilder.Make(planetGO, sector, whiteDwarfModule, mod, true).Item1;
        }

        private static GameObject MakeNeutronStar(GameObject planetGO, Sector sector, IModBehaviour mod, StarModule progenitor, GameObject proxy = null)
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
            GameObject neutronStar;
            if (proxy != null) neutronStar = StarBuilder.MakeStarProxy(planetGO, proxy, neutronStarModule, mod, true);
            else (neutronStar, _, _) = StarBuilder.Make(planetGO, sector, neutronStarModule, mod, true);
            neutronStar.FindChild("Surface").SetActive(false);

            // Modify solar flares
            var flares = neutronStar.GetComponentInChildren<SolarFlareEmitter>();
            flares.lifeLength = 3;
            flares._endScale = 1;
            flares.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

            // Add singularity
            var singularityRenderer = SingularityBuilder.MakeBlackHoleGraphics(planetGO, neutronStarSize * 2.5f);
            singularityRenderer.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 2f, 2f, 1f);

            return neutronStar;
        }

        private static GameObject MakeBlackhole(GameObject planetGO, Sector sector, StarModule progenitor, GameObject proxy = null)
        {
            var blackHoleSize = progenitor.size / 100;

            if (proxy != null) return SingularityBuilder.MakeBlackHoleProxy(proxy, Vector3.zero, blackHoleSize);
            else return SingularityBuilder.MakeBlackHole(planetGO, sector, Vector3.zero, blackHoleSize, true, string.Empty);
        }

        public static GameObject MakeProxyRemnant(GameObject planet, GameObject proxy, IModBehaviour mod, StarModule progenitor)
        {
            var remnantType = progenitor.stellarRemnantType;

            if (remnantType == StellarRemnantType.Default) remnantType = GetDefault(progenitor.size);

            switch (remnantType)
            {
                case StellarRemnantType.WhiteDwarf:
                    return MakeWhiteDwarf(planet, null, mod, progenitor, proxy);
                case StellarRemnantType.NeutronStar:
                    return MakeNeutronStar(planet, null, mod, progenitor, proxy);
                case StellarRemnantType.Pulsar:
                    return MakeNeutronStar(planet, null, mod, progenitor, proxy);
                case StellarRemnantType.BlackHole:
                    return MakeBlackhole(planet, null, progenitor, proxy);
                default:
                    Logger.LogError($"Couldn't make proxy remnant for {planet.name}");
                    return null;
            }
        }
    }
}
