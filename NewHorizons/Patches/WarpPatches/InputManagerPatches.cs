using HarmonyLib;

namespace NewHorizons.Patches.WarpPatches;

[HarmonyPatch(typeof(InputManager))]
public static class InputManagerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InputManager.ChangeInputMode))]
    public static bool InputManager_ChangeInputMode(InputManager __instance, InputMode mode)
    {
        // Can't use player state because it is updated after this method is called
        var atFlightConsole = Locator.GetPlayerCameraController()?._shipController?.IsPlayerAtFlightConsole() ?? false;
        // If we're flying the ship don't let it break our input by changing us to another input mode
        if (atFlightConsole && mode == InputMode.Character)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
