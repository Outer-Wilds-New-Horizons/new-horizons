using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class RemoteHandler
    {
        private static Dictionary<string, NomaiRemoteCameraPlatform.ID> _customPlatformIDs;
        private static readonly int _startingInt = 19;

        public static void Init()
        {
            _customPlatformIDs = new Dictionary<string, NomaiRemoteCameraPlatform.ID>();
        }

        public static string GetPlatformIDName(NomaiRemoteCameraPlatform.ID id)
        {
            foreach (var pair in _customPlatformIDs)
            {
                if (pair.Value == id) return TranslationHandler.GetTranslation(pair.Key, TranslationHandler.TextType.UI);
            }
            return id.ToString();
        }

        public static string GetPlatformIDKey(NomaiRemoteCameraPlatform.ID id)
        {
            foreach (var pair in _customPlatformIDs)
            {
                if (pair.Value == id) return pair.Key;
            }
            return id.ToString();
        }

        public static NomaiRemoteCameraPlatform.ID GetPlatformID(string id)
        {
            try
            {
                NomaiRemoteCameraPlatform.ID platformID;
                if (_customPlatformIDs.TryGetValue(id, out platformID) || Enum.TryParse(id, true, out platformID))
                {
                    return platformID;
                }
                else
                {
                    return AddCustomPlatformID(id);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Couldn't load platform id:\n{e}");
                return NomaiRemoteCameraPlatform.ID.None;
            }
        }

        public static NomaiRemoteCameraPlatform.ID AddCustomPlatformID(string id)
        {
            var platformID = (NomaiRemoteCameraPlatform.ID)_startingInt + _customPlatformIDs.Count();

            Logger.LogVerbose($"Registering custom platform id {id} as {platformID}");

            _customPlatformIDs.Add(id, platformID);

            return platformID;
        }
    }
}
