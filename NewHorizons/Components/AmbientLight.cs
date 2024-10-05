using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    public class AmbientLight : MonoBehaviour
    {
        Light light;
        float cachedIntensity;

        void Awake()
        {
            light = GetComponent<Light>();
            cachedIntensity = light.intensity;
        }

        void Update()
        {
            var targetIntensity = PlayerState.InDreamWorld() ? 0f : cachedIntensity;
            light.intensity = Mathf.MoveTowards(light.intensity, targetIntensity, Time.deltaTime * 5f);
        }
    }
}
