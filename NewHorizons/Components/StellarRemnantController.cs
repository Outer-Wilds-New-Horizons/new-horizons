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

        private StellarRemnantController _proxy;

        public void SetProxy(StellarRemnantController proxy) => _proxy = proxy;

        public void SetStarEvolutionController(StarEvolutionController controller) => _starEvolutionController = controller;
    }
}
