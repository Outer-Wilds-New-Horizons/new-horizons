using NewHorizons.Builder.StarSystem;
using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Utility;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.OWML;
using NewHorizons.Utility.OuterWilds;
using UnityEngine;
using Object = UnityEngine.Object;
using NewHorizons.OtherMods;
using NewHorizons.Components.EOTE;
using Epic.OnlineServices.Presence;

namespace NewHorizons.Handlers
{
    public static class SystemCreationHandler
    {
        public static void LoadSystem(NewHorizonsSystem system)
        {
            if (system.Config.Skybox?.destroyStarField ?? false)
            {
                Object.Destroy(SearchUtilities.Find("Skybox/Starfield"));
            }

            if (system.Config.Skybox?.rightPath != null ||
                system.Config.Skybox?.leftPath != null ||
                system.Config.Skybox?.topPath != null ||
                system.Config.Skybox?.bottomPath != null ||
                system.Config.Skybox?.frontPath != null ||
                system.Config.Skybox?.bottomPath != null)
            {
                SkyboxBuilder.Make(system.Config.Skybox, system.Mod);
            }

            // No time loop or travel audio at the eye
            if (Main.Instance.CurrentStarSystem == "EyeOfTheUniverse") return;

            // Small mod compat change for StopTime - do nothing if it's enabled
            // Do not add our custom time loop controller in the base game system: It will handle itself
            if (Main.Instance.CurrentStarSystem != "SolarSystem" && system.Config.enableTimeLoop && !OtherModUtil.IsEnabled("_nebula.StopTime"))
            {
                var timeLoopController = new GameObject("TimeLoopController");
                timeLoopController.AddComponent<TimeLoopController>();
            }

            if (system.Config.loopDuration != 22f)
            {
                TimeLoopUtilities.SetLoopDuration(system.Config.loopDuration);
            }

            if (system.Config.GlobalMusic != null)
            {
                if (!string.IsNullOrEmpty(system.Config.GlobalMusic.travelAudio))
                {
                    var audioType = AudioTypeHandler.GetAudioType(system.Config.GlobalMusic.travelAudio, system.Mod);
                    Delay.FireOnNextUpdate(() => Locator.GetGlobalMusicController()._travelSource.AssignAudioLibraryClip(audioType));
                }

                if (!string.IsNullOrEmpty(system.Config.GlobalMusic.endTimesAudio))
                {
                    var audioType = AudioTypeHandler.GetAudioType(system.Config.GlobalMusic.endTimesAudio, system.Mod);
                    Delay.FireOnNextUpdate(() => {
                        Locator.GetGlobalMusicController().gameObject.GetAddComponent<DreamWorldEndTimes>().SetEndTimesAudio(audioType);
                        Locator.GetGlobalMusicController()._endTimesSource.AssignAudioLibraryClip(audioType);
                    });
                }

                if (!string.IsNullOrEmpty(system.Config.GlobalMusic.endTimesDreamAudio))
                {
                    var audioType = AudioTypeHandler.GetAudioType(system.Config.GlobalMusic.endTimesDreamAudio, system.Mod);
                    Delay.FireOnNextUpdate(() => Locator.GetGlobalMusicController().gameObject.GetAddComponent<DreamWorldEndTimes>().SetEndTimesDreamAudio(audioType));
                }

                if (!string.IsNullOrEmpty(system.Config.GlobalMusic.brambleDimensionAudio))
                {
                    var audioType = AudioTypeHandler.GetAudioType(system.Config.GlobalMusic.brambleDimensionAudio, system.Mod);
                    Delay.FireOnNextUpdate(() => Locator.GetGlobalMusicController()._darkBrambleSource.AssignAudioLibraryClip(audioType));
                }

                if (!string.IsNullOrEmpty(system.Config.GlobalMusic.finalEndTimesIntroAudio))
                {
                    var audioType = AudioTypeHandler.GetAudioType(system.Config.GlobalMusic.finalEndTimesIntroAudio, system.Mod);
                    Delay.FireOnNextUpdate(() => Locator.GetGlobalMusicController()._finalEndTimesIntroSource.AssignAudioLibraryClip(audioType));
                }

                if (!string.IsNullOrEmpty(system.Config.GlobalMusic.finalEndTimesLoopAudio))
                {
                    var audioType = AudioTypeHandler.GetAudioType(system.Config.GlobalMusic.finalEndTimesLoopAudio, system.Mod);
                    Delay.FireOnNextUpdate(() => Locator.GetGlobalMusicController()._finalEndTimesLoopSource.AssignAudioLibraryClip(audioType));
                }

                if (!string.IsNullOrEmpty(system.Config.GlobalMusic.finalEndTimesBrambleDimensionAudio))
                {
                    var audioType = AudioTypeHandler.GetAudioType(system.Config.GlobalMusic.finalEndTimesBrambleDimensionAudio, system.Mod);
                    Delay.FireOnNextUpdate(() => Locator.GetGlobalMusicController()._finalEndTimesDarkBrambleSource.AssignAudioLibraryClip(audioType));
                }
            }
        }
    }
}
