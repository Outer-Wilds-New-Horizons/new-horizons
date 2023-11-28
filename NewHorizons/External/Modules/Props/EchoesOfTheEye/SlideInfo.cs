using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class SlideInfo
    {
        /// <summary>
        /// The path to the image file for this slide.
        /// </summary>
        public string imagePath;

        // SlideAmbientLightModule

        /// <summary>
        /// Ambient light intensity when viewing this slide. (Base game default: 1)
        /// </summary>
        public float ambientLightIntensity;

        /// <summary>
        /// Ambient light range when viewing this slide. (Base game default: 20)
        /// </summary>
        public float ambientLightRange;

        /// <summary>
        /// Ambient light colour when viewing this slide. (Base game default: white)
        /// </summary>
        public MColor ambientLightColor;

        /// <summary>
        /// Spotlight intensity modifier when viewing this slide. (Base game default: 0)
        /// </summary>
        public float spotIntensityMod;

        // SlideBackdropAudioModule

        /// <summary>
        /// The name of the AudioClip that will continuously play while watching these slides (Base game default: Reel_1_Backdrop_A)
        /// </summary>
        public string backdropAudio;

        /// <summary>
        /// The time to fade into the backdrop audio (Base game default: 2)
        /// </summary>
        public float backdropFadeTime;

        // SlideBeatAudioModule

        /// <summary>
        /// The name of the AudioClip for a one-shot sound when opening the slide. (Base game default: Reel_1_Beat_A)
        /// </summary>
        public string beatAudio;

        /// <summary>
        /// The time delay until the one-shot audio (Base game default: 0)
        /// </summary>
        public float beatDelay;

        // SlideBlackFrameModule

        /// <summary>
        /// Before viewing this slide, there will be a black frame for this many seconds. (Base game default: 0)
        /// </summary>
        public float blackFrameDuration;

        // SlidePlayTimeModule

        /// <summary>
        /// Play-time duration for auto-projector slides. (Base game default: 0)
        /// </summary>
        public float playTimeDuration;

        // SlideShipLogEntryModule

        /// <summary>
        /// Ship log fact revealed when viewing this slide (Base game default: "")
        /// </summary>
        public string reveal;
    }
}