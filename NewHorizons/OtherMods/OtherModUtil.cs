namespace NewHorizons.OtherMods;

public static class OtherModUtil
{
    public static bool IsEnabled(string modName) => Main.Instance.ModHelper.Interaction.ModExists(modName);
}
