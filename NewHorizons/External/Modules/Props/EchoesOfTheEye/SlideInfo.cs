using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class SlideInfo
    {
        /// <summary>
        /// Ambient light colour when viewing this slide.
        /// </summary>
        public MColor ambientLightColor;


        // SlideAmbientLightModule

        /// <summary>
        /// Ambient light intensity when viewing this slide.
        /// </summary>
        public float ambientLightIntensity;

        /// <summary>
        /// Ambient light range when viewing this slide.
        /// </summary>
        public float ambientLightRange;

        // SlideBackdropAudioModule

        /// <summary>
        /// The name of the AudioClip that will continuously play while watching these slides
        /// </summary>
        public string backdropAudio;

        /// <summary>
        /// The time to fade into the backdrop audio
        /// </summary>
        public float backdropFadeTime;

        // SlideBeatAudioModule

        /// <summary>
        /// The name of the AudioClip for a one-shot sound when opening the slide.
        /// </summary>
        public string beatAudio;

        /// <summary>
        /// The time delay until the one-shot audio
        /// </summary>
        public float beatDelay;


        // SlideBlackFrameModule

        /// <summary>
        /// Before viewing this slide, there will be a black frame for this many seconds.
        /// </summary>
        public float blackFrameDuration;

        /// <summary>
        /// The path to the image file for this slide.
        /// </summary>
        public string imagePath;


        // SlidePlayTimeModule

        /// <summary>
        /// Play-time duration for auto-projector slides.
        /// </summary>
        public float playTimeDuration;


        // SlideShipLogEntryModule

        /// <summary>
        /// Ship log fact revealed when viewing this slide
        /// </summary>
        public string reveal;

        /// <summary>
        /// Spotlight intensity modifier when viewing this slide.
        /// </summary>
        public float spotIntensityMod;
    }

}
