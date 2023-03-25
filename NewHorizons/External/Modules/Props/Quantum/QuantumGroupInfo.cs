using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Quantum
{
    [JsonObject]
    public class QuantumGroupInfo
    {
        /// <summary>
        /// What type of group this is: does it define a list of states a single quantum object could take or a list of sockets one or more quantum objects could share?
        /// </summary>
        public QuantumGroupType type;

        /// <summary>
        /// A unique string used by props (that are marked as quantum) use to refer back to this group
        /// </summary>
        public string id;

        /// <summary>
        /// Only required if type is `sockets`. This lists all the possible locations for any props assigned to this group.
        /// </summary>
        public QuantumSocketInfo[] sockets;

        /// <summary>
        /// Optional. Only used if type is `states`. If this is true, then the first prop made part of this group will be used to construct a visibility box for an empty game object, which will be considered one of the states.
        /// </summary>
        public bool hasEmptyState;

        /// <summary>
        /// Optional. Only used if type is `states`. If this is true, then the states will be presented in order, rather than in a random order
        /// </summary>
        public bool sequential;

        /// <summary>
        /// Optional. Only used if type is `states` and `sequential` is true. If this is false, then after the last state has appeared, the object will no longer change state
        /// </summary>
        [DefaultValue(true)] public bool loop = true;
    }
}
