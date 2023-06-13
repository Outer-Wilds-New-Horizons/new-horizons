using NewHorizons.External.Modules.VariableSize;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace NewHorizons.External.Modules
{
    [JsonObject]
    public class RingModule
    {
        /// <summary>
        /// Fluid type for sounds/effects when colliding with this ring.
        /// </summary>
        public NHFluidType fluidType = NHFluidType.NONE;

        /// <summary>
        /// Angle between the rings and the equatorial plane of the planet.
        /// </summary>
        public float inclination;

        /// <summary>
        /// Inner radius of the disk
        /// </summary>
        [Range(0, double.MaxValue)] public float innerRadius;

        /// <summary>
        /// Angle defining the point where the rings rise up from the planet's equatorial plane if inclination is nonzero.
        /// </summary>
        public float longitudeOfAscendingNode;

        /// <summary>
        /// Outer radius of the disk
        /// </summary>
        [Range(0, double.MaxValue)] public float outerRadius;

        /// <summary>
        /// Allows the rings to rotate.
        /// </summary>
        public float rotationSpeed;

        /// <summary>
        /// Relative filepath to the texture used for the rings.
        /// </summary>
        public string texture;

        /// <summary>
        /// Should this ring be unlit?
        /// </summary>
        public bool unlit;

        #region Obsolete
        [Obsolete("curve is deprecated, please use scaleCurve instead")]
        public TimeValuePair[] curve;
        #endregion

        /// <summary>
        /// Scale rings over time. Optional. Value between 0-1, time is in minutes.
        /// </summary>
        public TimeValuePair[] scaleCurve;

        /// <summary>
        /// Fade rings in/out over time. Optional. Value between 0-1, time is in minutes.
        /// </summary>
        public TimeValuePair[] opacityCurve;

        /// <summary>
        /// An optional rename of this object
        /// </summary>
        public string rename;
    }
}