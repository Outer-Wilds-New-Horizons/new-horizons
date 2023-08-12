using NewHorizons.Utility.OWML;
using OWML.Utils;
using System;

namespace NewHorizons.Handlers
{
    public static class ShuttleHandler
    {
        public static NomaiShuttleController.ShuttleID GetShuttleID(string id)
        {
            try
            {
                if (EnumUtils.TryParse(id, out NomaiShuttleController.ShuttleID shuttleID))
                {
                    return shuttleID;
                }
                else
                {
                    return AddCustomShuttleID(id);
                }
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load shuttle id [{id}]:\n{e}");
                return EnumUtils.FromObject<NomaiShuttleController.ShuttleID>(-1);
            }
        }

        public static NomaiShuttleController.ShuttleID AddCustomShuttleID(string id)
        {
            NHLogger.LogVerbose($"Registering new shuttle id [{id}]");

            return EnumUtilities.Create<NomaiShuttleController.ShuttleID>(id);
        }
    }
}
