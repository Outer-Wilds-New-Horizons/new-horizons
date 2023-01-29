using NewHorizons.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public class NomaiTextArcArranger : MonoBehaviour {
        public List<SpiralManipulator> spirals = new List<SpiralManipulator>();
        public List<SpiralManipulator> reverseToposortedSpirals = null;
        public SpiralManipulator root { get; private set; }
        private Dictionary<int, int> sprialOverlapResolutionPriority = new Dictionary<int, int>();

        private static int MAX_MOVE_DISTANCE = 2;

        public float maxX = 2.7f;
        public float minX = -2.7f;
        public float maxY = 2.6f;
        public float minY = -1f;

        public void DrawBoundsWithDebugSpheres() 
        {
            AddDebugShape.AddSphere(this.gameObject, 0.1f, Color.green).transform.localPosition = new Vector3(minX, minY, 0);
            AddDebugShape.AddSphere(this.gameObject, 0.1f, Color.green).transform.localPosition = new Vector3(minX, maxY, 0);
            AddDebugShape.AddSphere(this.gameObject, 0.1f, Color.green).transform.localPosition = new Vector3(maxX, maxY, 0);
            AddDebugShape.AddSphere(this.gameObject, 0.1f, Color.green).transform.localPosition = new Vector3(maxX, minY, 0);
            AddDebugShape.AddSphere(this.gameObject, 0.1f, Color.red).transform.localPosition = new Vector3(0, 0, 0);
        }
        
        public void GenerateReverseToposort()
        {
            reverseToposortedSpirals = new List<SpiralManipulator>();
            Queue<SpiralManipulator> frontierQueue = new Queue<SpiralManipulator>();
            frontierQueue.Enqueue(root);
        
            while(frontierQueue.Count > 0)
            {
                var spiral = frontierQueue.Dequeue();
                reverseToposortedSpirals.Add(spiral);
        
                foreach(var child in spiral.children) frontierQueue.Enqueue(child);
            }
        
            reverseToposortedSpirals.Reverse();
        }

        public static SpiralManipulator CreateSpiral(NomaiTextArcBuilder.SpiralProfile profile, GameObject spiralMeshHolder) 
        {
            var rootArc = NomaiTextArcBuilder.BuildSpiralGameObject(profile);
            rootArc.transform.parent = spiralMeshHolder.transform;
            rootArc.transform.localEulerAngles = new Vector3(0, 0, Random.Range(-60, 60));

            var manip = rootArc.AddComponent<SpiralManipulator>();
            if (Random.value < 0.5) manip.transform.localScale = new Vector3(-1, 1, 1); // randomly mirror
            
            // add to arranger
            var arranger = spiralMeshHolder.GetComponent<NomaiTextArcArranger>();
            if (arranger.root == null) arranger.root = manip;
            arranger.spirals.Add(manip);

            return manip;
        }
        
        // returns whether there was overlap or not
        public bool AttemptOverlapResolution() 
        {
            var overlappingSpirals = FindOverlap();
            if (overlappingSpirals.x < 0) return false;

            if (!sprialOverlapResolutionPriority.ContainsKey(overlappingSpirals.x)) sprialOverlapResolutionPriority[overlappingSpirals.x] = 0;
            if (!sprialOverlapResolutionPriority.ContainsKey(overlappingSpirals.y)) sprialOverlapResolutionPriority[overlappingSpirals.y] = 0;

            int mirrorIndex = overlappingSpirals.x;
            if (sprialOverlapResolutionPriority[overlappingSpirals.y] > sprialOverlapResolutionPriority[overlappingSpirals.x]) mirrorIndex = overlappingSpirals.y;

            this.spirals[mirrorIndex].Mirror();
            sprialOverlapResolutionPriority[mirrorIndex]--;

            return true;
        }
        
        public Vector2Int FindOverlap() 
        {
            var index = -1;
            foreach (var s1 in spirals) 
            {
                index++;
                if (s1.parent == null) continue;

                var jndex = -1;
                foreach (var s2 in spirals) 
                {
                    jndex++;
                    if (SpiralsOverlap(s1, s2)) return new Vector2Int(index, jndex);;
                }
            }

            return new Vector2Int(-1, -1);
        }

        public bool SpiralsOverlap(SpiralManipulator s1, SpiralManipulator s2) 
        {
            if (s1 == s2) return false;
            if (Vector3.Distance(s1.center, s2.center) > Mathf.Max(s1.NomaiTextLine.GetWorldRadius(), s2.NomaiTextLine.GetWorldRadius())) return false; // no overlap possible - too far away

            var s1Points = s1.NomaiTextLine.GetPoints().Select(p => s1.transform.TransformPoint(p)).ToList();
            var s2Points = s2.NomaiTextLine.GetPoints().Select(p => s2.transform.TransformPoint(p)).ToList();
            var s1ThresholdForOverlap = Vector3.Distance(s1Points[0], s1Points[1]);
            var s2ThresholdForOverlap = Vector3.Distance(s2Points[0], s2Points[1]);
            var thresholdForOverlap = Mathf.Pow(Mathf.Max(s1ThresholdForOverlap, s2ThresholdForOverlap), 2); // square to save on computation (we'll work in distance squared from here on)

            if (s1.parent == s2) s1Points.RemoveAt(0); // don't consider the base points so that we can check if children overlap their parents 
            if (s2.parent == s1) s2Points.RemoveAt(0); // (note: the base point of a child is always exactly overlapping with one of the parent's points)

            foreach(var p1 in s1Points)
            {
                foreach(var p2 in s2Points)
                {
                    if (Vector3.SqrMagnitude(p1-p2) <= thresholdForOverlap) return true; // s1 and s2 overlap
                }
            }

            return false;
        }
        
        public bool OutsideBounds(SpiralManipulator spiral) 
        {
            var points = spiral.NomaiTextLine.GetPoints()
                .Select(p => spiral.transform.TransformPoint(p))
                .Select(p => spiral.transform.parent.InverseTransformPoint(p))
                .ToList();

            foreach(var point in points) {
                if (point.x < minX || point.x > maxX ||
                    point.y < minY || point.y > maxY) 
                {
                    return true;
                }
            }

            return false;
        }
        
        public void FDGSimulationStep() 
        {
            if (reverseToposortedSpirals == null) GenerateReverseToposort();

            Dictionary<SpiralManipulator, Vector2> childForces = new Dictionary<SpiralManipulator, Vector2>();

            foreach (var s1 in reverseToposortedSpirals) // treating the conversation like a tree datastructure, move "leaf" spirals first so that we can propogate their force up to the parents
            {
                Vector2 force = Vector2.zero;

                // accumulate the force the children feel
                if (childForces.ContainsKey(s1)) 
                {
                    force += 0.9f * childForces[s1];
                }
            
                // push away from fellow spirals
                foreach (var s2 in spirals) 
                {
                    if (s1 == s2) continue;
                    if (s1.parent == s2) continue;
                    if (s1 == s2.parent) continue;

                    var f = (s2.center - s1.center);
                    force -= f / Mathf.Pow(f.magnitude, 6);

                    var f2 = (s2.localPosition - s1.localPosition);
                    force -= f2 / Mathf.Pow(f2.magnitude, 6);
                }
            
                // push away from the edges
                var MAX_EDGE_PUSH_FORCE = 1;
                force += new Vector2(0, -1) * Mathf.Max(0, (s1.transform.localPosition.y + maxY)*(MAX_EDGE_PUSH_FORCE / maxY) - MAX_EDGE_PUSH_FORCE);
                force += new Vector2(0,    1) * Mathf.Max(0, (s1.transform.localPosition.y + minY)*(MAX_EDGE_PUSH_FORCE / minY) - MAX_EDGE_PUSH_FORCE);
                force += new Vector2(-1, 0) * Mathf.Max(0, (s1.transform.localPosition.x + maxX)*(MAX_EDGE_PUSH_FORCE / maxX) - MAX_EDGE_PUSH_FORCE);
                force += new Vector2(1,    0) * Mathf.Max(0, (s1.transform.localPosition.x + minX)*(MAX_EDGE_PUSH_FORCE / minX) - MAX_EDGE_PUSH_FORCE);

                // push up just to make everything a little more pretty (this is not neccessary to get an arrangement that simply has no overlap/spirals exiting the bounds)
                force += new Vector2(0,    1) * 1;
            
                // renormalize the force magnitude (keeps force sizes reasonable, and improves stability in the case of small forces)
                var avg = 1; // the size of vector required to get a medium push
                var scale = 0.75f;
                force = force.normalized * scale * (1 / (1 + Mathf.Exp(avg-force.magnitude)) - 1 / (1 + Mathf.Exp(avg))); // apply a sigmoid-ish smoothing operation, so only giant forces actually move the spirals

                // if this is the root spiral, then rotate it instead of trying to move it
                if (s1.parent == null) 
                {
                    // this is the root spiral, so rotate instead of moving
                    var finalAngle = Mathf.Atan2(force.y, force.x); // root spiral is always at 0, 0
                    var currentAngle = Mathf.Atan2(s1.center.y, s1.center.x); // root spiral is always at 0, 0
                    s1.transform.localEulerAngles = new Vector3(0, 0, finalAngle-currentAngle);
                    s1.UpdateChildren();

                    continue;
                }

                // pick the parent point that's closest to center+force, and move to there
                var spiral = s1;
                var parentPoints = spiral.parent.GetComponent<NomaiTextLine>().GetPoints();
            
                var idealPoint = spiral.position + force;
                var bestPointIndex = 0;
                var bestPointDistance = 99999999f;
                for (var j = SpiralManipulator.MIN_PARENT_POINT; j < SpiralManipulator.MAX_PARENT_POINT; j++) 
                {
                    // don't put this spiral on a point already occupied by a sibling
                    if (j != spiral._parentPointIndex && spiral.parent.pointsOccupiedByChildren.Contains(j)) continue;

                    var point = parentPoints[j];
                    point = spiral.parent.transform.TransformPoint(point);

                    var dist = Vector2.Distance(point, idealPoint);
                    if (dist < bestPointDistance) {
                        bestPointDistance = dist;
                        bestPointIndex = j;
                    }
                }
            
                // limit the distance a spiral can move in a single step
                bestPointIndex = spiral._parentPointIndex + Mathf.Min(MAX_MOVE_DISTANCE, Mathf.Max(-MAX_MOVE_DISTANCE, bestPointIndex - spiral._parentPointIndex)); // minimize step size to help stability
            
                // actually move the spiral
                spiral.PlaceOnParentPoint(bestPointIndex);
            
                // Enforce bounds
                if (OutsideBounds(s1)) 
                {
                    var start = s1._parentPointIndex;
                    var originalMirror = s1.Mirrored;

                    var success = AttemptToPushSpiralInBounds(s1, start);
                    if (!success) 
                    {
                        // try flipping it if nothing worked with original mirror state
                        s1.Mirror(); 
                        success = AttemptToPushSpiralInBounds(s1, start);
                    }

                    if (!success) 
                    {
                        // if we couldn't put it inside the bounds, put it back how we found it (this increases stability of the rest of the spirals)
                        if (s1.Mirrored != originalMirror) s1.Mirror();
                        s1.PlaceOnParentPoint(start);
                        Logger.LogVerbose("Unable to place spiral " + s1.gameObject.name + " within bounds.");
                    }
                }

                // record force for parents
                if (!childForces.ContainsKey(s1.parent)) childForces[s1.parent] = Vector2.zero;
                childForces[s1.parent] += force;
            }
        }

        private bool AttemptToPushSpiralInBounds(SpiralManipulator s1, int start) 
        {
            var range = Mathf.Max(start-SpiralManipulator.MIN_PARENT_POINT, SpiralManipulator.MAX_PARENT_POINT-start);

            for (var i = 1; i <= range; i++)
            {
                if (start-i >= SpiralManipulator.MIN_PARENT_POINT) 
                { 
                    s1.PlaceOnParentPoint(start-i);
                    if (!OutsideBounds(s1)) return true;
                }
                    
                if (start+i <= SpiralManipulator.MAX_PARENT_POINT) 
                { 
                    s1.PlaceOnParentPoint(start+i);
                    if (!OutsideBounds(s1)) return true;
                }
            }

            return false;
        }
    }

    public class SpiralManipulator : MonoBehaviour {
        public SpiralManipulator parent;
        public List<SpiralManipulator> children = new List<SpiralManipulator>();

        public HashSet<int> pointsOccupiedByChildren = new HashSet<int>();
        public int _parentPointIndex = -1;
    
        public static int MIN_PARENT_POINT = 3;
        public static int MAX_PARENT_POINT = 26;

        #region properties

        public bool Mirrored { get { return this.transform.localScale.x < 0; } }

        private NomaiTextLine _NomaiTextLine;
        public NomaiTextLine NomaiTextLine 
        {
            get 
            {
                if (_NomaiTextLine == null) _NomaiTextLine = GetComponent<NomaiTextLine>();
                return _NomaiTextLine;
            }
        }

        public Vector2 center 
        { 
            get { return NomaiTextLine.GetWorldCenter(); } 
        }

        public Vector2 localPosition 
        {
            get { return new Vector2(this.transform.localPosition.x, this.transform.localPosition.y); }
        }
        public Vector2 position 
        {
            get { return new Vector2(this.transform.position.x, this.transform.position.y); }
        }
        
        #endregion properties
        
        public SpiralManipulator AddChild(NomaiTextArcBuilder.SpiralProfile profile) {
            var child = NomaiTextArcArranger.CreateSpiral(profile, this.transform.parent.gameObject);

            var index = Random.Range(MIN_PARENT_POINT, MAX_PARENT_POINT);
            child.transform.parent = this.transform.parent;
            child.parent = this;
            child.PlaceOnParentPoint(index);

            this.children.Add(child);
            return child;
        }

        public void Mirror() 
        {             
            this.transform.localScale = new Vector3(-this.transform.localScale.x, 1, 1);
            if (this.parent != null) this.PlaceOnParentPoint(this._parentPointIndex);
        }
    
        public void UpdateChildren() 
        {
            foreach(var child in this.children) 
            {
                child.PlaceOnParentPoint(child._parentPointIndex);
            }
        }

        public int PlaceOnParentPoint(int parentPointIndex, bool updateChildren=true) 
        {
            // validate
            var _points = parent.GetComponent<NomaiTextLine>().GetPoints();
            parentPointIndex = Mathf.Max(0, Mathf.Min(parentPointIndex, _points.Length-1));
            
            // track occupied points
            if (this._parentPointIndex != -1) parent.pointsOccupiedByChildren.Remove(this._parentPointIndex);
            this._parentPointIndex = parentPointIndex;
            parent.pointsOccupiedByChildren.Add(parentPointIndex);

            // calculate the normal
            var normal = _points[Mathf.Min(parentPointIndex+1, _points.Length-1)] - _points[Mathf.Max(parentPointIndex-1, 0)];
            if (parent.transform.localScale.x < 0) normal = new Vector3(normal.x, -normal.y, -normal.z);
            float rot = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;

            // get location of the point
            var point = _points[parentPointIndex];
            if (parent.transform.localScale.x < 0) point = new Vector3(-point.x, point.y, point.z);

            // finalize
            this.transform.localPosition = Quaternion.Euler(0, 0, parent.transform.localEulerAngles.z) * point + parent.transform.localPosition;
            this.transform.localEulerAngles = new Vector3(0, 0, rot + parent.transform.localEulerAngles.z);
            if (updateChildren) this.UpdateChildren();

            return parentPointIndex;
        }
    }
}