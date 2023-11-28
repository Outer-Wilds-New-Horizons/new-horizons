using NewHorizons.External.Modules.Volumes.VolumeInfos;
using NewHorizons.External.SerializableData;
using NewHorizons.External.SerializableEnums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NewHorizons.External.Modules.Props
{

    [JsonObject]
    public class TornadoInfo : GeneralPropInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TornadoType
        {
            [EnumMember(Value = @"upwards")] Upwards = 0,

            [EnumMember(Value = @"downwards")] Downwards = 1,

            [EnumMember(Value = @"hurricane")] Hurricane = 2
        }

        [Obsolete("Downwards is deprecated. Use Type instead.")] public bool downwards;

        /// <summary>
        /// Alternative to setting the position. Will choose a random place at this elevation.
        /// </summary>
        public float elevation;

        /// <summary>
        /// The height of this tornado.
        /// </summary>
        [DefaultValue(30f)] public float height = 30f;

        /// <summary>
        /// The colour of the tornado.
        /// </summary>
        public MColor tint;

        /// <summary>
        /// What type of cyclone should this be? Upwards and downwards are both tornados and will push in that direction.
        /// </summary>
        [DefaultValue("upwards")] public TornadoType type = TornadoType.Upwards;

        /// <summary>
        /// Angular distance from the starting position that it will wander, in terms of the angle around the x-axis.
        /// </summary>
        [DefaultValue(45f)] public float wanderDegreesX = 45f;

        /// <summary>
        /// Angular distance from the starting position that it will wander, in terms of the angle around the z-axis.
        /// </summary>
        [DefaultValue(45f)] public float wanderDegreesZ = 45f;

        /// <summary>
        /// The rate at which the tornado will wander around the planet. Set to 0 for it to be stationary. Should be around
        /// 0.1.
        /// </summary>
        public float wanderRate;

        /// <summary>
        /// The maximum distance at which you'll hear the sounds of the cyclone. If not set it will scale relative to the size of the cyclone.
        /// </summary>
        public float audioDistance;

        /// <summary>
        /// Fluid type for sounds/effects when colliding with this tornado.
        /// </summary>
        [DefaultValue("cloud")] public NHFluidType fluidType = NHFluidType.CLOUD;

        /// <summary>
        /// The type of hazard for this volume. Leave empty for this tornado to not be hazardous.
        /// </summary>
        public HazardVolumeInfo.HazardType? hazardType;

        /// <summary>
        /// The amount of damage you will take per second while inside this tornado. Only used it hazardType is set.
        /// </summary>
        [DefaultValue(10f)] public float damagePerSecond = 10f;

        /// <summary>
        /// The type of damage you will take when you first touch this volume. Leave empty for this tornado to not cause damage on first contact.
        /// </summary>
        public HazardVolumeInfo.InstantDamageType? firstContactDamageType;

        /// <summary>
        /// The amount of damage you will take when you first touch this volume. Only relevant if firstContactDamageType is set.
        /// </summary>
        [DefaultValue(10f)] public float firstContactDamage = 10f;
    }

}
