using NewHorizons.External.Modules.Props.Audio;
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
        /// If set, the player must know this ship log fact for this traveler (and their instrument zones and quantum instruments) to appear. The fact does not need to exist in the current star system; the player's save data will be checked directly.
        /// </summary>
        public string requiredFact;

        /// <summary>
        /// If set, the player must have this persistent dialogue condition set for this traveler (and their instrument zones and quantum instruments) to appear.
        /// </summary>
        public string requiredPersistentCondition;

        /// <summary>
        /// The dialogue condition that will trigger the traveler to start playing their instrument. Must be unique for each traveler.
        /// </summary>
        public string startPlayingCondition;

        /// <summary>
        /// If specified, this dialogue condition must be set for the traveler to participate in the campfire song. Otherwise, the song will be able to start without them.
        /// </summary>
        public string participatingCondition;

        /// <summary>
        /// The audio signal to use for the traveler while playing around the campfire (and also for their paired quantum instrument if another is not specified). The audio clip should be 16 measures at 92 BPM (approximately 42 seconds long).
        /// </summary>
        public SignalInfo signal;

        /// <summary>
        /// The audio to use for the traveler during the finale of the campfire song. It should be 8 measures of the main loop at 92 BPM followed by 2 measures of fade-out (approximately 26 seconds long in total). Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// </summary>
        public string finaleAudio;

        /// <summary>
        /// The dialogue to use for this traveler. If omitted, the first CharacterDialogueTree in the object will be used.
        /// </summary>
        public DialogueInfo dialogue;
    }
}
