using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility.DebugUtilities
{
    class DebugArrow : MonoBehaviour
    {
        public Transform target;

        void Start()
        {
            // make the mesh in code so we don't need an assetbundle or anything
            /*            E
             *         (0, .5)
             *            /\
             *           /  \
             *  C (-.5,0) || (.5,0) D
             *            ||
             *   (-.1,-.5)  (.1,-.5)
             *       A          B
             */

            Vector3[] topVerts = new Vector3[]
            {
                new Vector3(-0.1f, -0.5f, 0.1f), // A
                new Vector3( 0.1f, -0.5f, 0.1f), // B
                new Vector3(-0.5f,    0f, 0.1f), // C
                new Vector3( 0.5f,    0f, 0.1f), // D
                new Vector3(   0f,  0.5f, 0.1f), // E
            };
        }
    }
}
