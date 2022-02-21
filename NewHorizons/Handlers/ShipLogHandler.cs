using NewHorizons.Components;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NewHorizons.External;
using NewHorizons.Utility;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Handlers
{
    public static class ShipLogHandler
    {
        public static readonly string PAN_ROOT_PATH = "Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/MapMode/ScaleRoot/PanRoot";

        private static Dictionary<string, List<NewHorizonsBody>> _astroIdToBodies = new Dictionary<string, List<NewHorizonsBody>>();
        private static string[] vanillaBodies;
        private static string[] vanillaIDs;

        public static void Init()
        {
            List<GameObject> gameObjects = SearchUtilities.GetAllChildren(GameObject.Find(PAN_ROOT_PATH));
            vanillaBodies = gameObjects.ConvertAll(g => g.name).ToArray();
            vanillaIDs = gameObjects.ConvertAll(g => g.GetComponent<ShipLogAstroObject>()?.GetID()).ToArray();
        }

        public static bool IsVanillaAstroID(string astroId)
        {
            return vanillaIDs.Contains(astroId);
        }

        public static bool IsVanillaBody(NewHorizonsBody body)
        {
            return vanillaBodies.Contains(body.Config.Name.Replace(" ", ""));
        }

        public static NewHorizonsBody GetConfigFromID(string id)
        {
            return _astroIdToBodies.ContainsKey(id) ? _astroIdToBodies[id][0] : null;
        }

        public static void AddConfig(string id, NewHorizonsBody body)
        {
            if (!_astroIdToBodies.ContainsKey(id))
            {
                _astroIdToBodies.Add(id, new List<NewHorizonsBody>() { body });
            }
            else
            {
                if(!_astroIdToBodies[id].Contains(body))
                    _astroIdToBodies[id].Append(body);
            }
        }

        public static string GetAstroObjectId(NewHorizonsBody body)
        {
            foreach(var id in _astroIdToBodies.Keys)
            {
                if (_astroIdToBodies[id].Contains(body)) return id;                    
            }

            return body.Config.Name;
        }

        public static bool BodyHasEntries(NewHorizonsBody body)
        {
            foreach(var id in _astroIdToBodies.Keys)
            {
                if (_astroIdToBodies[id].Contains(body)) return true;
            }

            return false;
        }
    }
}
