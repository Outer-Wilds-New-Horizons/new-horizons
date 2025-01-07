using Newtonsoft.Json;

namespace NewHorizons.External.Modules.Props.Quantum;

[JsonObject]
public class LightningQuantumInfo : GeneralPropInfo
{
    /// <summary>
    /// List of props which will be alternated between during flashes of lightning
    /// </summary>
    public DetailInfo[] details;
}
