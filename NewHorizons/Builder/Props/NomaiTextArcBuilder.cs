using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class NomaiTextArcBuilder
    {
        //
        //
        // TODO: move all this code to NomaiTextBuilder and delete this file
        //
        //

        public class Spiral {
            public bool mirror;
            public float a;
            public float b;
            public float len;
            public float startSOnParent;
            public float scale;
            public List<Spiral> children;
            
            public float x;
            public float y;
            public float ang;


            public float startIndex = 2.5f;

            public Spiral(float startSOnParent=0, bool mirror=false, float len=-2, float a=0.7f, float b=0.305f, float scale=20) 
            {
                this.mirror = mirror;
                this.a = a;
                this.b = b;
                this.len = len;
                this.startSOnParent = startSOnParent;
                this.scale = scale;

                this.children = new List<Spiral>();

                //this.params = [mirror, scale, a, b, len]

                this.x = 0;
                this.y = 0;
                this.ang = 0;
            }

            private void updateChild(Spiral child)
            {
                Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent, this.x, this.y, this.ang, this.mirror, this.scale, this.a, this.b); //, this.len); // len is not needed - this function pretends the spiral is infinite
                var cx = pointAndNormal.x;
                var cy = pointAndNormal.y;
                var cang = pointAndNormal.z;
                child.x = cx;
                child.y = cy;
                child.ang = cang+(child.mirror?Mathf.PI:0);
            }

            public void addChild(Spiral child)
            {
                updateChild(child);
                this.children.Add(child);
            }

            public void updateChildren() 
            {
                this.children.ForEach(child => {
                    updateChild(child);
                    child.updateChildren();
                });
            }

            public List<Vector3> getSkeleton(int numPoints)
            {
                var startT = spiralStartT(startIndex, a, b);
                var startS = tToArcLen(startT, a, b);
                var endS = startS - len; // remember the spiral is defined backwards, so the start is the larger value
                var endT = tFromArcLen(endS, a, b);

                var rangeT = startT-endT;    

                List<Vector3> skeleton = new List<Vector3>();
                for (int i = 0; i < numPoints; i++)
                {
                    float fraction = ((float)i)/((float)numPoints); // casting is so uuuuuuuugly

                    // note: cutting the sprial into numPoints equal slices of arclen would
                    // provide evenly spaced skeleton points
                    // on the other hand, cutting the spiral into numPoints equal slices of t
                    // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
                    float inputT = rangeT*fraction;
                    float inputS = tToArcLen(inputT, a, b);

                    skeleton.Add(getDrawnSpiralPointAndNormal(inputS, x, y, ang, mirror, scale, a, b));
                }

                return skeleton;
            }

            //draw() {
            //    drawSpiral(drawMsg, this.x, this.y, this.ang, ...this.params)

            //    this.children.forEach(child => child.draw())
            //}
        }

        

        // all of this math is based off of this:
        // https://www.desmos.com/calculator/9gdfgyuzf6

        // note: t refers to theta, and s refers to arc length
        //


        private static float lerp(float a, float b, float t) {
            return a*t + b*(1-t);
        }
        
        private static float cos(float t) { return Mathf.Cos(t); }
        private static float sin(float t) { return Mathf.Sin(t); }
        private static float exp(float t) { return Mathf.Exp(t); }
        private static float sqrt(float t) { return Mathf.Sqrt(t); }
        private static float ln(float t) { return Mathf.Log(t); }


        // get the (x, y) coordinates and the normal angle at the given location (measured in arcLen) of a spiral with the given parameters 
        // note: arcLen is inverted so that 0 refers to what we consider the start of the Nomai spiral
        private static Vector3 getDrawnSpiralPointAndNormal(float arcLen, float offsetX=0, float offsetY=0, float offsetAngle=0, bool mirror=false, float scale=20, float a=0.7f, float b=0.305f) {

            var startIndex = 2.5f;

            var startT = spiralStartT(startIndex, a, b);
            var startS = tToArcLen(startT, a, b);

            var width = spiralBoundingBoxWidth(startIndex, a, b);

            var startPoint = spiralPoint(startT, a, b);
            var startX = startPoint.x;
            var startY = startPoint.y;

            var t = tFromArcLen(startS-arcLen, a, b);
            var point = spiralPoint(t, a, b);
            var x = point.x; 
            var y = point.y;
            var ang = normalAngle(t, a, b);

            if (mirror) { 
                x = -(x-startX-width/2) +width/2+startX;
                ang = -ang+Mathf.PI;
            } 

            var retX = 0f;
            var retY = 0f;

    
    
            retX += scale*(x-startX);
            retY += scale*(y-startY);

            // rotate offsetAngle rads 
            var retX2=retX*cos(offsetAngle)
                     -retY*sin(offsetAngle);
            var retY2=retX*sin(offsetAngle)        
                     +retY*cos(offsetAngle);

            retX = retX2;
            retY = retY2;

            retX += offsetX;
            retY += offsetY;

            return new Vector3(retX, retY, ang+offsetAngle+Mathf.PI/2f);
        } 

        private static Vector2 spiralPoint(float t, float a, float b) {
            var r = a*exp(b*t);
            return new Vector2(r*cos(t), r*sin(t));
        }

        // the spiral's got two functions: x(t) and y(t)
        // so it's got two derrivatives (with respect to t) x'(t) and y'(t)
        private static Vector2 spiralDerivative(float t, float a, float b) { // derrivative with respect to t
            var r = a*exp(b*t);
            return new Vector2(
                -r*(sin(t)-b*cos(t)),
                 r*(b*sin(t)+cos(t))
            );
        }

        // returns the length of the spiral between t0 and t1
        private static float spiralArcLength(float t0, float t1, float a, float b) {
            return (a/b)*sqrt(b*b+1)*(exp(b*t1)-exp(b*t0));
        }

        // converts from a value of t to the equivalent value of s (the value of s that corresponds to the same point on the spiral as t)
        private static float tToArcLen(float t, float a, float b) {
            return spiralArcLength(0, t, a, b);
        }

        // reverse of above
        private static float tFromArcLen(float s, float a, float b) {
            return ln(
                    1+s/(
                        (a/b)*
                        sqrt(b*b+1)
                    )
                )/b;
        }

        // returns the value of t where the spiral starts
        // nomai spirals are reversed from the way the math is defined. in the math, they start at the center and spiral out, whereas Nomai writing spirals in
        // so this really returns the largest allowed value of t for this spiral
        // note: n is just an index. what it's an index of is irrelevant, but 2.5 is a good value
        private static float spiralStartT(float n, float a, float b) {
            return Mathf.Atan(b)+Mathf.PI*n;
        }

        // returns the angle of the spiral's normal at a given point
        private static float normalAngle(float t, float a, float b) {
            var d = spiralDerivative(t, a, b);
            var n = new Vector2(d.y, -d.x);
            var angle = Mathf.Atan2(n.y, n.x);

            return angle-Mathf.PI/2;
        }

        // startN refers to the same n as spiralStartT
        private static float spiralBoundingBoxWidth(float startN, float a, float b) {
            var topT = Mathf.Atan(-1/b)+Mathf.PI*startN;
            var startT = spiralStartT(startN, a, b);

            var topX = spiralPoint(topT, a, b).x;
            var startX = spiralPoint(startT, a, b).x;

            return Mathf.Abs(topX-startX);
        }
    }
}
