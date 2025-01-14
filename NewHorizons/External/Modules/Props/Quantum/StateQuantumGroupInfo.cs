using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Quantum;

[JsonObject]
public class StateQuantumGroupInfo : BaseQuantumGroupInfo
{
    /// <summary>
    /// Optional. If this is true, then the states will be presented in order, rather than in a random order
    /// </summary>
    public bool sequential;

    /// <summary>
    /// Optional. Only used if `sequential` is true. If this is false, then after the last state has appeared, the object will no longer change state
    /// </summary>
    [DefaultValue(true)] public bool loop = true;

    /// <summary>
    /// Optional. Only used if type is `states`. If this is true, then the first prop made part of this group will be used to construct a visibility box for an empty game object, which will be considered one of the states.
    /// </summary>
    public bool hasEmptyState;
}
