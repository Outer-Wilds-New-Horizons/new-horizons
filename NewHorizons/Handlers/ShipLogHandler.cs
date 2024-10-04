using NewHorizons.External;
using NewHorizons.Utility;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        private static string[] _vanillaBodies;
        private static string[] _vanillaBodyIDs;
        private static string[] _moddedFactsIDs;

        public static void Init()
        {
            _nhBodyToEntryIDs = new Dictionary<NewHorizonsBody, List<string>>();
            _entryIDsToNHBody = new Dictionary<string, NewHorizonsBody>();
            _nhBodyToAstroIDs = new Dictionary<NewHorizonsBody, string>();

            GameObject panRoot = SearchUtilities.Find(PAN_ROOT_PATH);
            if (panRoot != null)
            {
                List<GameObject> gameObjects = panRoot.GetAllChildren();
                _vanillaBodies = gameObjects.ConvertAll(g => g.name).ToArray();
                _vanillaBodyIDs = gameObjects.ConvertAll(g => g.GetComponent<ShipLogAstroObject>()?.GetID()).ToArray();
            }
            else
            {
                _vanillaBodies = new string[0];
                _vanillaBodyIDs = new string[0];
            }
        }

        public static void CheckForModdedFacts(ShipLogManager manager)
        {
            List<ShipLogFact> moddedFacts = manager._factList.Where(e => manager._entryDataDict.ContainsKey(e._entryID) == false).ToList();
            _moddedFactsIDs = moddedFacts.ConvertAll(e => e.GetID()).ToArray();
        }

        public static bool IsVanillaAstroID(string astroId)
        {
            return _vanillaBodyIDs.Contains(astroId);
        }

        public static bool IsVanillaBody(NewHorizonsBody body)
        {
            var existingBody = AstroObjectLocator.GetAstroObject(body.Config.name);
            if (existingBody != null && existingBody.GetAstroObjectName() != AstroObject.Name.CustomString)
                return true;

            return _vanillaBodies.Contains(body.Config.name.Replace(" ", ""));
        }

        public static string GetNameFromAstroID(string astroID)
        {
            return CollectionUtilities.KeyByValue(_nhBodyToAstroIDs, astroID)?.Config.name;
        }

        public static NewHorizonsBody GetConfigFromEntryID(string entryID)
        {
            if (_entryIDsToNHBody.ContainsKey(entryID)) return _entryIDsToNHBody[entryID];
            else
            {
                NHLogger.LogError($"Couldn't find NewHorizonsBody that corresponds to {entryID}");
                return null;
            }
        }

        public static bool IsModdedFact(string FactID)
        {
            return _moddedFactsIDs.Contains(FactID);
        }

        public static void AddConfig(string astroID, List<string> entryIDs, NewHorizonsBody body)
        {
            // Nice to be able to just get the AstroID from the body
            if (!_nhBodyToEntryIDs.ContainsKey(body)) _nhBodyToEntryIDs.Add(body, entryIDs);
            else NHLogger.LogWarning($"Possible duplicate shiplog entry {body.Config.name}");

            // AstroID
            if (!_nhBodyToAstroIDs.ContainsKey(body)) _nhBodyToAstroIDs.Add(body, astroID);
            else NHLogger.LogWarning($"Possible duplicate shiplog entry {astroID} for {body.Config.name}");

            // EntryID to Body
            foreach (var entryID in entryIDs)
            {
                if (!_entryIDsToNHBody.ContainsKey(entryID)) _entryIDsToNHBody.Add(entryID, body);
                else NHLogger.LogWarning($"Possible duplicate shiplog entry  {entryID} for {astroID} from NewHorizonsBody {body.Config.name}");
            }
        }

        public static string GetAstroObjectId(NewHorizonsBody body)
        {
            if (_nhBodyToAstroIDs.ContainsKey(body)) return _nhBodyToAstroIDs[body];
            else return body.Config.name;
        }

        public static bool BodyHasEntries(NewHorizonsBody body)
        {
            return _nhBodyToAstroIDs.ContainsKey(body) && _nhBodyToAstroIDs[body].Length > 0;
        }

        public static bool KnowsFact(string fact)
        {
            // Use save data directly so stuff works between systems
            return PlayerData.GetShipLogFactSave(fact)?.revealOrder > -1;
        }
    }
}
