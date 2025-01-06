using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class DreamModule
    {
        /// <summary>
        /// Setting this value will make this body a dream world style dimension where its contents are only activated while entering it from a dream campfire. Disables the body's map marker.
        /// </summary>
        public bool inDreamWorld;
        /// <summary>
        /// Whether to generate simulation meshes (the models used in the "tronworld" or "matrix" view) for most objects on this planet by cloning the existing meshes and applying the simulation materials. Leave this off if you are building your own simulation meshes or using existing objects which have them.
        /// </summary>
        public bool generateSimulationMeshes;
    }
}
