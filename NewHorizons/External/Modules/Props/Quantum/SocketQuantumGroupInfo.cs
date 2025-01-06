using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Quantum;

[JsonObject]
public class SocketQuantumGroupInfo : BaseQuantumGroupInfo
{
    public QuantumSocketInfo[] sockets;
}
