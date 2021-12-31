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
            if (Keyboard.current != null && Keyboard.current[Key.P].wasReleasedThisFrame)
            {
                /*
                var soundSources = GameObject.FindObjectsOfType<AudioSource>();
                foreach(var s in soundSources)
                {
                    if (s.isPlaying)
                    {
                        Logger.Log($"{s.name}, {s.gameObject.name}");
                        Logger.LogPath(s.gameObject);
                        s.loop = false;
                        s.Stop();
                    }
                }
                */

                // Raycast
                _rb.DisableCollisionDetection();
                int layerMask = OWLayerMask.physicalMask;
                var origin = Locator.GetActiveCamera().transform.position;
                var direction = Locator.GetActiveCamera().transform.TransformDirection(Vector3.forward);
                if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, 100f, layerMask))
                {
                    Logger.Log($"Raycast hit [{hitInfo.transform.InverseTransformPoint(hitInfo.point)}] on [{hitInfo.transform.gameObject.name}]");
                }
                _rb.EnableCollisionDetection();
            }
        }
    }
}
