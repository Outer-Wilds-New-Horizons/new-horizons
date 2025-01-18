using NewHorizons.External.Modules.Props.Dialogue;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.EyeOfTheUniverse
{
    [JsonObject]
    public class EyeTravelerInfo : DetailInfo
    {
        /// <summary>
        /// A unique ID to associate this traveler with their corresponding quantum instruments and instrument zones. Must be unique for each traveler.
        /// </summary>
        public string id;

        /// <summary>
        /// The name to display for this traveler's signals. Defaults to the name of the detail.
        /// </summary>
        public string name;

        /// <summary>
        /// The dialogue condition that will trigger the traveler to start playing their instrument. Must be unique for each traveler.
        /// </summary>
        public string startPlayingCondition;

        /// <summary>
        /// If specified, this dialogue condition must be set for the traveler to participate in the campfire song. Otherwise, the song will be able to start without them.
        /// </summary>
        public string participatingCondition;

        /// <summary>
        /// The audio to use for the traveler while playing around the campfire (and also for their paired quantum instrument). It should be 16 measures at 92 BPM (approximately 42 seconds long). Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string loopAudio;

        /// <summary>
        /// The audio to use for the traveler during the finale of the campfire song. It should be 8 measures of the main loop at 92 BPM followed by 2 measures of fade-out (approximately 26 seconds long in total). Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string finaleAudio;

        /// <summary>
        /// The frequency ID of the signal emitted by the traveler. The built-in game values are `Default`, `Traveler`, `Quantum`, `EscapePod`,
        /// `Statue`, `WarpCore`, `HideAndSeek`, and `Radio`. Defaults to `Traveler`. You can also put a custom value.
        /// </summary>
        public string frequency;

        /// <summary>
        /// The dialogue to use for this traveler. Omit this or set it to null if your traveler already has valid dialogue.
        /// </summary>
        public DialogueInfo dialogue;
    }
}
