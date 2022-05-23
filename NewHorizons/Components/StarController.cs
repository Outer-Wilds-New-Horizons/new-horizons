using UnityEngine;

namespace NewHorizons.Components
{
    public class StarController : MonoBehaviour
    {
        public Light Light;
        public Light AmbientLight;
        public FaceActiveCamera FaceActiveCamera;
        public CSMTextureCacher CSMTextureCacher;
        public ProxyShadowLight ProxyShadowLight;
        public float Intensity;
        public Color SunColor;

        public void Awake()
        {
            Disable();
        }

        public void Disable()
        {
            if (FaceActiveCamera != null) FaceActiveCamera.enabled = false;
            if (CSMTextureCacher != null) CSMTextureCacher.enabled = false;
            if (ProxyShadowLight != null) ProxyShadowLight.enabled = false;
        }

        public void Enable()
        {
            if (FaceActiveCamera != null) FaceActiveCamera.enabled = true;
            if (CSMTextureCacher != null) CSMTextureCacher.enabled = true;
            if (ProxyShadowLight != null) ProxyShadowLight.enabled = true;
        }
    }
}