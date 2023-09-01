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
}
