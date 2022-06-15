using NewHorizons.Builder.StarSystem;
using NewHorizons.Components;
using NewHorizons.Utility;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;
using Object = UnityEngine.Object;
namespace NewHorizons.Handlers
{
    public static class SystemCreationHandler
    {
        public static void LoadSystem(NewHorizonsSystem system)
        {
            var skybox = SearchUtilities.Find("Skybox/Starfield");

            if (system.Config.skybox?.destroyStarField ?? false)
            {
                Object.Destroy(skybox);
            }

            if (system.Config.skybox?.assetBundle != null && system.Config.skybox?.path != null)
            {
                SkyboxBuilder.Make(system.Config.skybox, system.Mod);
            }

            if (system.Config.enableTimeLoop)
            {
                var timeLoopController = new GameObject("TimeLoopController");
                timeLoopController.AddComponent<TimeLoopController>();
            }

            AudioClip clip = null;
            if (!string.IsNullOrEmpty(system.Config.travelAudioClip))
            {
                clip = SearchUtilities.FindResourceOfTypeAndName<AudioClip>(system.Config.travelAudioClip);

                if (clip == null)
                {
                    Logger.LogError($"Couldn't get audio from clip [{system.Config.travelAudioClip}]");
                }
            }
            else if (!string.IsNullOrEmpty(system.Config.travelAudioFilePath))
            {
                try
                {
                    clip = AudioUtilities.LoadAudio(system.Mod.ModHelper.Manifest.ModFolderPath + "/" + system.Config.travelAudioFilePath);
                }
                catch { }

                if (clip == null)
                {
                    Logger.LogError($"Couldn't get audio from file [{system.Config.travelAudioFilePath}]");
                }
            }

            if (clip != null)
            {
                var travelSource = Locator.GetGlobalMusicController()._travelSource;
                travelSource._audioLibraryClip = AudioType.None;
                travelSource._clipArrayIndex = 0;
                travelSource._clipArrayLength = 0;
                travelSource._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.MANUAL;
                travelSource.clip = clip;
            }
        }
    }
}
