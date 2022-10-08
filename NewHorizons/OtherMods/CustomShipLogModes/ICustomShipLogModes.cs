using System;

namespace NewHorizons.OtherMods.CustomShipLogModes;

public interface ICustomShipLogModesAPI
{
    public void AddMode(ShipLogMode mode, Func<bool> isEnabledSupplier, Func<string> nameSupplier);
}
