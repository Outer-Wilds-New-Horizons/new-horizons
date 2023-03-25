using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class VolcanoInfo : GeneralPropInfo
    {
        /// <summary>
        /// The colour of the meteor's lava.
        /// </summary>
        public MColor lavaTint;

        /// <summary>
        /// Maximum time between meteor launches.
        /// </summary>
        [DefaultValue(20f)]
        public float maxInterval = 20f;

        /// <summary>
        /// Maximum random speed at which meteors are launched.
        /// </summary>
        [DefaultValue(150f)]
        public float maxLaunchSpeed = 150f;

        /// <summary>
        /// Minimum time between meteor launches.
        /// </summary>
        [DefaultValue(5f)]
        public float minInterval = 5f;

        /// <summary>
        /// Minimum random speed at which meteors are launched.
        /// </summary>
        [DefaultValue(50f)]
        public float minLaunchSpeed = 50f;

        /// <summary>
        /// Scale of the meteors.
        /// </summary>
        public float scale = 1;

        /// <summary>
        /// The colour of the meteor's stone.
        /// </summary>
        public MColor stoneTint;
    }
}
