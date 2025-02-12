using NewHorizons.External.Modules.Props.Audio;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.EyeOfTheUniverse
{
    [JsonObject]
    public class QuantumInstrumentInfo : DetailInfo
    {
        /// <summary>
        /// The unique ID of the Eye Traveler associated with this quantum instrument.
        /// </summary>
        public string id;

        /// <summary>
        /// A dialogue condition to set when gathering this quantum instrument. Use it in conjunction with `activationCondition` or `deactivationCondition` on other details.
        /// </summary>
        public string gatherCondition;

        /// <summary>
        /// Allows gathering this quantum instrument using the zoomed-in signalscope, like Chert's bongos.
        /// </summary>
        public bool gatherWithScope;

        /// <summary>
        /// The audio signal emitted by this quantum instrument. The fields `name`, `audio`, and `frequency` will be copied from the corresponding Eye Traveler's signal if not specified here.
        /// </summary>
        public SignalInfo signal;

        /// <summary>
        /// The radius of the added sphere collider that will be used for interaction.
        /// </summary>
        [DefaultValue(0.5f)] public float interactRadius = 0.5f;

        /// <summary>
        /// The furthest distance where the player can interact with this quantum instrument.
        /// </summary>
        [DefaultValue(2f)] public float interactRange = 2f;
    }
}
