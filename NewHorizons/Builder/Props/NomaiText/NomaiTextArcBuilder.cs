using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class NomaiTextArcBuilder {
        public static int i = 0;
        public static bool removeBakedInRotationAndPosition = true;        

        public static void PlaceAdult() 
        { 
            BuildSpiralGameObject(adultSpiralProfile, "Text Arc Prefab " + (i++));
        }
        public static void PlaceChild() 
        {
            BuildSpiralGameObject(childSpiralProfile, "Text Arc Prefab " + (i++));
        }
        public static void PlaceStranger() 
        {
            BuildSpiralGameObject(strangerSpiralProfile, "Text Arc Prefab " + (i++));
        }

        public static GameObject BuildSpiralGameObject(SpiralProfile profile, string goName="New Nomai Spiral") 
        {
            var g = new GameObject(goName);
            g.SetActive(false);
            g.transform.localPosition = Vector3.zero;
            g.transform.localEulerAngles = Vector3.zero;

            var m = new SpiralMesh(profile);
            m.Randomize();
            m.updateMesh();

            g.AddComponent<MeshFilter>().sharedMesh = m.mesh;
            g.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            g.GetComponent<MeshRenderer>().sharedMaterial.color = Color.magenta;

            var owNomaiTextLine = g.AddComponent<NomaiTextLine>();

            //
            // rotate mesh to face up
            //
        
            var norm = m.skeleton[1] - m.skeleton[0];
            float r = Mathf.Atan2(-norm.y, norm.x) * Mathf.Rad2Deg;
            if (m.mirror) r += 180;
            var ang = m.mirror ? 90-r : -90-r;

            // using m.sharedMesh causes old meshes to disappear for some reason, idk why
            var mesh = g.GetComponent<MeshFilter>().mesh;
            if (removeBakedInRotationAndPosition)
            {
                var meshCopy = mesh;
                var newVerts = meshCopy.vertices.Select(v => Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, ang, 0) * v).ToArray();
                meshCopy.vertices = newVerts;
                meshCopy.RecalculateBounds();
            }

            // TODO: caching? would this help with that?
            //AssetDatabase.CreateAsset(mesh, "Assets/Spirals/"+(profile.profileName)+"spiral" + (NomaiTextArcBuilder.i) + ".asset");
            //g.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath("Assets/Spirals/"+(profile.profileName)+"spiral" + (NomaiTextArcBuilder.i) + ".asset", typeof(Mesh)) as Mesh;
            //NomaiTextArcBuilder.i++;

            //
            // set up NomaiTextArc stuff
            //
        
            var _points = m.skeleton
                .Select((compiled) => 
                    Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, ang, 0) * (new Vector3(compiled.x, 0, compiled.y)) // decompile them, rotate them by ang, and then rotate them to be vertical, like the base game spirals are
                )
                .ToArray();

            owNomaiTextLine._points = _points;
            owNomaiTextLine._state = NomaiTextLine.VisualState.HIDDEN;
            owNomaiTextLine._textLineLocation = NomaiText.Location.UNSPECIFIED;
            owNomaiTextLine._active = true;
            owNomaiTextLine._prebuilt = false;

            g.SetActive(true);
            return g;
        }

        //
        //
        // Handle the connection between game objects and spiral meshes
        //
        //

        public struct SpiralProfile {
            // all of the Vector2 params here refer to a range of valid values
            public string profileName;
            public bool canMirror;
            public Vector2 a;
            public Vector2 b;
            public Vector2 startS;
            public Vector2 endS;
            public Vector2 skeletonScale;
            public int numSkeletonPoints;
            public float uvScale;
            public float innerWidth; // width at the tip
            public float outerWidth; // width at the base
            public Material material;
        }
    
        public static SpiralProfile adultSpiralProfile = new SpiralProfile() {
            profileName="Adult",
            canMirror = false, // we don't want to mirror the actual mesh itself anymore, we'll just mirror the game object using localScale.x
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
            canMirror = false, // we don't want to mirror the actual mesh itself anymore, we'll just mirror the game object using localScale.x
            a = new Vector2(0.9f, 0.9f),
            b = new Vector2(0.305f, 0.4f),
            startS = new Vector2(342.8796f, 342.8796f),
            endS = new Vector2(16f, 60f), 
            skeletonScale = 0.75f * new Vector2(0.002f, 0.005f),
            numSkeletonPoints = 51,

            innerWidth = 0.001f/10f, 
            outerWidth = 2f*0.05f, 
            uvScale = 4.9f/3.5f, 
        };
        
        // location of example stranger writing:
        // RingWorld_Body/Sector_RingInterior/Sector_Zone1/Interactables_Zone1/Props_IP_ZoneSign_1/Arc_TestAlienWriting/Arc 1
        // 17 points
        // length of 1.8505
        // width of 1
        // _revealDuration of 0.5633
        // _targetColor of 1 1 1 0
        // I think this'll do it
        public static SpiralProfile strangerSpiralProfile = new SpiralProfile() {
            profileName="Stranger",
            canMirror = false,
            a = new Vector2(0.9f, 0.9f), // this value doesn't really matter for this
            b = new Vector2(5f, 5f),
            startS = new Vector2(1.8505f, 1.8505f), 
            endS = new Vector2(0, 0), 
            skeletonScale = new Vector2(1, 1),
            numSkeletonPoints = 17,

            innerWidth = 1, 
            outerWidth = 1, 
            uvScale = 1f/1.8505f, 
        };

        //
        //
        // Construct spiral meshes from the mathematical spirals generated below
        //
        //

        public class SpiralMesh: MathematicalSpiral {
            public List<Vector3> skeleton;
            public List<Vector2> skeletonOutsidePoints;

            public int numSkeletonPoints = 51; // seems to be Mobius' default

            public float innerWidth = 0.001f; // width at the tip
            public float outerWidth = 0.05f; //0.107f; // width at the base
            public float uvScale = 4.9f; //2.9f;
            private float baseUVScale = 1f / 300f;
            public float uvOffset = 0;

            public Mesh mesh;

            public SpiralMesh(SpiralProfile profile): base(profile) {
                this.numSkeletonPoints = profile.numSkeletonPoints;
                this.innerWidth = profile.innerWidth;
                this.outerWidth = profile.outerWidth;
                this.uvScale = profile.uvScale;

                this.uvOffset = UnityEngine.Random.value;
            }

            public override void Randomize() {
                base.Randomize();
                uvOffset = UnityEngine.Random.value; // this way even two spirals that are exactly the same shape will look different (this changes the starting point of the handwriting texture)
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

                var startT = tFromArcLen(startS);
                var endT = tFromArcLen(endS);

                var rangeT = endT - startT;
                var rangeS = endS - startS;

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
                    float sFraction = (inputS - startS) / rangeS;
                    float absoluteS = (inputS - startS);

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

        //
        //
        // Construct the mathematical spirals that Nomai arcs are built from
        //
        //

        public class MathematicalSpiral {
            public bool mirror;
            public float a;
            public float b; // 0.3-0.6
            public float startSOnParent;
            public float scale;
            public List<MathematicalSpiral> children;

            public float x;
            public float y;
            public float ang;

            public float startS = 42.87957f; // go all the way down to 0, all the way up to 50
            public float endS = 342.8796f;

            SpiralProfile profile;

            public MathematicalSpiral(SpiralProfile profile) {
                this.profile = profile;

                this.Randomize();
            }

            public MathematicalSpiral(float startSOnParent = 0, bool mirror = false, float len = 300, float a = 0.5f, float b = 0.43f, float scale = 0.01f) {
                this.mirror = mirror;
                this.a = a;
                this.b = b;
                this.startSOnParent = startSOnParent;
                this.scale = scale;

                this.children = new List<MathematicalSpiral>();

                this.x = 0;
                this.y = 0;
                this.ang = 0;
            }

            public virtual void Randomize() {
                this.a = UnityEngine.Random.Range(profile.a.x, profile.a.y); //0.5f;
                this.b = UnityEngine.Random.Range(profile.b.x, profile.b.y);
                this.startS = UnityEngine.Random.Range(profile.endS.x, profile.endS.y);   // idk why I flipped these, please don't hate me
                this.endS = UnityEngine.Random.Range(profile.startS.x, profile.startS.y);
                this.scale = UnityEngine.Random.Range(profile.skeletonScale.x, profile.skeletonScale.y);
                if (profile.canMirror) this.mirror = UnityEngine.Random.value<0.5f;
            }

            internal virtual void updateChild(MathematicalSpiral child) {
                Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent);
                var cx = pointAndNormal.x;
                var cy = pointAndNormal.y;
                var cang = pointAndNormal.z;
                child.x = cx;
                child.y = cy;
                child.ang = cang + (child.mirror ? Mathf.PI : 0);
            }

            public virtual void addChild(MathematicalSpiral child) {
                updateChild(child);
                this.children.Add(child);
            }

            public virtual void updateChildren() {
                this.children.ForEach(child => {
                    updateChild(child);
                    child.updateChildren();
                });
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
                var endT = tFromArcLen(endS);
                var startT = tFromArcLen(startS);
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

            // all of this math is based off of this:
            // https://www.desmos.com/calculator/9gdfgyuzf6
            //
            // note: t refers to theta, and s refers to arc length
            //

            // get the (x, y) coordinates and the normal angle at the given location (measured in arcLen) of a spiral with the given parameters 
            // note: arcLen is inverted so that 0 refers to what we consider the start of the Nomai spiral
            public Vector3 getDrawnSpiralPointAndNormal(float arcLen) {
                float offsetX = this.x;
                float offsetY = this.y;
                float offsetAngle = this.ang;
                var startS = this.endS; // I know this is funky, but just go with it for now. 

                var startT = tFromArcLen(startS); // this is the `t` value for the root of the spiral (the end of the non-curled side)

                var startPoint = spiralPoint(startT); // and this is the (x,y) location of the non-curled side, relative to the rest of the spiral. we'll offset everything so this is at (0,0) later
                var startX = startPoint.x;
                var startY = startPoint.y;

                var t = tFromArcLen(arcLen);
                var point = spiralPoint(t); // the absolute (x,y) location that corresponds to `arcLen`, before accounting for things like putting the start point at (0,0), or dealing with offsetX/offsetY/offsetAngle
                var x = point.x;
                var y = point.y;
                var ang = normalAngle(t);

                if (mirror) {
                    x = x + 2 * (startX - x);
                    ang = -ang + Mathf.PI;
                }

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
    }
}