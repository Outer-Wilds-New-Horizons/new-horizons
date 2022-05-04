using NewHorizons;
using NewHorizons.Builder.Body;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (Main.Debug && Keyboard.current != null && Keyboard.current[Key.P].wasReleasedThisFrame)
            {
                // Raycast
                _rb.DisableCollisionDetection();
                int layerMask = OWLayerMask.physicalMask;
                var origin = Locator.GetActiveCamera().transform.position;
                var direction = Locator.GetActiveCamera().transform.TransformDirection(Vector3.forward);
                if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, 100f, layerMask))
                {
                    var pos = hitInfo.transform.InverseTransformPoint(hitInfo.point);
                    var norm = hitInfo.transform.InverseTransformDirection(hitInfo.normal);
                    var o = hitInfo.transform.gameObject;

                    var posText = $"{{\"x\": {pos.x}, \"y\": {pos.y}, \"z\": {pos.z}}}";
                    var normText = $"{{\"x\": {norm.x}, \"y\": {norm.y}, \"z\": {norm.z}}}";

                    if(_surfaceSphere != null) GameObject.Destroy(_surfaceSphere);
                    if(_normalSphere1 != null) GameObject.Destroy(_normalSphere1);
                    if(_normalSphere2 != null) GameObject.Destroy(_normalSphere2);

                    _surfaceSphere = AddDebugShape.AddSphere(hitInfo.transform.gameObject, 0.1f, Color.green);
                    _normalSphere1 = AddDebugShape.AddSphere(hitInfo.transform.gameObject, 0.01f, Color.red);
                    _normalSphere2 = AddDebugShape.AddSphere(hitInfo.transform.gameObject, 0.01f, Color.red);

                    _surfaceSphere.transform.localPosition = pos;
                    _normalSphere1.transform.localPosition = pos + norm * 0.5f;
                    _normalSphere2.transform.localPosition = pos + norm;

                    Logger.Log($"Raycast hit \"position\": {posText}, \"normal\": {normText} on [{o.name}] at [{SearchUtilities.GetPath(o.transform)}]");
                }
                _rb.EnableCollisionDetection();
            }
        }
    }
}
