using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Quantum;

public class QuantumDetailInfo : DetailInfo
{
    public QuantumDetailInfo() { }

    public QuantumDetailInfo(DetailInfo info)
    {
        JsonConvert.PopulateObject(JsonConvert.SerializeObject(info), this);
    }

    /// <summary>
    /// When used in a quantum socket this object will randomize its rotation around the local Y axis.
    /// </summary>
    [DefaultValue(true)] public bool randomizeYRotation = true;

    /// <summary>
    /// When used in a quantum socket this object will align to the nearest gravity volume. Else use the rotation of the quantum socket.
    /// </summary>
    [DefaultValue(true)] public bool alignWithGravity = true;
}
