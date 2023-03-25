using Newtonsoft.Json;

namespace NewHorizons.Utility
{
    [JsonObject]
    public class MGradient
    {
        public MGradient(float time, MColor tint)
        {
            this.time = time; 
            this.tint = tint;
        }

        public float time;
        public MColor tint;
    }
}
