using NewHorizons.Components;
using NewHorizons.External.Modules.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class ShapeBuilder
    {
        public static OWTriggerVolume AddTriggerVolume(GameObject go, ShapeInfo info, float defaultRadius)
        {
            var owTriggerVolume = go.AddComponent<OWTriggerVolume>();

            if (info != null)
            {
                var shapeOrCol = AddShapeOrCollider(go, info);
                if (shapeOrCol is Shape shape)
                    owTriggerVolume._shape = shape;
                else if (shapeOrCol is Collider col)
                    owTriggerVolume._owCollider = col.GetComponent<OWCollider>();
            }
            else
            {
                var col = go.AddComponent<SphereCollider>();
                col.radius = defaultRadius;
                col.isTrigger = true;
                var owCollider = go.GetAddComponent<OWCollider>();

                owTriggerVolume._owCollider = owCollider;
            }

            return owTriggerVolume;
        }

        public static Component AddShapeOrCollider(GameObject go, ShapeInfo info)
        {
            if (info.useShape.HasValue)
            {
                // Explicitly add either a shape or collider if specified
                if (info.useShape.Value)
                {
                    return AddShape(go, info);
                }
                else
                {
                    return AddCollider(go, info);
                }
            }
            else
            {
                // Prefer shapes over colliders if no preference is specified and not using collision
                // This is required for backwards compat (previously it defaulted to shapes)
                // A common-ish puzzle is to put an insulating volume on a held item. Held items disabled all colliders when held, but don't disable shapes.
                // Changing the default from shapes to colliders broke these puzzles
                if (info.hasCollision)
                {
                    return AddCollider(go, info);
                }
                else
                {
                    return AddShape(go, info);
                }
            }
        }

        public static Shape AddShape(GameObject go, ShapeInfo info)
        {
            if (info.hasCollision)
            {
                throw new NotSupportedException($"Shapes do not support collision; set {nameof(info.hasCollision)} to false or use a supported collider type (sphere, box, or capsule).");
            }
            if (info.useShape.HasValue && !info.useShape.Value)
            {
                throw new NotSupportedException($"{info.useShape} was explicitly set to false but a shape is required here.");
            }
            switch (info.type)
            {
                case ShapeType.Sphere:
                    var sphereShape = go.AddComponent<SphereShape>();
                    sphereShape._radius = info.radius;
                    sphereShape._center = info.offset ?? Vector3.zero;
                    return sphereShape;
                case ShapeType.Box:
                    var boxShape = go.AddComponent<BoxShape>();
                    boxShape._size = info.size ?? Vector3.one;
                    boxShape._center = info.offset ?? Vector3.zero;
                    return boxShape;
                case ShapeType.Capsule:
                    var capsuleShape = go.AddComponent<CapsuleShape>();
                    capsuleShape._radius = info.radius;
                    capsuleShape._direction = (int)info.direction;
                    capsuleShape._height = info.height;
                    capsuleShape._center = info.offset ?? Vector3.zero;
                    return capsuleShape;
                case ShapeType.Cylinder:
                    var cylinderShape = go.AddComponent<CylinderShape>();
                    cylinderShape._radius = info.radius;
                    cylinderShape._height = info.height;
                    cylinderShape._center = info.offset ?? Vector3.zero;
                    cylinderShape._pointChecksOnly = true;
                    return cylinderShape;
                case ShapeType.Cone:
                    var coneShape = go.AddComponent<ConeShape>();
                    coneShape._topRadius = info.innerRadius;
                    coneShape._bottomRadius = info.outerRadius;
                    coneShape._direction = (int)info.direction;
                    coneShape._height = info.height;
                    coneShape._center = info.offset ?? Vector3.zero;
                    coneShape._pointChecksOnly = true;
                    return coneShape;
                case ShapeType.Hemisphere:
                    var hemisphereShape = go.AddComponent<HemisphereShape>();
                    hemisphereShape._radius = info.radius;
                    hemisphereShape._direction = (int)info.direction;
                    hemisphereShape._cap = info.cap;
                    hemisphereShape._center = info.offset ?? Vector3.zero;
                    hemisphereShape._pointChecksOnly = true;
                    return hemisphereShape;
                case ShapeType.Hemicapsule:
                    var hemicapsuleShape = go.AddComponent<HemicapsuleShape>();
                    hemicapsuleShape._radius = info.radius;
                    hemicapsuleShape._direction = (int)info.direction;
                    hemicapsuleShape._height = info.height;
                    hemicapsuleShape._cap = info.cap;
                    hemicapsuleShape._center = info.offset ?? Vector3.zero;
                    hemicapsuleShape._pointChecksOnly = true;
                    return hemicapsuleShape;
                case ShapeType.Ring:
                    var ringShape = go.AddComponent<RingShape>();
                    ringShape.innerRadius = info.innerRadius;
                    ringShape.outerRadius = info.outerRadius;
                    ringShape.height = info.height;
                    ringShape.center = info.offset ?? Vector3.zero;
                    ringShape._pointChecksOnly = true;
                    return ringShape;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info.type), info.type, $"Unsupported shape type");
            }
        }

        public static Collider AddCollider(GameObject go, ShapeInfo info)
        {
            if (info.useShape.HasValue && info.useShape.Value)
            {
                throw new NotSupportedException($"{info.useShape} was explicitly set to true but a non-shape collider is required here.");
            }
            switch (info.type)
            {
                case ShapeType.Sphere:
                    var sphereCollider = go.AddComponent<SphereCollider>();
                    sphereCollider.radius = info.radius;
                    sphereCollider.center = info.offset ?? Vector3.zero;
                    sphereCollider.isTrigger = !info.hasCollision;
                    go.GetAddComponent<OWCollider>();
                    return sphereCollider;
                case ShapeType.Box:
                    var boxCollider = go.AddComponent<BoxCollider>();
                    boxCollider.size = info.size ?? Vector3.one;
                    boxCollider.center = info.offset ?? Vector3.zero;
                    boxCollider.isTrigger = !info.hasCollision;
                    go.GetAddComponent<OWCollider>();
                    return boxCollider;
                case ShapeType.Capsule:
                    var capsuleCollider = go.AddComponent<CapsuleCollider>();
                    capsuleCollider.radius = info.radius;
                    capsuleCollider.direction = (int)info.direction;
                    capsuleCollider.height = info.height;
                    capsuleCollider.center = info.offset ?? Vector3.zero;
                    capsuleCollider.isTrigger = !info.hasCollision;
                    go.GetAddComponent<OWCollider>();
                    return capsuleCollider;
                default:
                    throw new ArgumentOutOfRangeException(nameof(info.type), info.type, $"Unsupported collider type");
            }
        }
    }
}
