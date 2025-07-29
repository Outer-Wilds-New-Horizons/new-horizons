using NewHorizons.Handlers;

namespace NewHorizons.OtherMods.CustomShipLogModes;

public static class CustomShipLogModesHandler
{
    private static readonly ICustomShipLogModesAPI API;

    static CustomShipLogModesHandler()
    {
        API = Main.Instance.ModHelper.Interaction.TryGetModApi<ICustomShipLogModesAPI>("dgarro.CustomShipLogModes");
    }

    public static void AddInterstellarMode()
    {
        API.AddMode(StarChartHandler.ShipLogStarChartMode, 
            () => Main.HasWarpDriveFunctionality, 
            () => TranslationHandler.GetTranslation("INTERSTELLAR_MODE", TranslationHandler.TextType.UI));
    }
}
