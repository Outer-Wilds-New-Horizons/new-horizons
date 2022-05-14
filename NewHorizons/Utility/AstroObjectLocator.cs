using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility
{
    public static class AstroObjectLocator
    {
        private static Dictionary<string, AstroObject> _customAstroObjectDictionary = new Dictionary<string, AstroObject>();

        public static void Init()
        {
            _customAstroObjectDictionary = new Dictionary<string, AstroObject>();
            foreach (AstroObject ao in GameObject.FindObjectsOfType<AstroObject>())
            {
                // Ignore the sun station debris, we handle it as a child of the sun station
                if (ao.gameObject.name == "SS_Debris_Body") continue;

                RegisterCustomAstroObject(ao);
            }
        }

        public static AstroObject GetAstroObject(string name, bool flag = false)
        {
            if (string.IsNullOrEmpty(name)) return null;

            if (_customAstroObjectDictionary.ContainsKey(name))
            {
                return _customAstroObjectDictionary[name];
            }

            // Else check stock names
            var stringID = name.ToUpper().Replace(" ", "_").Replace("'", "");
            if (stringID.Equals("ATTLEROCK")) stringID = "TIMBER_MOON";
            if (stringID.Equals("HOLLOWS_LANTERN")) stringID = "VOLCANIC_MOON";
            if (stringID.Equals("ASH_TWIN")) stringID = "TOWER_TWIN";
            if (stringID.Equals("EMBER_TWIN")) stringID = "CAVE_TWIN";
            if (stringID.Equals("INTERLOPER")) stringID = "COMET";

            string key;
            if (stringID.ToUpper().Replace("_", "").Equals("MAPSATELLITE"))
            {
                key = AstroObject.Name.MapSatellite.ToString();
            }
            else
            {
                key = AstroObject.StringIDToAstroObjectName(stringID).ToString();
            }

            if (_customAstroObjectDictionary.ContainsKey(key))
            {
                return _customAstroObjectDictionary[key];
            }

            // Try again
            if (!flag) return GetAstroObject(name.Replace(" ", ""), true);

            return null;
        }

        public static void RegisterCustomAstroObject(AstroObject ao)
        {
            var key = ao._name == AstroObject.Name.CustomString ? ao.GetCustomName() : ao._name.ToString();

            if (_customAstroObjectDictionary.Keys.Contains(key))
            {
                Logger.LogWarning($"Registering duplicate [{ao.name}] as [{key}]");
                _customAstroObjectDictionary[key] = ao;
            }
            else
            {
                Logger.Log($"Registering [{ao.name}] as [{key}]");
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

        public static GameObject[] GetChildren(AstroObject primary)
        {
            if (primary == null) return new GameObject[0];

            var otherChildren = new List<GameObject>();
            switch (primary.GetAstroObjectName())
            {
                case AstroObject.Name.TowerTwin:
                    otherChildren.Add(GameObject.Find("TimeLoopRing_Body"));
                    break;
                case AstroObject.Name.ProbeCannon:
                    otherChildren.Add(GameObject.Find("NomaiProbe_Body"));
                    otherChildren.Add(GameObject.Find("CannonMuzzle_Body"));
                    otherChildren.Add(GameObject.Find("FakeCannonMuzzle_Body (1)"));
                    otherChildren.Add(GameObject.Find("CannonBarrel_Body"));
                    otherChildren.Add(GameObject.Find("FakeCannonBarrel_Body (1)"));
                    otherChildren.Add(GameObject.Find("Debris_Body (1)"));
                    break;
                case AstroObject.Name.GiantsDeep:
                    otherChildren.Add(GameObject.Find("BrambleIsland_Body"));
                    otherChildren.Add(GameObject.Find("GabbroIsland_Body"));
                    otherChildren.Add(GameObject.Find("QuantumIsland_Body"));
                    otherChildren.Add(GameObject.Find("StatueIsland_Body"));
                    otherChildren.Add(GameObject.Find("ConstructionYardIsland_Body"));
                    otherChildren.Add(GameObject.Find("GabbroShip_Body"));
                    break;
                case AstroObject.Name.WhiteHole:
                    otherChildren.Add(GameObject.Find("WhiteholeStation_Body"));
                    otherChildren.Add(GameObject.Find("WhiteholeStationSuperstructure_Body"));
                    break;
                case AstroObject.Name.TimberHearth:
                    otherChildren.Add(GameObject.Find("MiningRig_Body"));
                    otherChildren.Add(GameObject.Find("Ship_Body"));
                    break;
                case AstroObject.Name.DreamWorld:
                    otherChildren.Add(GameObject.Find("BackRaft_Body"));
                    otherChildren.Add(GameObject.Find("SealRaft_Body"));
                    break;
                // For some dumb reason the sun station doesn't use AstroObject.Name.SunStation
                case AstroObject.Name.CustomString:
                    if(primary._customName.Equals("Sun Station")) otherChildren.Add(GameObject.Find("SS_Debris_Body"));
                    break;
                default:
                    break;
            }

            return otherChildren.ToArray();
        }
    }
}
