using Newtonsoft.Json;

namespace NewHorizons.Utility.Files;

[JsonObject]
public class TextureCacheData
{
    [JsonProperty] public bool useMipmaps;
    [JsonProperty] public bool linear;
    [JsonProperty] public bool wrap;
    [JsonProperty] public string relativePath;
}
