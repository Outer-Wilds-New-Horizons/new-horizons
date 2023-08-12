namespace NewHorizons.Components.EyeOfTheUniverse
{
    /*
    public class EyeSunLightParamUpdater : MonoBehaviour
    {
        public static readonly int SunPosition = Shader.PropertyToID("_SunPosition");
        public static readonly int OWSunPositionRange = Shader.PropertyToID("_OWSunPositionRange");
        public static readonly int OWSunColorIntensity = Shader.PropertyToID("_OWSunColorIntensity");
        public static readonly int SunIntensity = Shader.PropertyToID("_SunIntensity");
        public static readonly Vector3 position = new Vector3(0, 1, -10) * 1000000;
        public static readonly Color color = new Color(0.3569f, 0.7843f, 1, 0.2f);
        public static readonly float radius = 100;
        public static readonly float range = 100000;
        public static readonly float intensity = 0.2f;

        public void LateUpdate()
        {
            var currentIntensity = TimeLoop.GetSecondsRemaining() > -10 ? intensity : 0;
            
            // Changing these global shader values breaks the CosmicInflationController orb shader
            // So DON'T use this component (sad)
            Shader.SetGlobalVector(SunPosition, new Vector4(position.x, position.y, position.z, radius));
            Shader.SetGlobalVector(OWSunPositionRange, new Vector4(position.x, position.y, position.z, range * range));
            Shader.SetGlobalVector(OWSunColorIntensity, new Vector4(color.r, color.g, color.b, currentIntensity));
            
            foreach (var (_, material) in AtmosphereBuilder.Skys)
            {
                material.SetFloat(SunIntensity, currentIntensity);
            }
        }
    }
    */
}
