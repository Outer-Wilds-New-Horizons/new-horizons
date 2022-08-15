using NewHorizons.Components;
using NewHorizons.Components.SizeControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.Handlers
{
    public class SupernovaEffectHandler
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
            float nearestDistance = float.MaxValue;
            foreach (StarEvolutionController starEvolutionController in _starEvolutionControllers)
            {
                if (starEvolutionController == null) continue;
                if (starEvolutionController._disabled || !(starEvolutionController.gameObject.activeSelf && starEvolutionController.gameObject.activeInHierarchy)) continue;
                float distance = (supernovaPlanetEffectController.transform.position - starEvolutionController.transform.position).sqrMagnitude;
                if (distance < (starEvolutionController.supernovaSize * starEvolutionController.supernovaSize) && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestStarEvolutionController = starEvolutionController;
                }
            }

            if (_sunController != null && _sunController.gameObject.activeSelf)
            {
                float distance = (supernovaPlanetEffectController.transform.position - _sunController.transform.position).sqrMagnitude;
                if (distance < 2500000000f && distance < nearestDistance)
                {
                    supernovaPlanetEffectController._sunController = _sunController;
                    supernovaPlanetEffectController._starEvolutionController = null;
                }
                else
                {
                    supernovaPlanetEffectController._sunController = null;
                    supernovaPlanetEffectController._starEvolutionController = nearestStarEvolutionController;
                }
            }
            else
            {
                supernovaPlanetEffectController._sunController = null;
                supernovaPlanetEffectController._starEvolutionController = nearestStarEvolutionController;
            }
        }

        public static SunController GetSunController() => _sunController;

        public static bool InPointInsideSunSupernova(NHSupernovaPlanetEffectController supernovaPlanetEffectController) => _sunController != null && (supernovaPlanetEffectController.transform.position - _sunController.transform.position).sqrMagnitude < 2500000000f;//(50000f*50000f);
    }
}
