using System.Collections.Generic;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Components
{
    public class RingShape : Shape
    {
        private Vector3 _center;
        private float _innerRadius;
        private float _outerRadius;
        private float _height;

        private CylinderShape _innerCylinderShape;
        private CylinderShape _outerCylinderShape;

        public Vector3 center
        {
            get { return _center; }
            set
            {
                _center = value;
                RecalculateLocalBounds();
            }
        }

        public float innerRadius
        {
            get { return _innerRadius; }
            set
            {
                _innerRadius = Mathf.Max(value, 0f);
                RecalculateLocalBounds();

                if (_innerCylinderShape) _innerCylinderShape.radius = _innerRadius;
            }
        }

        public float outerRadius
        {
            get { return _outerRadius; }
            set
            {
                _outerRadius = Mathf.Max(value, 0f);
                RecalculateLocalBounds();

                if (_outerCylinderShape) _outerCylinderShape.radius = _outerRadius;
            }
        }

        public float height
        {
            get { return _height; }
            set
            {
                _height = Mathf.Max(value, 0f);
                RecalculateLocalBounds();

                if (_innerCylinderShape) _innerCylinderShape.height = height + 30;
                if (_outerCylinderShape) _outerCylinderShape.height = height;
            }
        }

        public override int layerMask
        {
            get { return base.layerMask; }
            set
            {
                base.layerMask = value;
                if (_innerCylinderShape) _innerCylinderShape.layerMask = value;
                if (_outerCylinderShape) _outerCylinderShape.layerMask = value;
            }
        }

        public override bool pointChecksOnly
        {
            get { return base.pointChecksOnly; }
            set
            {
                base.pointChecksOnly = value;
                if (_innerCylinderShape) _innerCylinderShape.pointChecksOnly = value;
                if (_outerCylinderShape) _outerCylinderShape.pointChecksOnly = value;
            }
        }

        public override void Awake()
        {
            base.Awake();

            var innerCylinderGO = new GameObject("InnerCylinder");
            innerCylinderGO.layer = gameObject.layer;
            innerCylinderGO.transform.parent = transform;
            innerCylinderGO.transform.localPosition = Vector3.zero;
            innerCylinderGO.transform.localRotation = Quaternion.identity;
            _innerCylinderShape = innerCylinderGO.AddComponent<CylinderShape>();
            innerCylinderGO.AddComponent<OWTriggerVolume>();
            _innerCylinderShape.OnCollisionEnter += OnInnerCollisionEnter;
            _innerCylinderShape.OnCollisionExit += OnInnerCollisionExit;

            _innerCylinderShape.center = center;
            _innerCylinderShape.height = height + 30;
            _innerCylinderShape.radius = innerRadius;
            _innerCylinderShape.layerMask = layerMask;
            _innerCylinderShape.pointChecksOnly = pointChecksOnly;

            var outerCylinderGO = new GameObject("OuterCylinder");
            outerCylinderGO.layer = gameObject.layer;
            outerCylinderGO.transform.parent = transform;
            outerCylinderGO.transform.localPosition = Vector3.zero;
            outerCylinderGO.transform.localRotation = Quaternion.identity;
            _outerCylinderShape = outerCylinderGO.AddComponent<CylinderShape>();
            outerCylinderGO.AddComponent<OWTriggerVolume>();
            _outerCylinderShape.OnCollisionEnter += OnOuterCollisionEnter;
            _outerCylinderShape.OnCollisionExit += OnOuterCollisionExit;

            _outerCylinderShape.center = center;
            _outerCylinderShape.height = height;
            _outerCylinderShape.radius = outerRadius;
            _outerCylinderShape.layerMask = layerMask;
            _outerCylinderShape.pointChecksOnly = pointChecksOnly;
        }

        public void OnDestroy()
        {
            if (_innerCylinderShape)
            {
                _innerCylinderShape.OnCollisionEnter -= OnInnerCollisionEnter;
                _innerCylinderShape.OnCollisionExit -= OnInnerCollisionExit;
            }

            if (_outerCylinderShape)
            {
                _outerCylinderShape.OnCollisionEnter -= OnOuterCollisionEnter;
                _outerCylinderShape.OnCollisionExit -= OnOuterCollisionExit;
            }
        }

        public void Start()
        {
            RecalculateLocalBounds();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _innerCylinderShape?.OnEnable();
            _outerCylinderShape?.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _innerCylinderShape?.OnDisable();
            _outerCylinderShape?.OnDisable();
        }

        public override void SetCollisionMode(CollisionMode newCollisionMode)
        {
            base.SetCollisionMode(newCollisionMode);
            _innerCylinderShape?.SetCollisionMode(newCollisionMode);
            _outerCylinderShape?.SetCollisionMode(newCollisionMode);
        }

        public override void SetLayer(Layer newLayer)
        {
            base.SetLayer(newLayer);
            _innerCylinderShape?.SetLayer(newLayer);
            _outerCylinderShape?.SetLayer(newLayer);
        }

        public override void SetActivation(bool newActive)
        {
            base.SetActivation(newActive);
            _innerCylinderShape?.SetActivation(newActive);
            _outerCylinderShape?.SetActivation(newActive);
        }

        public override Vector3 GetWorldSpaceCenter()
        {
            return transform.TransformPoint(_center);
        }

        public override void RecalculateLocalBounds()
        {
            localBounds.Set(_center, outerRadius);
        }

        public override bool PointInside(Vector3 point)
        {
            return (!_innerCylinderShape.PointInside(point) && _outerCylinderShape.PointInside(point));
        }

        private List<Shape> _shapesInInner = new List<Shape>();
        private List<Shape> _shapesInOuter = new List<Shape>();
        private List<Shape> _shapesInside = new List<Shape>();

        private void UpdateCollisionStatus(Shape shape)
        {
            var inside = _shapesInside.Contains(shape);
            var insideInner = _shapesInInner.Contains(shape);
            var insideOuter = _shapesInOuter.Contains(shape);

            if (inside)
            {
                // If we've moved into the inner cylinder then we're not in the ring
                // If we've excited the outer radius we're not in the ring either
                if (insideInner || !insideOuter)
                {
                    //Logger.Log($"Shape [{shape}] exited ring");
                    FireCollisionExitEvent(shape);
                    _shapesInside.Remove(shape);
                }
            }
            else
            {
                // If we've moved into the outer cylinder but not the inner one, we're now in the ring
                if (insideOuter && !insideInner)
                {
                    //Logger.Log($"Shape [{shape}] entered ring");
                    FireCollisionEnterEvent(shape);
                    _shapesInside.Add(shape);
                }
            }
        }

        public void OnInnerCollisionEnter(Shape shape)
        {
            //Logger.Log($"Shape [{shape}] entered inner radius");

            _shapesInInner.Add(shape);

            UpdateCollisionStatus(shape);
        }

        public void OnInnerCollisionExit(Shape shape)
        {
            //Logger.Log($"Shape [{shape}] exited inner radius");

            _shapesInInner.Remove(shape);

            UpdateCollisionStatus(shape);
        }

        public void OnOuterCollisionEnter(Shape shape)
        {
            //Logger.Log($"Shape [{shape}] entered outer radius");

            _shapesInOuter.Add(shape);

            UpdateCollisionStatus(shape);
        }

        public void OnOuterCollisionExit(Shape shape)
        {
            //Logger.Log($"Shape [{shape}] exited outer radius");

            _shapesInOuter.Remove(shape);

            UpdateCollisionStatus(shape);
        }
    }
}
