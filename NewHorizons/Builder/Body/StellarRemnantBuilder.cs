using NewHorizons.Builder.General;
using NewHorizons.External;
using NewHorizons.External.SerializableData;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Common;
using System;
using UnityEngine;
using Color = UnityEngine.Color;

namespace NewHorizons.Builder.Body
{
    public static class StellarRemnantBuilder
    {
        public const float whiteDwarfSize = 1000;
        public const float neutronStarSize = 2000;
        public const float blackholeSize = 4000;

        public static GameObject Make(GameObject go, OWRigidbody rb, float soi, IModBehaviour mod, NewHorizonsBody star)
        {
            if (!HasRemnant(star.Config.Star)) return null;

            try
            {
                NHLogger.Log($"Creating stellar remnant for [{star.Config.name}]");

                var sector = SectorBuilder.Make(go, rb, soi);
                sector._idString = star.Config.name;
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
                NHLogger.LogError($"Couldn't make stellar remnant for [{star.Config.name}]:\n{ex}");
                return null;
            }
        }

        private static StellarRemnantType GetDefault(float progenitorSize)
        {
            if (progenitorSize > blackholeSize) return StellarRemnantType.BlackHole;
            else if (neutronStarSize < progenitorSize && progenitorSize <= blackholeSize) return StellarRemnantType.NeutronStar;
            else return StellarRemnantType.WhiteDwarf;
        }

        public static bool HasRemnant(StarModule star) 
        {
            if (star.stellarDeathType == StellarDeathType.None) return false;

            return !(star.stellarRemnantType == StellarRemnantType.Default && star.size <= whiteDwarfSize);
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
            if (proxy != null) return StarBuilder.MakeStarProxy(planetGO, proxy, whiteDwarfModule, mod, true).Item1;
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
            if (proxy != null) neutronStar = StarBuilder.MakeStarProxy(planetGO, proxy, neutronStarModule, mod, true).Item1;
            else (neutronStar, _, _, _) = StarBuilder.Make(planetGO, sector, neutronStarModule, mod, true);
            neutronStar.FindChild("Surface").SetActive(false);

            // Modify solar flares
            var flares = neutronStar.GetComponentInChildren<SolarFlareEmitter>();
            flares.lifeLength = 3;
            flares._endScale = 1;
            flares.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);

            // Add singularity
            var singularityRenderer = SingularityBuilder.MakeSingularityGraphics(neutronStar, true, neutronStarSize, neutronStarSize * 2.5f);
            singularityRenderer.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 2f, 2f, 1f);

            return neutronStar;
        }

        private static GameObject MakeBlackhole(GameObject planetGO, Sector sector, StarModule progenitor, GameObject proxy = null)
        {
            var blackHoleSize = progenitor.size / 100;

            if (proxy != null) return SingularityBuilder.MakeSingularityProxy(proxy, Vector3.zero, true, blackHoleSize, blackHoleSize * 2.5f);
            else return SingularityBuilder.MakeSingularity(planetGO, sector, Vector3.zero, Vector3.zero, true, blackHoleSize, blackHoleSize * 2.5f, true, string.Empty);
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
                case StellarRemnantType.BlackHole:
                    return MakeBlackhole(planet, null, progenitor, proxy);
                default:
                    NHLogger.LogError($"Couldn't make proxy remnant for {planet.name}");
                    return null;
            }
        }
    }
}
