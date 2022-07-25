using NewHorizons.Utility;
using System.IO;
using System.Linq;

namespace NewHorizons.VoiceActing
{
    public static class VoiceHandler
    {
        public static bool Enabled { get; private set; }

        private static IVoiceMod API;

        public static void Init()
        {
            API = Main.Instance.ModHelper.Interaction.TryGetModApi<IVoiceMod>("Krevace.VoiceMod");

            if (API == null)
            {
                Logger.LogVerbose("VoiceMod isn't installed");
                Enabled = false;
                return;
            }

            Enabled = true;

            foreach (var mod in Main.Instance.GetDependants().Append(Main.Instance))
            {
                var folder = $"{mod.ModHelper.Manifest.ModFolderPath}voicemod";
                if (!Directory.Exists(folder)) {
                    // Fallback to PascalCase bc it used to be like that
                    folder = $"{mod.ModHelper.Manifest.ModFolderPath}VoiceMod";
                }
                if (Directory.Exists(folder))
                {
                    Logger.Log($"Registering VoiceMod audio for {mod.ModHelper.Manifest.Name} from {folder}");
                    API.RegisterAssets(folder);
                }
                else
                {
                    Logger.LogVerbose($"Didn't find VoiceMod audio for {mod.ModHelper.Manifest.Name} at {folder}");
                }
            }
        }
    }
}
