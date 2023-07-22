using System.ComponentModel;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class ReferenceFrameModule
    {
        /// <summary>
        /// Allows the object to be targeted.
        /// </summary>
        [DefaultValue(true)] public bool enabled = true;

        /// <summary>
        /// Stop the object from being targeted on the map.
        /// </summary>
        public bool hideInMap;

        /// <summary>
        /// Radius of the brackets that show up when you target this. Defaults to the sphere of influence.
        /// </summary>
        [DefaultValue(-1f)] public float bracketRadius = -1f;

        /// <summary>
        /// If it should be targetable even when super close.
        /// </summary>
        public bool targetWhenClose;

        /// <summary>
        /// The maximum distance that the reference frame can be targeted from. Defaults to 100km and cannot be greater than that.
        /// </summary>
        public float maxTargetDistance; // If it's less than or equal to zero the game makes it 100km 

        /// <summary>
        /// The radius of the sphere around the planet which you can click on to target it. Defaults to twice the sphere of influence.
        /// </summary>
        public float targetColliderRadius;

        /// <summary>
        /// Position of the reference frame relative to the object.
        /// </summary>
        public MVector3 localPosition;
    }
}