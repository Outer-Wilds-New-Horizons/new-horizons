using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.OrbitalPhysics
{
    public class BinaryFocalPoint : MonoBehaviour
    {
        public string PrimaryName = null;
        public string SecondaryName = null;

        public AstroObject Primary = null;
        public AstroObject Secondary = null;

        public List<AstroObject> Planets { get; private set; } = new List<AstroObject>(); 
    }
}
