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

            if (!string.IsNullOrEmpty(system.Config.GlobalMusic.travelAudio))
            {
                Delay.FireOnNextUpdate(() => AudioUtilities.SetAudioClip(Locator.GetGlobalMusicController()._travelSource, system.Config.GlobalMusic.travelAudio, system.Mod));
            }

            if (!string.IsNullOrEmpty(system.Config.GlobalMusic.endTimesAudio))
            {
                Delay.FireOnNextUpdate(() => {
                    Locator.GetGlobalMusicController().gameObject.GetAddComponent<DreamWorldEndTimes>().SetEndTimesAudio(system.Config.GlobalMusic.endTimesAudio, system.Mod);
                    AudioUtilities.SetAudioClip(Locator.GetGlobalMusicController()._endTimesSource, system.Config.GlobalMusic.endTimesAudio, system.Mod);
                });
            }

            if (!string.IsNullOrEmpty(system.Config.GlobalMusic.endTimesDreamAudio))
            {
                Delay.FireOnNextUpdate(() => Locator.GetGlobalMusicController().gameObject.GetAddComponent<DreamWorldEndTimes>().SetEndTimesDreamAudio(system.Config.GlobalMusic.endTimesDreamAudio, system.Mod));
            }

            if (!string.IsNullOrEmpty(system.Config.GlobalMusic.brambleDimensionAudio))
            {
                Delay.FireOnNextUpdate(() => AudioUtilities.SetAudioClip(Locator.GetGlobalMusicController()._darkBrambleSource, system.Config.GlobalMusic.brambleDimensionAudio, system.Mod));
            }

            if (!string.IsNullOrEmpty(system.Config.GlobalMusic.finalEndTimesIntroAudio))
            {
                Delay.FireOnNextUpdate(() => AudioUtilities.SetAudioClip(Locator.GetGlobalMusicController()._finalEndTimesIntroSource, system.Config.GlobalMusic.finalEndTimesIntroAudio, system.Mod));
            }

            if (!string.IsNullOrEmpty(system.Config.GlobalMusic.finalEndTimesLoopAudio))
            {
                Delay.FireOnNextUpdate(() => AudioUtilities.SetAudioClip(Locator.GetGlobalMusicController()._finalEndTimesLoopSource, system.Config.GlobalMusic.finalEndTimesLoopAudio, system.Mod));
            }

            if (!string.IsNullOrEmpty(system.Config.GlobalMusic.finalEndTimesBrambleDimensionAudio))
            {
                Delay.FireOnNextUpdate(() => AudioUtilities.SetAudioClip(Locator.GetGlobalMusicController()._finalEndTimesDarkBrambleSource, system.Config.GlobalMusic.finalEndTimesBrambleDimensionAudio, system.Mod));
            }
        }
    }
}
