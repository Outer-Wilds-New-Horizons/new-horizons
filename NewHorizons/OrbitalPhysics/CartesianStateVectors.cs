using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.OrbitalPhysics
{
    public class CartesianStateVectors
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public CartesianStateVectors()
        {
            Position = Vector3.zero;
            Velocity = Vector3.zero;
        }

        public CartesianStateVectors(Vector3 pos, Vector3 vel)
        {
            Position = pos;
            Velocity = vel;
        }
    }
}
