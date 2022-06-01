using NewHorizons.Builder.StarSystem;
using NewHorizons.Components;
using NewHorizons.Utility;
using UnityEngine;
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
            if (system.Config.travelAudioClip != null) clip = SearchUtilities.FindResourceOfTypeAndName<AudioClip>(system.Config.travelAudioClip);
            else if (system.Config.travelAudioFilePath != null)
            {
                try
                {
                    clip = AudioUtilities.LoadAudio(system.Mod.ModHelper.Manifest.ModFolderPath + "/" + system.Config.travelAudioFilePath);
                }
                catch (System.Exception e)
                {
                    Utility.Logger.LogError($"Couldn't load audio file {system.Config.travelAudioFilePath} : {e.Message}");
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
