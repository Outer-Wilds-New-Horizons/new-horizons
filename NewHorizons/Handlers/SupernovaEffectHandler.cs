using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class SupernovaEffectHandler
    {
        private static List<NHSupernovaPlanetEffectController> _supernovaPlanetEffectControllers = new List<NHSupernovaPlanetEffectController>();
        private static List<StarEvolutionController> _starEvolutionControllers = new List<StarEvolutionController>();
        private static SunController _sunController;

        public static void RegisterStar(StarEvolutionController starEvolutionController)
        {
            _starEvolutionControllers.SafeAdd(starEvolutionController);
        }

        public static void UnregisterStar(StarEvolutionController starEvolutionController)
        {
            _starEvolutionControllers.Remove(starEvolutionController);
        }

        public static void RegisterSun(SunController sunController)
        {
            _sunController = sunController;
        }

        public static void UnregisterSun()
        {
            _sunController = null;
        }

        public static void RegisterPlanetEffect(NHSupernovaPlanetEffectController supernovaPlanetEffectController)
        {
            _supernovaPlanetEffectControllers.SafeAdd(supernovaPlanetEffectController);
        }

        public static void UnregisterPlanetEffect(NHSupernovaPlanetEffectController supernovaPlanetEffectController)
        {
            _supernovaPlanetEffectControllers.Remove(supernovaPlanetEffectController);
        }

        public static void GetNearestStarSupernova(NHSupernovaPlanetEffectController supernovaPlanetEffectController)
        {
            if (supernovaPlanetEffectController == null) return;

            StarEvolutionController nearestStarEvolutionController = null;
            float nearestDistanceSqr = float.MaxValue;

            // First we check for the nearest custom star
            foreach (var starEvolutionController in _starEvolutionControllers)
            {
                if (!IsStarActive(starEvolutionController)) continue;

                var distanceSqr = (supernovaPlanetEffectController.transform.position - starEvolutionController.transform.position).sqrMagnitude;

                if (distanceSqr < (starEvolutionController.supernovaSize * starEvolutionController.supernovaSize) && distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestStarEvolutionController = starEvolutionController;
                }
            }

            // We check if the stock sun is actually the closest star. Since it doesn't use StarEvolutionController we do this separately.
            if (_sunController != null && _sunController.gameObject.activeSelf)
            {
                var distanceSqr = (supernovaPlanetEffectController.transform.position - _sunController.transform.position).sqrMagnitude;
                if (distanceSqr < 2500000000f && distanceSqr < nearestDistanceSqr)
                {
                    supernovaPlanetEffectController.SunController = _sunController;
                    supernovaPlanetEffectController.shockLayerStartRadius = Mathf.Sqrt(distanceSqr) / 10f;
                    supernovaPlanetEffectController.shockLayerFullRadius = Mathf.Sqrt(distanceSqr);
                }
                else
                {
                    supernovaPlanetEffectController.StarEvolutionController = nearestStarEvolutionController;
                    if (nearestDistanceSqr != float.MaxValue)
                    {
                        supernovaPlanetEffectController.shockLayerStartRadius = Mathf.Sqrt(nearestDistanceSqr) / 10f;
                        supernovaPlanetEffectController.shockLayerFullRadius = Mathf.Sqrt(nearestDistanceSqr);
                    }
                }
            }
            else
            {
                supernovaPlanetEffectController.StarEvolutionController = nearestStarEvolutionController;
                if (nearestDistanceSqr != float.MaxValue)
                {
                    supernovaPlanetEffectController.shockLayerStartRadius = Mathf.Sqrt(nearestDistanceSqr) / 10f;
                    supernovaPlanetEffectController.shockLayerFullRadius = Mathf.Sqrt(nearestDistanceSqr);
                }
            }
        }

        private static bool IsStarActive(Component component)
        {
            return component != null && component.gameObject.activeSelf && component.gameObject.activeInHierarchy;
        }

        public static SunController GetSunController() => _sunController;

        public static bool InPointInsideAnySupernova(Vector3 position)
        {
            foreach (var starEvolutionController in _starEvolutionControllers)
            {
                if (!IsStarActive(starEvolutionController)) continue;

                var distanceSqr = (position - starEvolutionController.transform.position).sqrMagnitude;
                var size = starEvolutionController.GetSupernovaRadius();

                if (distanceSqr < (size * size)) return true;
            }

            if (IsStarActive(_sunController))
            {
                var distanceSqr = (position - _sunController.transform.position).sqrMagnitude;
                var size = _sunController.GetSupernovaRadius();

                if (distanceSqr < (size * size)) return true;
            }

            return false;
        }
    }
}
