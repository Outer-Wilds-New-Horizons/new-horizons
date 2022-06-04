using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static NewHorizons.Builder.Props.NomaiTextBuilder;

namespace NewHorizons.Utility.DebugUtilities
{
    class DebugNomaiTextPlacer : MonoBehaviour
    {
        public static bool active;
        public Action<DebugRaycastData> onRaycast;

        private DebugRaycaster _rc;
        public SpiralMesh spiralMesh;

        private void Awake()
        {
            _rc = this.GetComponent<DebugRaycaster>();
        }

        void Update()
        {
            if (Keyboard.current[Key.G].wasReleasedThisFrame) // TODO: REMOVE THIS WHOLE IF STATEMENT, it's just for debug testing
            {
                spiralMesh = new SpiralMesh();
                spiralMesh.updateChildren();
                var gameObject = new GameObject("dynamic spiral");
                gameObject.AddComponent<MeshFilter>().sharedMesh = spiralMesh.mesh;
                gameObject.AddComponent<MeshRenderer>().sharedMaterial = _arcPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial;

                DebugRaycastData data = _rc.Raycast();
                
                var sectorObject = data.hitBodyGameObject.GetComponentInChildren<Sector>()?.gameObject;
                if (sectorObject == null) sectorObject = data.hitBodyGameObject.GetComponentInParent<Sector>()?.gameObject;

                gameObject.transform.parent = sectorObject.transform;
                gameObject.transform.localPosition = data.pos;
                DebugPropPlacer.SetGameObjectRotation(gameObject, data, this.gameObject.transform.position);

                // // add a sphere at each skeleton point
                //spiralMesh.skeleton.ForEach(point => {
                //    var go = AddDebugShape.AddSphere(gameObject, 0.02f, Color.green);
                //    go.transform.localPosition = new Vector3(point.x, 0, point.y);
                //});
            }

            if (Keyboard.current[Key.Equals].wasReleasedThisFrame) { spiralMesh.a += 0.05f; spiralMesh.updateChildren(); }
            if (Keyboard.current[Key.Minus].wasReleasedThisFrame) { spiralMesh.a -= 0.05f; spiralMesh.updateChildren(); }

            if (!Main.Debug) return;
            if (!active) return;

            if (Keyboard.current[Key.G].wasReleasedThisFrame)
            {
                DebugRaycastData data = _rc.Raycast();
                if (onRaycast != null) onRaycast.Invoke(data);
            }
        }
    }
}
