using NewHorizons.Components.SizeControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewHorizons.Components
{
    public class StellarRemnantController : MonoBehaviour
    {
        private StarEvolutionController _starEvolutionController;

        public void SetStarEvolutionController(StarEvolutionController controller) => _starEvolutionController = controller;
    }
}
