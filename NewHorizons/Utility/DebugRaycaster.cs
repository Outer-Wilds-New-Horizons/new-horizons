using UnityEngine;
using UnityEngine.InputSystem;
namespace NewHorizons.Utility
{
    [RequireComponent(typeof(OWRigidbody))]
    public class DebugRaycaster : MonoBehaviour
    {
        private OWRigidbody _rb;
        private GameObject _surfaceSphere;
        private GameObject _normalSphere1;
        private GameObject _normalSphere2;

        private void Awake()
        {
            _rb = this.GetRequiredComponent<OWRigidbody>();
        }
        

        private void Update()
        {
            if (!Main.Debug) return;
            if (Keyboard.current == null) return;

            if (Keyboard.current[Key.P].wasReleasedThisFrame)
            {
                PrintRaycast();
            }
        }

       

        internal void PrintRaycast()
        {
            DebugRaycastData data = Raycast();
            var posText = $"{{\"x\": {data.pos.x}, \"y\": {data.pos.y}, \"z\": {data.pos.z}}}";
            var normText = $"{{\"x\": {data.norm.x}, \"y\": {data.norm.y}, \"z\": {data.norm.z}}}";

            if(_surfaceSphere != null) GameObject.Destroy(_surfaceSphere);
            if(_normalSphere1 != null) GameObject.Destroy(_normalSphere1);
            if(_normalSphere2 != null) GameObject.Destroy(_normalSphere2);

            _surfaceSphere = AddDebugShape.AddSphere(data.hitObject, 0.1f, Color.green);
            _normalSphere1 = AddDebugShape.AddSphere(data.hitObject, 0.01f, Color.red);
            _normalSphere2 = AddDebugShape.AddSphere(data.hitObject, 0.01f, Color.red);

            _surfaceSphere.transform.localPosition = data.pos;
            _normalSphere1.transform.localPosition = data.pos + data.norm * 0.5f;
            _normalSphere2.transform.localPosition = data.pos + data.norm;

            Logger.Log($"Raycast hit \"position\": {posText}, \"normal\": {normText} on [{data.bodyName}] at [{data.bodyPath}]");
        }
        internal DebugRaycastData Raycast()
        {
            DebugRaycastData data = new DebugRaycastData();

            _rb.DisableCollisionDetection();
            int layerMask = OWLayerMask.physicalMask;
            var origin = Locator.GetActiveCamera().transform.position;
            var direction = Locator.GetActiveCamera().transform.TransformDirection(Vector3.forward);
            
            data.hit = Physics.Raycast(origin, direction, out RaycastHit hitInfo, 100f, layerMask);
            if (data.hit)
            {
                data.pos = hitInfo.transform.InverseTransformPoint(hitInfo.point);
                data.norm = hitInfo.transform.InverseTransformDirection(hitInfo.normal);
                var o = hitInfo.transform.gameObject;

                data.bodyName = o.name;
                data.bodyPath = SearchUtilities.GetPath(o.transform);
                data.hitObject = hitInfo.transform.gameObject;
            }
            _rb.EnableCollisionDetection();

            return data;
        }
    }
}
