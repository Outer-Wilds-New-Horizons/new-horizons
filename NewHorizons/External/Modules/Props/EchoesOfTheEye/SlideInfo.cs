using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

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
        /// Ambient light intensity when viewing this slide.
        /// Set this to add ambient light module. Base game default is 1.
        /// </summary>
        public float ambientLightIntensity;

        /// <summary>
        /// Ambient light range when viewing this slide.
        /// </summary>
        [DefaultValue(20f)] public float ambientLightRange = 20f;

        /// <summary>
        /// Ambient light colour when viewing this slide. Defaults to white.
        /// </summary>
        public MColor ambientLightColor;

        /// <summary>
        /// Spotlight intensity modifier when viewing this slide.
        /// </summary>
        [DefaultValue(0f)] public float spotIntensityMod = 0f;

        // SlideBackdropAudioModule

        /// <summary>
        /// The name of the AudioClip that will continuously loop while watching these slides.
        /// Set this to include backdrop audio module. Base game default is Reel_1_Backdrop_A.
        /// </summary>
        public string backdropAudio;

        /// <summary>
        /// The time to fade into the backdrop audio.
        /// </summary>
        [DefaultValue(2f)] public float backdropFadeTime = 2f;

        // SlideBeatAudioModule

        /// <summary>
        /// The name of the AudioClip for a one-shot sound when opening the slide.
        /// Set this to include beat audio module. Base game default is Reel_1_Beat_A.
        /// </summary>
        public string beatAudio;

        /// <summary>
        /// The time delay until the one-shot audio.
        /// </summary>
        [DefaultValue(0f)] public float beatDelay = 0f;

        // SlideBlackFrameModule

        /// <summary>
        /// Before viewing this slide, there will be a black frame for this many seconds.
        /// Set this to include black frame module. Base game default is 0.
        /// </summary>
        public float blackFrameDuration;

        // SlidePlayTimeModule

        /// <summary>
        /// Play-time duration for auto-projector slides.
        /// Set this to include play time module. Base game default is 0.
        /// </summary>
        public float playTimeDuration;

        // SlideShipLogEntryModule

        /// <summary>
        /// Ship log fact revealed when viewing this slide.
        /// Set this to include ship log entry module. Base game default is "".
        /// </summary>
        public string reveal;

        // SlideRotationModule

        /// <summary>
        /// Exclusive to slide reels. Whether this slide should rotate the reel item while inside a projector.
        /// </summary>
        public bool rotate = true;
    }
}