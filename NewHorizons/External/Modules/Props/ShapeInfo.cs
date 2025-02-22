using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class ShapeInfo
    {
        /// <summary>
        /// The type of shape or collider to add. Sphere, box, and capsule colliders are more performant and support collision. Defaults to sphere.
        /// </summary>
        public ShapeType type = ShapeType.Sphere;

        /// <summary>
        /// The radius of the shape or collider. Defaults to 0.5 meters. Only used by spheres, capsules, cylinders, hemispheres, hemicapsules, and rings.
        /// </summary>
        public float radius = 0.5f;

        /// <summary>
        /// The height of the shape or collider. Defaults to 1 meter. Only used by capsules, cylinders, cones, hemicapsules, and rings.
        /// </summary>
        public float height = 1f;

        /// <summary>
        /// The axis that the shape or collider is aligned with. Defaults to the Y axis (up). The flat bottom of the shape will be pointing towards the negative axis. Only used by capsules, cones, hemispheres, and hemicapsules.
        /// </summary>
        public ColliderAxis direction = ColliderAxis.Y;

        /// <summary>
        /// The inner radius of the shape. Defaults to 0 meters. Only used by cones and rings.
        /// </summary>
        public float innerRadius = 0f;

        /// <summary>
        /// The outer radius of the shape. Defaults to 0.5 meters. Only used by cones and rings.
        /// </summary>
        public float outerRadius = 0.5f;

        /// <summary>
        /// Whether the shape has an end cap. Defaults to true. Only used by hemispheres and hemicapsules.
        /// </summary>
        public bool cap = true;

        /// <summary>
        /// The size of the shape or collider. Defaults to (1,1,1). Only used by boxes.
        /// </summary>
        public MVector3 size;

        /// <summary>
        /// The offset of the shape or collider from the object's origin. Defaults to (0,0,0). Supported by all collider and shape types.
        /// </summary>
        public MVector3 offset;

        /// <summary>
        /// Whether the collider should have collision enabled. If false, the collider will be a trigger. Defaults to false. Only supported for spheres, boxes, and capsules.
        /// </summary>
        public bool hasCollision = false;

        /// <summary>
        /// Whether to explicitly use a shape instead of a collider. Shapes do not support collision and are less performant, but support a wider set of shapes and are required by some components. Omit this unless you explicitly want to use a sphere, box, or capsule shape instead of a collider.
        /// </summary>
        public bool? useShape;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ShapeType
    {
        [EnumMember(Value = @"sphere")] Sphere,
        [EnumMember(Value = @"box")] Box,
        [EnumMember(Value = @"capsule")] Capsule,
        [EnumMember(Value = @"cylinder")] Cylinder,
        [EnumMember(Value = @"cone")] Cone,
        [EnumMember(Value = @"hemisphere")] Hemisphere,
        [EnumMember(Value = @"hemicapsule")] Hemicapsule,
        [EnumMember(Value = @"ring")] Ring,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ColliderAxis
    {
        [EnumMember(Value = @"x")] X = 0,
        [EnumMember(Value = @"y")] Y = 1,
        [EnumMember(Value = @"z")] Z = 2,
    }
}
