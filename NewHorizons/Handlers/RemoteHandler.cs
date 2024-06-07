using NewHorizons.Utility.OWML;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;


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

        public static bool TryGetPlatformID(string id, out NomaiRemoteCameraPlatform.ID platformID)
        {
            try
            {
                if (!(_customPlatformIDs.TryGetValue(id, out platformID) || EnumUtils.TryParse<NomaiRemoteCameraPlatform.ID>(id, out platformID)))
                {
                    platformID = AddCustomPlatformID(id);
                }
                return true;
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load platform id [{id}]:\n{e}");
                platformID = NomaiRemoteCameraPlatform.ID.None;
                return false;
            }
        }

        public static NomaiRemoteCameraPlatform.ID GetPlatformID(string id)
        {
            NomaiRemoteCameraPlatform.ID platformID = NomaiRemoteCameraPlatform.ID.None;
            if (_customPlatformIDs.TryGetValue(id, out platformID) || EnumUtils.TryParse<NomaiRemoteCameraPlatform.ID>(id, out platformID))
            {
                return platformID;
            }
            else
            {
                return AddCustomPlatformID(id);
            }
        }

        public static NomaiRemoteCameraPlatform.ID AddCustomPlatformID(string id)
        {
            NHLogger.LogVerbose($"Registering new platform id [{id}]");

            var platformID = EnumUtilities.Create<NomaiRemoteCameraPlatform.ID>(id);

            _customPlatformIDs.Add(id, platformID);

            return platformID;
        }
    }
}
