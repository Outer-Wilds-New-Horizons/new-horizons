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

        private GameObject blackHole;
        private GameObject whiteHole;

        private void Awake()
        {
            _rb = this.GetRequiredComponent<OWRigidbody>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[Key.P].wasReleasedThisFrame)
            {
                // Raycast
                _rb.DisableCollisionDetection();
                int layerMask = OWLayerMask.physicalMask;
                var origin = Locator.GetActiveCamera().transform.position;
                var direction = Locator.GetActiveCamera().transform.TransformDirection(Vector3.forward);
                if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, 100f, layerMask))
                {
                    var pos = hitInfo.transform.InverseTransformPoint(hitInfo.point);
                    Logger.Log($"Raycast hit {{\"x\": {pos.x}, \"y\": {pos.y}, \"z\": {pos.z}}} on [{hitInfo.transform.gameObject.name}]");
                }
                _rb.EnableCollisionDetection();
            }

            /*
            // Portal Gun:
            if (Keyboard.current == null) return;
            var fireBlackHole = Keyboard.current[Key.B].wasReleasedThisFrame;
            var fireWhiteHole = Keyboard.current[Key.N].wasReleasedThisFrame;
            if (fireBlackHole || fireWhiteHole)
            {
                // Raycast
                _rb.DisableCollisionDetection();
                int layerMask = OWLayerMask.physicalMask;
                var origin = Locator.GetActiveCamera().transform.position;
                var direction = Locator.GetActiveCamera().transform.TransformDirection(Vector3.forward);
                if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, Mathf.Infinity, OWLayerMask.physicalMask))
                {
                    var pos = hitInfo.transform.InverseTransformPoint(hitInfo.point + hitInfo.normal);
                    var hitBody = hitInfo.transform.gameObject;
                    var sector = hitBody.GetComponent<AstroObject>()?.GetRootSector();

                    if (hitBody == null || sector == null) return;
                    Logger.Log($"{hitBody}");
                    if (fireBlackHole)
                    {
                        if (blackHole != null) GameObject.Destroy(blackHole);
                        blackHole = SingularityBuilder.MakeBlackHole(hitBody, sector, pos, 2, false, null, false);
                        Logger.Log("Make black hole");
                    }
                    else
                    {
                        if (whiteHole != null) GameObject.Destroy(whiteHole);
                        whiteHole = SingularityBuilder.MakeWhiteHole(hitBody, sector, hitBody.GetAttachedOWRigidbody(), pos, 2, false);
                        Logger.Log("Make white hole");
                    }

                    if(blackHole && whiteHole)
                    {
                        SingularityBuilder.PairSingularities(blackHole, whiteHole);
                    }
                }
                _rb.EnableCollisionDetection();
            }
            */
        }
    }
}
