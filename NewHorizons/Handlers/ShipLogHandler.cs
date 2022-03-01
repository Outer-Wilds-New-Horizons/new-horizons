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

namespace NewHorizons.Handlers
{
    public static class ShipLogHandler
    {
        public static readonly string PAN_ROOT_PATH = "Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/MapMode/ScaleRoot/PanRoot";

        // NewHorizonsBody -> EntryIDs
        private static Dictionary<NewHorizonsBody, List<string>> _nhBodyToEntryIDs;
        //EntryID -> NewHorizonsBody
        private static Dictionary<string, NewHorizonsBody> _entryIDsToNHBody;
        // NewHorizonsBody -> AstroID
        private static Dictionary<NewHorizonsBody, string> _nhBodyToAstroIDs;

        private static string[] vanillaBodies;
        private static string[] vanillaIDs;

        public static void Init()
        {
            _nhBodyToEntryIDs = new Dictionary<NewHorizonsBody, List<string>>();
            _entryIDsToNHBody = new Dictionary<string, NewHorizonsBody>();
            _nhBodyToAstroIDs = new Dictionary<NewHorizonsBody, string>();

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
            var existingBody = AstroObjectLocator.GetAstroObject(body.Config.Name);
            if (existingBody != null && existingBody.GetAstroObjectName() != AstroObject.Name.CustomString)
                return true;

            return vanillaBodies.Contains(body.Config.Name.Replace(" ", ""));
        }

        public static string GetNameFromAstroID(string astroID)
        {
            return CollectionUtilities.KeyByValue(_nhBodyToAstroIDs, astroID)?.Config.Name;
        }

        public static NewHorizonsBody GetConfigFromEntryID(string entryID)
        {
            if (_entryIDsToNHBody.ContainsKey(entryID)) return _entryIDsToNHBody[entryID];
            else
            {
                Logger.LogError($"Couldn't find NewHorizonsBody that corresponds to {entryID}");
                return null;
            }
        }

        public static void AddConfig(string astroID, List<string> entryIDs, NewHorizonsBody body)
        {
            // Nice to be able to just get the AstroID from the body
            if (!_nhBodyToEntryIDs.ContainsKey(body)) _nhBodyToEntryIDs.Add(body, entryIDs);
            else Logger.LogWarning($"Possible duplicate shiplog entry {body.Config.Name}");

            // AstroID
            if (!_nhBodyToAstroIDs.ContainsKey(body)) _nhBodyToAstroIDs.Add(body, astroID);
            else Logger.LogWarning($"Possible duplicate shiplog entry {astroID} for {body.Config.Name}");

            // EntryID to Body
            foreach (var entryID in entryIDs)
            {
                if (!_entryIDsToNHBody.ContainsKey(entryID)) _entryIDsToNHBody.Add(entryID, body);
                else Logger.LogWarning($"Possible duplicate shiplog entry  {entryID} for {astroID} from NewHorizonsBody {body.Config.Name}");
            }
        }

        public static string GetAstroObjectId(NewHorizonsBody body)
        {
            if (_nhBodyToAstroIDs.ContainsKey(body)) return _nhBodyToAstroIDs[body];
            else return body.Config.Name;
        }

        public static bool BodyHasEntries(NewHorizonsBody body)
        {
            return _nhBodyToAstroIDs.ContainsKey(body) && _nhBodyToAstroIDs[body].Length > 0;
        }
    }
}
