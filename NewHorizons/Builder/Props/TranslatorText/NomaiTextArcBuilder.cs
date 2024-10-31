using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Props.TranslatorText
{
    public static class NomaiTextArcBuilder {    
        // TODO: stranger arcs
        // Note: building a wall text (making meshes and arranging) takes 0.1s for an example with 10 spirals
        // TODO: caching - maybe make a "cachable" annotaion! if cache does not contain results of function, run function, write results to cache file. otherwise return results from cache file. 
        // cache file should be shipped with release but doesn't need to be. if debug mode is enabled, always regen cache, if click regen configs, reload cache

        public static GameObject BuildSpiralGameObject(SpiralProfile profile, string goName="New Nomai Spiral") 
        {
            var m = new SpiralMesh(profile);
            m.Randomize();
            m.updateMesh();

            //
            // rotate mesh to point up
            //
        
            var norm = m.skeleton[1] - m.skeleton[0];
            float r = Mathf.Atan2(-norm.y, norm.x) * Mathf.Rad2Deg;
            var ang = -90-r;

            // using m.sharedMesh causes old meshes to disappear for some reason, idk why
            var mesh = m.mesh;
            var newVerts = mesh.vertices.Select(v => Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, ang, 0) * v).ToArray();
            mesh.vertices = newVerts;
            mesh.RecalculateBounds();

            // rotate the skeleton to point up, too
            var _points = m.skeleton 
                .Select((point) => 
                    Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, ang, 0) * (new Vector3(point.x, 0, point.y))
                )
                .ToArray();

            return BuildSpiralGameObject(_points, mesh, goName);
        }

        public static GameObject BuildSpiralGameObject(Vector3[] _points, Mesh mesh, string goName="New Nomai Spiral") 
        {
            var g = new GameObject(goName);
            g.SetActive(false);
            g.transform.localPosition = Vector3.zero;
            g.transform.localEulerAngles = Vector3.zero;

            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            g.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            g.GetComponent<MeshRenderer>().sharedMaterial.color = Color.magenta;

            var owNomaiTextLine = g.AddComponent<NomaiTextLine>();

            owNomaiTextLine._points = _points;
            owNomaiTextLine._active = true;
            owNomaiTextLine._prebuilt = false;

            g.SetActive(true);
            return g;
        }
        
        #region spiral shape definitions

        public struct SpiralProfile {
            // all of the Vector2 params here refer to a range of valid values
            public string profileName;

            /// <summary>
            /// What is this
            /// </summary>
            public Vector2 a;

            /// <summary>
            /// What is this
            /// </summary>
            public Vector2 b;

            /// <summary>
            /// What is this
            /// </summary>
            public Vector2 startS;

            /// <summary>
            /// What is this
            /// </summary>
            public Vector2 endS;

            /// <summary>
            /// What is this
            /// </summary>
            public Vector2 skeletonScale;

            /// <summary>
            /// What is this
            /// </summary>
            public int numSkeletonPoints;

            /// <summary>
            /// What is this
            /// </summary>
            public float uvScale;

            /// <summary>
            /// Width at tip
            /// </summary>
            public float innerWidth; 

            /// <summary>
            /// Width at base
            /// </summary>
            public float outerWidth;

            public Material material;
        }
    
        public static SpiralProfile adultSpiralProfile = new SpiralProfile() {
            profileName="Adult",
            a = new Vector2(0.5f, 0.5f),
            b = new Vector2(0.3f, 0.6f),
            startS = new Vector2(342.8796f, 342.8796f),
            endS = new Vector2(0, 50f),
            skeletonScale = 0.75f * new Vector2(0.01f, 0.01f),
            numSkeletonPoints = 51,

            innerWidth = 0.001f, 
            outerWidth = 0.05f, 
            uvScale = 4.9f,
        };

        public static SpiralProfile childSpiralProfile = new SpiralProfile() {
            profileName="Child",
            a = new Vector2(0.9f, 0.9f),
            b = new Vector2(0.17f, 0.4f), 
            startS = new Vector2(342.8796f, 342.8796f),
            endS = new Vector2(35f, 25f), 
            skeletonScale = 0.8f * new Vector2(0.01f, 0.01f),
            numSkeletonPoints = 51,

            innerWidth = 0.001f/10f, 
            outerWidth = 2f*0.05f, 
            uvScale = 4.9f * 0.55f, 
        };
        
        public static SpiralProfile strangerSpiralProfile = new SpiralProfile() {
            profileName="Stranger",
            a = new Vector2(0.9f, 0.9f), // this value doesn't really matter for this
            b = new Vector2(5f, 5f),
            startS = new Vector2(1.8505f, 1.8505f), 
            endS = new Vector2(0, 0), 
            skeletonScale = new Vector2(0.6f, 0.6f),
            numSkeletonPoints = 17,

            innerWidth = 0.75f, 
            outerWidth = 0.75f, 
            uvScale = 1f/1.8505f, 
        };
        
        
        #endregion spiral shape definitions
        
        #region mesh generation

        public class SpiralMesh: MathematicalSpiral {
            public List<Vector3> skeleton;
            public List<Vector2> skeletonOutsidePoints;

            public int numSkeletonPoints = 51; // seems to be Mobius' default

            public float innerWidth = 0.001f; // width at the tip
            public float outerWidth = 0.05f; // width at the base
            public float uvScale = 4.9f; 
            private float baseUVScale = 1f / 300f;
            public float uvOffset = 0;

            public Mesh mesh;

            public SpiralMesh(SpiralProfile profile): base(profile) {
                this.numSkeletonPoints = profile.numSkeletonPoints;
                this.innerWidth = profile.innerWidth;
                this.outerWidth = profile.outerWidth;
                this.uvScale = profile.uvScale;

                this.uvOffset = Random.value;
            }

            public override void Randomize() {
                base.Randomize();
                uvOffset = Random.value; // this way even two spirals that are exactly the same shape will look different (this changes the starting point of the handwriting texture)
            }

            internal void updateMesh() {
                skeleton = this.getSkeleton(numSkeletonPoints);
                skeletonOutsidePoints = this.getSkeletonOutsidePoints(numSkeletonPoints);
            
                List<Vector3> vertsSide1 = skeleton.Select((skeletonPoint, index) => {
                    Vector3 normal = new Vector3(cos(skeletonPoint.z), 0, sin(skeletonPoint.z));
                    float width = lerp(((float) index) / ((float) skeleton.Count()), outerWidth, innerWidth);

                    return new Vector3(skeletonPoint.x, 0, skeletonPoint.y) + width * normal;
                }).ToList();

                List<Vector3> vertsSide2 = skeleton.Select((skeletonPoint, index) => {
                    Vector3 normal = new Vector3(cos(skeletonPoint.z), 0, sin(skeletonPoint.z));
                    float width = lerp(((float) index) / ((float) skeleton.Count()), outerWidth, innerWidth);

                    return new Vector3(skeletonPoint.x, 0, skeletonPoint.y) - width * normal;
                }).ToList();

                Vector3[] newVerts = vertsSide1.Zip(vertsSide2, (f, s) => new [] {
                 f,
                    s
                }).SelectMany(f =>f).ToArray(); // interleave vertsSide1 and vertsSide2
            
                List<int> triangles = new List<int>();
                for (int i = 0; i<newVerts.Length - 2; i += 2) {
                        /*
                          |  ⟍  |
                          |    ⟍|
                        2 *-----* 3                                    
                          |⟍    |                                     
                          |  ⟍  |                
                          |    ⟍|                                     
                        0 *-----* 1             
                          |⟍    | 
                        */
                        triangles.Add(i + 2);
                        triangles.Add(i + 1);
                        triangles.Add(i);

                        triangles.Add(i + 2);
                        triangles.Add(i + 3);
                        triangles.Add(i + 1);
                }

                var startT = tFromArcLen(endS);
                var endT = tFromArcLen(startS);

                var rangeT = endT - startT;
                var rangeS = startS - endS;

                Vector2[] uvs = new Vector2[newVerts.Length];
                Vector2[] uv2s = new Vector2[newVerts.Length];
                for (int i = 0; i<skeleton.Count(); i++) {
                    float fraction = 1 - ((float) i) / ((float) skeleton.Count()); // casting is so uuuuuuuugly

                    // note: cutting the sprial into numPoints equal slices of arclen would
                    // provide evenly spaced skeleton points
                    // on the other hand, cutting the spiral into numPoints equal slices of t
                    // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
                    float inputT = startT + rangeT * fraction;
                    float inputS = tToArcLen(inputT);
                    float sFraction = (inputS - endS) / rangeS;
                    float absoluteS = (inputS - endS);

                    float u = absoluteS * uvScale * baseUVScale + uvOffset;
                    uvs[i * 2] = new Vector2(u, 0);
                    uvs[i * 2 + 1] = new Vector2(u, 1);

                    uv2s[i * 2] = new Vector2(1 - sFraction, 0);
                    uv2s[i * 2 + 1] = new Vector2(1 - sFraction, 1);
                }

                Vector3[] normals = new Vector3[newVerts.Length];
                for (int i = 0; i<newVerts.Length; i++) normals[i] = new Vector3(0, 0, 1);

                if (mesh == null){
                    mesh = new Mesh();
                }
                mesh.vertices = newVerts.ToArray();
                mesh.triangles = triangles.ToArray().Reverse().ToArray(); // triangles need to be reversed so the spirals face the right way (I generated them backwards above, on accident)
                mesh.uv = uvs;
                mesh.uv2 = uv2s;
                mesh.normals = normals;
                mesh.RecalculateBounds();
            }
        }
        
        #endregion mesh generation

        #region underlying math

        // NOTE: startS is greater than endS because the math equation traces the spiral outward - it starts at the center
        // and winds its way out. However, since we want to think of the least curly part as the start, that means we
        // start at a higher S and end at a lower S
        //
        // note: t refers to theta, and s refers to arc length
        //
        // All this math is based off this Desmos graph I made. Play around with it if something doesn't make sense :)
        // https://www.desmos.com/calculator/9gdfgyuzf6
        public class MathematicalSpiral {
            public float a;
            public float b; 
            public float startSOnParent;
            public float scale;

            public float x;
            public float y;
            public float ang;

            public float endS = 42.87957f; 
            public float startS = 342.8796f;

            SpiralProfile profile;

            public MathematicalSpiral(SpiralProfile profile) {
                this.profile = profile;

                this.Randomize();
            }

            public MathematicalSpiral(float startSOnParent = 0, float len = 300, float a = 0.5f, float b = 0.43f, float scale = 0.01f) {
                this.a = a;
                this.b = b;
                this.startSOnParent = startSOnParent;
                this.scale = scale;

                this.x = 0;
                this.y = 0;
                this.ang = 0;
            }

            public virtual void Randomize() {
                this.a = Random.Range(profile.a.x, profile.a.y);
                this.b = Random.Range(profile.b.x, profile.b.y);
                this.endS = Random.Range(profile.endS.x, profile.endS.y);
                this.startS = Random.Range(profile.startS.x, profile.startS.y);
                this.scale = Random.Range(profile.skeletonScale.x, profile.skeletonScale.y);
            }

            internal virtual void updateChild(MathematicalSpiral child) {
                Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent);
                var cx = pointAndNormal.x;
                var cy = pointAndNormal.y;
                var cang = pointAndNormal.z;
                child.x = cx;
                child.y = cy;
                child.ang = cang;
            }

            // note: each Vector3 in this list is of form <x, y, angle in radians of the normal at this point>
            public List<Vector3> getSkeleton(int numPoints) {
                var skeleton =
                    WalkAlongSpiral(numPoints)
                    .Select(input => {
                        float inputS = input.y;
                        var skeletonPoint = getDrawnSpiralPointAndNormal(inputS);
                        return skeletonPoint;
                    })
                    .Reverse()
                    .ToList();

                return skeleton;
            }
        
            public List<Vector2> getSkeletonOutsidePoints(int numPoints) {
                var outsidePoints =
                    WalkAlongSpiral(numPoints)
                    .Select(input => {
                        float inputT = input.x;
                        float inputS = input.y;

                        var skeletonPoint = getDrawnSpiralPointAndNormal(inputS);

                        var deriv = spiralDerivative(inputT);
                        var outsidePoint = new Vector2(skeletonPoint.x, skeletonPoint.y) - (new Vector2(-deriv.y, deriv.x)).normalized * 0.1f;
                        return outsidePoint;
                    })
                    .Reverse()
                    .ToList();

                return outsidePoints;
            }
        
            // generate a list of <inputT, inputS> evenly distributed over the whole spiral. `numPoints` number of <inputT, inputS> pairs are generated
            public IEnumerable<Vector2> WalkAlongSpiral(int numPoints) {
                var endT = tFromArcLen(startS);
                var startT = tFromArcLen(endS);
                var rangeT = endT - startT;

                for (int i = 0; i<numPoints; i++) {
                    float fraction = ((float) i) / ((float) numPoints - 1f); // casting is so uuuuuuuugly

                    // note: cutting the sprial into numPoints equal slices of arclen would
                    // provide evenly spaced skeleton points
                    // on the other hand, cutting the spiral into numPoints equal slices of t
                    // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
                    float inputT = startT + rangeT * fraction;
                    float inputS = tToArcLen(inputT);

                    yield return new Vector2(inputT, inputS);
                }
            }

            // get the (x, y) coordinates and the normal angle at the given location (measured in arcLen) of a spiral with the given parameters 
            // note: arcLen is inverted so that 0 refers to what we consider the start of the Nomai spiral
            public Vector3 getDrawnSpiralPointAndNormal(float arcLen) {
                float offsetX = this.x;
                float offsetY = this.y;
                float offsetAngle = this.ang;

                var startT = tFromArcLen(startS); // this is the `t` value for the root of the spiral (the end of the non-curled side)

                var startPoint = spiralPoint(startT); // and this is the (x,y) location of the non-curled side, relative to the rest of the spiral. we'll offset everything so this is at (0,0) later
                var startX = startPoint.x;
                var startY = startPoint.y;

                var t = tFromArcLen(arcLen);
                var point = spiralPoint(t); // the absolute (x,y) location that corresponds to `arcLen`, before accounting for things like putting the start point at (0,0), or dealing with offsetX/offsetY/offsetAngle
                var x = point.x;
                var y = point.y;
                var ang = normalAngle(t);

                // translate so that startPoint is at (0,0)
                // (also scale the spiral)
                var retX = scale * (x - startX);
                var retY = scale * (y - startY);

                // rotate offsetAngle rads 
                var retX2 = retX * cos(offsetAngle) -
                    retY * sin(offsetAngle);
                var retY2 = retX * sin(offsetAngle) +
                    retY * cos(offsetAngle);

                retX = retX2;
                retY = retY2;

                // translate for offsetX, offsetY
                retX += offsetX;
                retY += offsetY;

                return new Vector3(retX, retY, ang + offsetAngle + Mathf.PI / 2f);
            }

            // the base formula for the spiral
            public Vector2 spiralPoint(float t) {
                var r = a * exp(b * t);
                var retval = new Vector2(r * cos(t), r * sin(t));
                return retval;
            }

            // the spiral's got two functions: x(t) and y(t)
            // so it's got two derrivatives (with respect to t) x'(t) and y'(t)
            public Vector2 spiralDerivative(float t) { // derrivative with respect to t
                var r = a * exp(b * t);
                return new Vector2(
                    -r * (sin(t) - b * cos(t)),
                    r * (b * sin(t) + cos(t))
                );
            }

            // returns the length of the spiral between t0 and t1
            public float spiralArcLength(float t0, float t1) {
                return (a / b) * sqrt(b * b + 1) * (exp(b * t1) - exp(b * t0));
            }

            // converts from a value of t to the equivalent value of s (the value of s that corresponds to the same point on the spiral as t)
            public float tToArcLen(float t) {
                return spiralArcLength(0, t);
            }

            // reverse of above
            public float tFromArcLen(float s) {
                return ln(
                    1 + s / (
                        (a / b) * sqrt(b * b + 1)
                    )
                ) / b;
            }

            // returns the angle of the spiral's normal at a given point
            public float normalAngle(float t) {
                var d = spiralDerivative(t);
                var n = new Vector2(d.y, -d.x);
                var angle = Mathf.Atan2(n.y, n.x);

                return angle - Mathf.PI / 2;
            }
        }

        // convenience, so the math above is more readable
        private static float lerp(float a, float b, float t) {
            return a * t + b * (1 - t);
        }

        private static float cos(float t) {
            return Mathf.Cos(t);
        }
        private static float sin(float t) {
            return Mathf.Sin(t);
        }
        private static float exp(float t) {
            return Mathf.Exp(t);
        }
        private static float sqrt(float t) {
            return Mathf.Sqrt(t);
        }
        private static float ln(float t) {
            return Mathf.Log(t);
        }

        
        #endregion underlying math
    }
}