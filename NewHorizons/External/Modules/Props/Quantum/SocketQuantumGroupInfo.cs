using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Quantum;

[JsonObject]
internal class SocketQuantumGroupInfo : BaseQuantumGroupInfo
{
    QuantumSocketInfo[] sockets;
}
