using NewHorizons.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public class NomaiTextArcArranger : MonoBehaviour {
        public List<SpiralManipulator> spirals = new List<SpiralManipulator>();
        public List<SpiralManipulator> reverseToposortedSpirals = null;
        public SpiralManipulator root { get; private set; }
        private Dictionary<int, int> sprialOverlapResolutionPriority = new Dictionary<int, int>();

        private static int MAX_MOVE_DISTANCE = 2;

        public float maxX = 2.7f;//3;
        public float minX = -2.7f;//-3;
        public float maxY = 2.6f;//2.75f;
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
        public void LimitRepeatedMirrors() 
        {
            if(true) return; // temp disable

            foreach(var spiral in reverseToposortedSpirals) 
            {
                if (spiral.Mirrored == spiral.parent?.Mirrored && spiral.Mirrored == spiral.parent?.parent?.Mirrored) 
                {
                    spiral.parent.parent.Mirror(); // flipping the grandparent causes every 3rd spiral in a row to be flipped, meaning the maximum number of spirals in a row with the same mirror is 2
                }
            }
        }

        public static SpiralManipulator Place(NomaiTextArcBuilder.SpiralProfile profile, GameObject spiralMeshHolder) 
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

        public void OnDrawGizmosSelected() 
        {
            var topLeft         = new Vector3(minX, maxY) + transform.position;
            var topRight        = new Vector3(maxX, maxY) + transform.position;
            var bottomRight = new Vector3(maxX, minY) + transform.position;
            var bottomLeft    = new Vector3(minX, minY) + transform.position;
            Debug.DrawLine(topLeft, topRight, Color.red);
            Debug.DrawLine(topRight, bottomRight, Color.red);
            Debug.DrawLine(bottomRight, bottomLeft, Color.red);
            Debug.DrawLine(bottomLeft, topLeft, Color.red);
        }

        public int AttemptOverlapResolution(Vector2Int overlappingSpirals) 
        {
            if (!sprialOverlapResolutionPriority.ContainsKey(overlappingSpirals.x)) sprialOverlapResolutionPriority[overlappingSpirals.x] = 0;
            if (!sprialOverlapResolutionPriority.ContainsKey(overlappingSpirals.y)) sprialOverlapResolutionPriority[overlappingSpirals.y] = 0;

            int mirrorIndex = overlappingSpirals.x;
            if (sprialOverlapResolutionPriority[overlappingSpirals.y] > sprialOverlapResolutionPriority[overlappingSpirals.x]) mirrorIndex = overlappingSpirals.y;

            this.spirals[mirrorIndex].Mirror();
            sprialOverlapResolutionPriority[mirrorIndex]--;

            return mirrorIndex;
        }
        
        public Vector2Int Overlap() 
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
                    if (Overlap(s1, s2)) return new Vector2Int(index, jndex);;
                }
            }

            return new Vector2Int(-1, -1);
        }

        public bool Overlap(SpiralManipulator s1, SpiralManipulator s2) 
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

        
        public void Step() 
        {
            if (reverseToposortedSpirals == null) GenerateReverseToposort();

            // TODO: fix LocationB not making spiral orange

            Dictionary<SpiralManipulator, Vector2> childForces = new Dictionary<SpiralManipulator, Vector2>();

            //Debug.Log(reverseToposortedSpirals.Count);
            //Debug.Log( string.Join(", ", reverseToposortedSpirals.Select(hmslnk => hmslnk.gameObject.name) ) );

            var index = -1;
            foreach (var s1 in reverseToposortedSpirals) // treating the conversation like a tree datastructure, move "leaf" spirals first so that we can propogate their force up to the parents
            {
                index++;

                Vector2 force = Vector2.zero;

                //
                // Calculate the force s1 should experience from its children
                //

                if (childForces.ContainsKey(s1)) 
                {
                    force += 0.9f * childForces[s1];
                }
            
                //
                // Calculate the force s1 should experience from fellow spirals
                //

                foreach (var s2 in spirals) 
                {
                    if (s1 == s2) continue;
                    if (s1.parent == s2) continue;
                    if (s1 == s2.parent) continue;

                    //if (!Overlap(s1, s2)) continue;
                
                    // push away from other spirals
                    var f = (s2.center - s1.center);
                    force -= f / Mathf.Pow(f.magnitude, 6);

                    var f2 = (s2.localPosition - s1.localPosition);
                    force -= f2 / Mathf.Pow(f2.magnitude, 6);
                }
            
                //
                // push away from the edges
                //

                var MAX_EDGE_PUSH_FORCE = 1;
                force += new Vector2(0, -1) * Mathf.Max(0, (s1.transform.localPosition.y + maxY)*(MAX_EDGE_PUSH_FORCE / maxY) - MAX_EDGE_PUSH_FORCE);
                force += new Vector2(0,    1) * Mathf.Max(0, (s1.transform.localPosition.y + minY)*(MAX_EDGE_PUSH_FORCE / minY) - MAX_EDGE_PUSH_FORCE);
                force += new Vector2(-1, 0) * Mathf.Max(0, (s1.transform.localPosition.x + maxX)*(MAX_EDGE_PUSH_FORCE / maxX) - MAX_EDGE_PUSH_FORCE);
                force += new Vector2(1,    0) * Mathf.Max(0, (s1.transform.localPosition.x + minX)*(MAX_EDGE_PUSH_FORCE / minX) - MAX_EDGE_PUSH_FORCE);

                // push up just to make everything a little more pretty (this is not neccessary to get an arrangement that simply has no overlap/spirals exiting the bounds)
                force += new Vector2(0,    1) * 1;
            
                //
                // renormalize the force magnitude (keeps force sizes reasonable, and improves stability in the case of small forces)
                //

                var avg = 1; // the size of vector required to get a medium push
                var scale = 0.75f;
                force = force.normalized * scale * (1 / (1 + Mathf.Exp(avg-force.magnitude)) - 1 / (1 + Mathf.Exp(avg))); // apply a sigmoid-ish smoothing operation, so only giant forces actually move the spirals


                //
                // if this is the root spiral, then rotate it instead of trying to move it (what the rest of the code does)
                //

                if (s1.parent == null) 
                {
                    // this is the root spiral, so rotate instead of moving
                    var finalAngle = Mathf.Atan2(force.y, force.x); // root spiral is always at 0, 0
                    var currentAngle = Mathf.Atan2(s1.center.y, s1.center.x); // root spiral is always at 0, 0
                    s1.transform.localEulerAngles = new Vector3(0, 0, finalAngle-currentAngle);
                    s1.UpdateChildren();

                    continue;
                }

                //
                // look for the point closest to where the forces want to push this spiral
                //

                var spiral = s1;
                var parentPoints = spiral.parent.GetComponent<NomaiTextLine>().GetPoints();
            
                // pick the parent point that's closest to center+force, and move to there
                var idealPoint = spiral.position + force;
                var bestPointIndex = 0;
                var bestPointDistance = 99999999f;
                for (var j = SpiralManipulator.MIN_PARENT_POINT; j < SpiralManipulator.MAX_PARENT_POINT; j++) 
                {
                    // skip this point if it's already occupied by ANOTHER spiral (if it's occupied by this spiral, DO count it)
                    if (j != spiral._parentPointIndex && spiral.parent.occupiedParentPoints.Contains(j)) continue;

                    var point = parentPoints[j];
                    point = spiral.parent.transform.TransformPoint(point);

                    var dist = Vector2.Distance(point, idealPoint);
                    if (dist < bestPointDistance) {
                        bestPointDistance = dist;
                        bestPointIndex = j;
                    }
                }
            
                //
                // limit the distance a spiral can move in a single step
                //

                bestPointIndex = spiral._parentPointIndex + Mathf.Min(MAX_MOVE_DISTANCE, Mathf.Max(-MAX_MOVE_DISTANCE, bestPointIndex - spiral._parentPointIndex)); // minimize step size to help stability
            
                //
                // actually move the spiral
                //

                SpiralManipulator.PlaceChildOnParentPoint(spiral, spiral.parent, bestPointIndex);
            
                //
                // Ensure the spiral has not moved out of bounds, and if it has, move it back in bounds
                //
            
                if (OutsideBounds(s1)) 
                {
                    var start = s1._parentPointIndex;
                    var originalMirror = s1.Mirrored;

                    var success = AttemptToPushSpiralInBounds(s1, start);
                    if (!success) 
                    {
                        s1.Mirror(); // try flipping it if nothing worked with original mirror
                        success = AttemptToPushSpiralInBounds(s1, start);
                    }

                    if (!success) 
                    {
                        // if we couldn't put it inside the bounds, put it back how we found it (this increases stability of the rest of the spirals)
                        if (s1.Mirrored != originalMirror) s1.Mirror();
                        SpiralManipulator.PlaceChildOnParentPoint(s1, s1.parent, start);
                        Debug.LogWarning("Unable to place spiral " + s1.gameObject.name + " within bounds.");
                    }
                }

                if (!childForces.ContainsKey(s1.parent)) childForces[s1.parent] = Vector2.zero;
                childForces[s1.parent] += force;

                Debug.DrawRay(s1.transform.position, new Vector3(force.x, force.y, 0), Color.green);
            }
        }

        private bool AttemptToPushSpiralInBounds(SpiralManipulator s1, int start) 
        {
            var range = Mathf.Max(start-SpiralManipulator.MIN_PARENT_POINT, SpiralManipulator.MAX_PARENT_POINT-start);

            for (var i = 1; i <= range; i++)
            {
                if (start-i >= SpiralManipulator.MIN_PARENT_POINT) 
                { 
                    SpiralManipulator.PlaceChildOnParentPoint(s1, s1.parent, start-i);
                    if (!OutsideBounds(s1)) return true;
                }
                    
                if (start+i <= SpiralManipulator.MAX_PARENT_POINT) 
                { 
                    SpiralManipulator.PlaceChildOnParentPoint(s1, s1.parent, start+i);
                    if (!OutsideBounds(s1)) return true;
                }
            }

            return false;
        }
    }

    [ExecuteInEditMode]
    public class SpiralManipulator : MonoBehaviour {
        public SpiralManipulator parent;
        public List<SpiralManipulator> children = new List<SpiralManipulator>();

        public HashSet<int> occupiedParentPoints = new HashSet<int>();
        public int _parentPointIndex = -1;
    
        public static int MIN_PARENT_POINT = 3;
        public static int MAX_PARENT_POINT = 26;
    
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

        public Vector2 center { 
            get { return NomaiTextLine.GetWorldCenter(); } 
        }

        public Vector2 localPosition {
            get { return new Vector2(this.transform.localPosition.x, this.transform.localPosition.y); }
        }
        public Vector2 position {
            get { return new Vector2(this.transform.position.x, this.transform.position.y); }
        }
        
        public SpiralManipulator AddChild(NomaiTextArcBuilder.SpiralProfile profile) {
            return AddChild(NomaiTextArcArranger.Place(profile, this.transform.parent.gameObject).gameObject);
        }
        
        public SpiralManipulator AddChild(GameObject prebuiltChild) {
            var index = Random.Range(MIN_PARENT_POINT, MAX_PARENT_POINT);
            prebuiltChild.transform.parent = this.transform.parent;
            var child = prebuiltChild.gameObject.GetAddComponent<SpiralManipulator>();
            PlaceChildOnParentPoint(child, this, index);

            child.parent = this;
            this.children.Add(child);
            return child;
        }

        public void Mirror() 
        {             
            this.transform.localScale = new Vector3(-this.transform.localScale.x, 1, 1);
            if (this.parent != null) SpiralManipulator.PlaceChildOnParentPoint(this, this.parent, this._parentPointIndex);
        }
    
        public void UpdateChildren() 
        {
            foreach(var child in this.children) 
            {
                PlaceChildOnParentPoint(child, this, child._parentPointIndex);
            }
        }

        public static int PlaceChildOnParentPoint(SpiralManipulator child, SpiralManipulator parent, int parentPointIndex, bool updateChildren=true) 
        {
            // track which points on the parent are being occupied
            if (child._parentPointIndex != -1) parent.occupiedParentPoints.Remove(child._parentPointIndex);
            child._parentPointIndex = parentPointIndex; // just in case this function was called without setting this value
            parent.occupiedParentPoints.Add(parentPointIndex);

            // get the parent's points and make parentPointIndex valid
            var _points = parent.GetComponent<NomaiTextLine>().GetPoints();
            parentPointIndex = Mathf.Max(0, Mathf.Min(parentPointIndex, _points.Length-1));

            // calculate the normal at point by using the neighboring points to approximate the tangent (and account for mirroring, which means all points are actually at (-point.x, point.y) )
            var normal = _points[Mathf.Min(parentPointIndex+1, _points.Length-1)] - _points[Mathf.Max(parentPointIndex-1, 0)];
            if (parent.transform.localScale.x < 0) normal = new Vector3(-normal.x, normal.y, normal.z);
            float rot = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
            if (parent.transform.localScale.x < 0) rot += 180; // account for mirroring again (without doing this, the normal points inward on mirrored spirals, instead of outward)

            // get the location the child spiral should be at (and yet again account for mirroring)
            var point = _points[parentPointIndex];
            if (parent.transform.localScale.x < 0) point = new Vector3(-point.x, point.y, point.z);

            // set the child's position and rotation according to calculations
            child.transform.localPosition = Quaternion.Euler(0, 0, parent.transform.localEulerAngles.z) * point + parent.transform.localPosition;
            child.transform.localEulerAngles = new Vector3(0, 0, rot + parent.transform.localEulerAngles.z);

            // recursive update on all children so they move along with the parent
            if (updateChildren) 
            { 
                child.UpdateChildren();
            }

            return parentPointIndex;
        }
    }
}