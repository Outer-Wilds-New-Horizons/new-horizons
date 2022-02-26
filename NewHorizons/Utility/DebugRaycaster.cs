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
                    var o = hitInfo.transform.gameObject;
                    Logger.Log($"Raycast hit {{\"x\": {pos.x}, \"y\": {pos.y}, \"z\": {pos.z}}} on [{o.name}] at [{SearchUtilities.GetPath(o.transform)}]");
                }
                _rb.EnableCollisionDetection();
            }
        }
    }
}
