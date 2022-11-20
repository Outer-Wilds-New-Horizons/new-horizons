using NewHorizons.Utility;
using OWML.Common;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class ShuttleHandler
    {
        public static NomaiShuttleController.ShuttleID GetShuttleID(string id)
        {
            try
            {
                NomaiShuttleController.ShuttleID shuttleID;
                if (EnumUtils.TryParse<NomaiShuttleController.ShuttleID>(id, out shuttleID))
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
                Logger.LogError($"Couldn't load shuttle id [{id}]:\n{e}");
                return EnumUtils.FromObject<NomaiShuttleController.ShuttleID>(-1);
            }
        }

        public static NomaiShuttleController.ShuttleID AddCustomShuttleID(string id)
        {
            Logger.LogVerbose($"Registering new shuttle id [{id}]");

            return EnumUtilities.Create<NomaiShuttleController.ShuttleID>(id);
        }
    }
}
