using NewHorizons.Utility.OWMLUtilities;
using OWML.Utils;
using System;
using System.Collections.Generic;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class RemoteHandler
    {
        private static Dictionary<string, NomaiRemoteCameraPlatform.ID> _customPlatformIDs;

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
                if (_customPlatformIDs.TryGetValue(id, out platformID) || EnumUtils.TryParse<NomaiRemoteCameraPlatform.ID>(id, out platformID))
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
                Logger.LogError($"Couldn't load platform id [{id}]:\n{e}");
                return NomaiRemoteCameraPlatform.ID.None;
            }
        }

        public static NomaiRemoteCameraPlatform.ID AddCustomPlatformID(string id)
        {
            Logger.LogVerbose($"Registering new platform id [{id}]");

            var platformID = EnumUtilities.Create<NomaiRemoteCameraPlatform.ID>(id);

            _customPlatformIDs.Add(id, platformID);

            return platformID;
        }
    }
}
