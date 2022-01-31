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
		private static AstroObject _timberMoon;
		private static AstroObject _volcanicMoon;
		private static AstroObject _sunStation;
		private static AstroObject _mapSatellite;
		private static Dictionary<string, AstroObject> _customAstroObjectDictionary = new Dictionary<string, AstroObject>();

		private static List<AstroObject> _list = new List<AstroObject>();

		public static AstroObject GetAstroObject(string name, bool flag = false)
        {
			if (_customAstroObjectDictionary.ContainsKey(name)) return _customAstroObjectDictionary[name];

			var stringID = name.ToUpper().Replace(" ", "_").Replace("'", "");
			if (stringID.Equals("ATTLEROCK")) stringID = "TIMBER_MOON";
			if (stringID.Equals("HOLLOWS_LANTERN")) stringID = "VOLCANIC_MOON";
			if (stringID.Equals("ASH_TWIN")) stringID = "TOWER_TWIN";
			if (stringID.Equals("EMBER_TWIN")) stringID = "CAVE_TWIN";
			if (stringID.Equals("INTERLOPER")) stringID = "COMET";

			if (stringID.ToUpper().Replace("_", "").Equals("MAPSATELLITE"))
				return GetAstroObject(AstroObject.Name.MapSatellite);
			
			var aoName = AstroObject.StringIDToAstroObjectName(stringID);
			if (aoName != AstroObject.Name.None && aoName != AstroObject.Name.CustomString) return GetAstroObject(aoName);

			// Try again
			if (!flag) return GetAstroObject(name.Replace(" ", ""), true);

			return null;
		}

		public static AstroObject GetAstroObject(AstroObject.Name astroObjectName, string customName = null)
		{
			switch (astroObjectName)
			{
				case AstroObject.Name.Sun:
				case AstroObject.Name.CaveTwin:
				case AstroObject.Name.TowerTwin:
				case AstroObject.Name.TimberHearth:
				case AstroObject.Name.BrittleHollow:
				case AstroObject.Name.GiantsDeep:
				case AstroObject.Name.DarkBramble:
				case AstroObject.Name.Comet:
				case AstroObject.Name.WhiteHole:
				case AstroObject.Name.WhiteHoleTarget:
				case AstroObject.Name.QuantumMoon:
				case AstroObject.Name.RingWorld:
				case AstroObject.Name.ProbeCannon:
				case AstroObject.Name.DreamWorld:
					return Locator.GetAstroObject(astroObjectName);
				case AstroObject.Name.TimberMoon:
					if (_timberMoon == null) _timberMoon = GameObject.Find("Moon_Body")?.gameObject?.GetComponent<AstroObject>();
					return _timberMoon;
				case AstroObject.Name.VolcanicMoon:
					if (_volcanicMoon == null) _volcanicMoon = GameObject.Find("VolcanicMoon_Body")?.gameObject?.GetComponent<AstroObject>();
					return _volcanicMoon;
				case AstroObject.Name.SunStation:
					if (_sunStation == null) _sunStation = GameObject.Find("SunStation_Body")?.gameObject?.GetComponent<AstroObject>();
					return _sunStation;
				case AstroObject.Name.MapSatellite:
					if (_mapSatellite == null) _mapSatellite = GameObject.Find("HearthianMapSatellite_Body")?.gameObject?.GetComponent<AstroObject>();
					return _mapSatellite;
				case AstroObject.Name.CustomString:
					return _customAstroObjectDictionary[customName];
			}
			return null;
		}

		public static void RegisterCustomAstroObject(AstroObject ao)
		{
			if (ao.GetAstroObjectName() != AstroObject.Name.CustomString)
			{
				Logger.Log($"Can't register {ao.name} as it's AstroObject.Name isn't CustomString.");
				return;
			}

			var name = ao.GetCustomName();
			if (_customAstroObjectDictionary.Keys.Contains(name)) 
				_customAstroObjectDictionary[name] = ao;
			else 
				_customAstroObjectDictionary.Add(name, ao);
		}

		public static void DeregisterCustomAstroObject(AstroObject ao)
        {
			if (ao.GetAstroObjectName() != AstroObject.Name.CustomString) return;
			_customAstroObjectDictionary.Remove(ao.GetCustomName());
        }

		public static void RefreshList()
        {
			_customAstroObjectDictionary = new Dictionary<string, AstroObject>();
			_list = new List<AstroObject>();
        }

		public static AstroObject[] GetAllAstroObjects()
        {
			return _list.ToArray();
        }

		public static void AddAstroObject(AstroObject ao)
        {
			_list.Add(ao);
        }

		public static void RemoveAstroObject(AstroObject ao)
        {
			_list.Remove(ao);
        }
	}
}
