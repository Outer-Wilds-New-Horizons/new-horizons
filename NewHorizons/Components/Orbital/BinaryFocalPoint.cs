using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.Orbital
{
    public class BinaryFocalPoint : MonoBehaviour
    {
        public string PrimaryName = null;
        public string SecondaryName = null;

        public AstroObject Primary = null;
        public AstroObject Secondary = null;

        public GameObject FakeMassBody = null;

        public List<AstroObject> Planets { get; private set; } = new List<AstroObject>(); 

        void Awake()
        {
            FakeMassBody.SetActive(true);   
        }
    }
}
