using System;
using System.Collections.Generic;
using System.Linq;
using NewHorizons.Components.Orbital;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Utility.OuterWilds
{
    public static class AstroObjectLocator
    {
        private static Dictionary<string, AstroObject> _customAstroObjectDictionary = new Dictionary<string, AstroObject>();

        public static void Init()
        {
            _customAstroObjectDictionary = new Dictionary<string, AstroObject>();
            foreach (AstroObject ao in Object.FindObjectsOfType<AstroObject>())
            {
                // Ignore the sun station debris, we handle it as a child of the sun station
                if (ao.gameObject.name == "SS_Debris_Body") continue;

                RegisterCustomAstroObject(ao);
            }
        }

        private static AstroObject.Name GetAstroObjectName(string name)
        {
            // Names are all formated uppercase with _ insted of spaces and without punctuation
            var stringID = name.ToUpper().Replace(" ", "_").Replace("'", "");

            return stringID switch
            {
                // Manually handle some of the human readable names
                "ATTLEROCK" => AstroObject.Name.TimberMoon,
                "HOLLOWS_LANTERN" => AstroObject.Name.VolcanicMoon,
                "ASHTWIN" => AstroObject.Name.TowerTwin,
                "EMBER_TWIN" => AstroObject.Name.CaveTwin,
                "INTERLOPER" => AstroObject.Name.Comet,
                "EYE" or "EYE_OF_THE_UNIVERSE" => AstroObject.Name.Eye,
                "MAP_SATELLITE" => AstroObject.Name.MapSatellite,
                _ => AstroObject.StringIDToAstroObjectName(stringID),
            };
        }

        private static AstroObject SearchForAstroObject(string name)
        {
            if (_customAstroObjectDictionary.TryGetValue(name, out var astroObject))
            {
                return astroObject;
            }
            else
            {
                // If it doesn't contain the name, try the astro object name as the key (for stock bodies only)
                var aoName = GetAstroObjectName(name).ToString();
                if (_customAstroObjectDictionary.TryGetValue(aoName, out astroObject))
                {
                    return astroObject;
                }
                else
                {
                    return null;
                }
            }
        }

        public static AstroObject GetAstroObject(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            return SearchForAstroObject(name) ?? SearchForAstroObject(name.Replace(" ", ""));
        }

        public static void RegisterCustomAstroObject(AstroObject ao)
        {
            var key = ao._name == AstroObject.Name.CustomString ? ao.GetCustomName() : ao._name.ToString();

            if (_customAstroObjectDictionary.ContainsKey(key))
            {
                NHLogger.LogWarning($"Registering duplicate [{ao.name}] as [{key}]");
                _customAstroObjectDictionary[key] = ao;
            }
            else
            {
                NHLogger.LogVerbose($"Registering [{ao.name}] as [{key}]");
                _customAstroObjectDictionary.Add(key, ao);
            }
        }

        public static void DeregisterCustomAstroObject(AstroObject ao)
        {
            var key = ao._name == AstroObject.Name.CustomString ? ao.GetCustomName() : ao._name.ToString();
            _customAstroObjectDictionary.Remove(key);
        }

        public static AstroObject[] GetAllAstroObjects()
        {
            return _customAstroObjectDictionary.Values.ToArray();
        }

        public static GameObject[] GetMoons(AstroObject primary)
        {
            return _customAstroObjectDictionary.Values.Where(x => x._primaryBody == primary).Select(x => x.gameObject).ToArray();
        }

        public static AstroObject[] GetAncestors(AstroObject astroObject)
        {
            List<AstroObject> ancestors = new List<AstroObject>();
            for (AstroObject primaryBody = astroObject._primaryBody; primaryBody != null && !ancestors.Contains(primaryBody); primaryBody = primaryBody._primaryBody)
            {
                ancestors.Add(primaryBody);
            }
            return ancestors.ToArray();
        }

        public static GameObject[] GetChildren(AstroObject primary)
        {
            if (primary == null) return new GameObject[0];

            var otherChildren = new List<GameObject>();
            switch (primary._name)
            {
                case AstroObject.Name.TowerTwin:
                    otherChildren.Add(SearchUtilities.Find("TimeLoopRing_Body"));
                    break;
                case AstroObject.Name.ProbeCannon:
                    otherChildren.Add(SearchUtilities.Find("NomaiProbe_Body"));
                    otherChildren.Add(SearchUtilities.Find("CannonMuzzle_Body"));
                    otherChildren.Add(SearchUtilities.Find("FakeCannonMuzzle_Body (1)"));
                    otherChildren.Add(SearchUtilities.Find("CannonBarrel_Body"));
                    otherChildren.Add(SearchUtilities.Find("FakeCannonBarrel_Body (1)"));
                    otherChildren.Add(SearchUtilities.Find("Debris_Body (1)"));
                    break;
                case AstroObject.Name.GiantsDeep:
                    otherChildren.Add(SearchUtilities.Find("BrambleIsland_Body"));
                    otherChildren.Add(SearchUtilities.Find("GabbroIsland_Body"));
                    otherChildren.Add(SearchUtilities.Find("QuantumIsland_Body"));
                    otherChildren.Add(SearchUtilities.Find("StatueIsland_Body"));
                    otherChildren.Add(SearchUtilities.Find("ConstructionYardIsland_Body"));
                    otherChildren.Add(SearchUtilities.Find("GabbroShip_Body"));
                    break;
                case AstroObject.Name.WhiteHole:
                    otherChildren.Add(SearchUtilities.Find("WhiteholeStation_Body"));
                    otherChildren.Add(SearchUtilities.Find("WhiteholeStationSuperstructure_Body"));
                    break;
                case AstroObject.Name.TimberHearth:
                    otherChildren.Add(SearchUtilities.Find("MiningRig_Body"));
                    otherChildren.Add(SearchUtilities.Find("Ship_Body"));
                    otherChildren.Add(SearchUtilities.Find("ModelRocket_Body"));
                    break;
                case AstroObject.Name.DreamWorld:
                    otherChildren.Add(SearchUtilities.Find("BackRaft_Body"));
                    otherChildren.Add(SearchUtilities.Find("SealRaft_Body"));
                    break;
                case AstroObject.Name.MapSatellite:
                    otherChildren.Add(SearchUtilities.Find("HearthianRecorder_Body"));
                    break;
                case AstroObject.Name.DarkBramble:
                    otherChildren.Add(SearchUtilities.Find("DB_ClusterDimension_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_VesselDimension_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_PioneerDimension_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_HubDimension_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_ExitOnlyDimension_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_EscapePodDimension_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_AnglerNestDimension_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_SmallNest_Body"));
                    otherChildren.Add(SearchUtilities.Find("DB_Elsinore_Body"));
                    break;
                case AstroObject.Name.SunStation:
                    // there are multiple debris with the same name
                    otherChildren.AddRange(Object.FindObjectsOfType<AstroObject>()
                        .Select(x => x.gameObject)
                        .Where(x => x.name == "SS_Debris_Body"));
                    break;
                // Just in case GetChildren runs before sun station's name is changed
                case AstroObject.Name.CustomString:
                    if (primary._customName.Equals("Sun Station"))
                    {
                        // there are multiple debris with the same name
                        otherChildren.AddRange(Object.FindObjectsOfType<AstroObject>()
                            .Select(x => x.gameObject)
                            .Where(x => x.name == "SS_Debris_Body"));
                    }
                    break;
                default:
                    break;
            }

            return otherChildren.ToArray();
        }

        public static string GetPlanetName(AstroObject astroObject)
        {
            if (astroObject != null)
            {
                if (astroObject is NHAstroObject nhAstroObject)
                {
                    var customName = nhAstroObject.GetCustomName();

                    if (!string.IsNullOrWhiteSpace(customName))
                    {
                        return TranslationHandler.GetTranslation(customName, TranslationHandler.TextType.UI, false);
                    }
                }
                else
                {
                    AstroObject.Name astroObjectName = astroObject.GetAstroObjectName();

                    if (astroObjectName - AstroObject.Name.Sun <= 7 || astroObjectName - AstroObject.Name.TimberMoon <= 1)
                    {
                        return AstroObject.AstroObjectNameToString(astroObject.GetAstroObjectName());
                    }
                }
            }

            return "???";
        }
    }
}
