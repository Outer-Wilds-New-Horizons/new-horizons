using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using OWML.Utils;
using System;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class DreamHandler
    {
        public static DreamArrivalPoint.Location GetDreamArrivalLocation(string id)
        {
            try
            {
                if (EnumUtils.TryParse(id, out DreamArrivalPoint.Location location))
                {
                    return location;
                }
                else
                {
                    NHLogger.LogVerbose($"Registering new dream arrival location [{id}]");
                    return EnumUtilities.Create<DreamArrivalPoint.Location>(id);
                }
            }
            catch (Exception e)
            {
                NHLogger.LogError($"Couldn't load dream arrival location [{id}]:\n{e}");
                return DreamArrivalPoint.Location.Undefined;
            }
        }
    }
}
