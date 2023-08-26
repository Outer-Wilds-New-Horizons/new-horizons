using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Quantum;

[JsonObject]
public class SocketQuantumGroupInfo : BaseQuantumGroupInfo
{
    QuantumSocketInfo[] sockets;
}
