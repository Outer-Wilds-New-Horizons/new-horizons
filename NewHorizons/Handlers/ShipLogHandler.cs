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

        private static Dictionary<string, NewHorizonsBody> _astroIdToBody = new Dictionary<string, NewHorizonsBody>();
        private static string[] vanillaBodies;

        public static void Init()
        {
            vanillaBodies = SearchUtilities.GetAllChildren(GameObject.Find(PAN_ROOT_PATH)).ConvertAll(g => g.name).ToArray();
        }

        public static bool IsVanillaBody(NewHorizonsBody body)
        {
            return vanillaBodies.Contains(body.Config.Name.Replace(" ", ""));
        }

        public static NewHorizonsBody GetConfigFromID(string id)
        {
            return _astroIdToBody.ContainsKey(id) ? _astroIdToBody[id] : null;
        }

        public static void AddConfig(string id, NewHorizonsBody body)
        {
            if (!_astroIdToBody.ContainsKey(id))
            {
                _astroIdToBody.Add(id, body);
            }
        }

        public static string GetAstroObjectId(NewHorizonsBody body)
        {
            if (BodyHasEntries(body))
            {
                return CollectionUtilities.KeyByValue(_astroIdToBody, body);
            }
            else
            {
                return body.Config.Name;
            }
        }

        public static bool BodyHasEntries(NewHorizonsBody body)
        {
            return _astroIdToBody.ContainsValue(body);
        }
    }
}
