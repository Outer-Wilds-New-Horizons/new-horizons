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
        public static GameObject spiralMeshHolder;


        class SpiralTextArc
        {
            public GameObject g;
            public SpiralMesh m;

            public SpiralTextArc()
            {
                g = new GameObject();
                g.transform.parent = spiralMeshHolder.transform;
                g.transform.localPosition = Vector3.zero;
                g.transform.localEulerAngles = Vector3.zero;
                
                m = new SpiralMesh();
                m.updateMesh();
                
                g.AddComponent<MeshFilter>().sharedMesh = m.mesh;
                g.AddComponent<MeshRenderer>().sharedMaterial = _arcPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial;

                // (float arcLen, float offsetX, float offsetY, float offsetAngle, bool mirror, float scale, float a, float b, float startS)
                GameObject p = AddDebugShape.AddSphere(g, 0.05f, Color.green);
                Vector3 start = m.getDrawnSpiralPointAndNormal(m.endS);
                p.transform.localPosition = new Vector3(start.x, 0, start.y);
            }

            public SpiralTextArc MakeChild()
            {
                Logger.Log("MAKING CHILD");
                var s = new SpiralTextArc();
                s.m.startSOnParent = UnityEngine.Random.Range(50, 250);
                m.addChild(s.m);
                return s;
            }
        }



        private void Awake()
        {
            _rc = this.GetComponent<DebugRaycaster>();
        }

        void Update()
        {
            if (Keyboard.current[Key.G].wasReleasedThisFrame) // TODO: REMOVE THIS WHOLE IF STATEMENT, it's just for debug testing
            {



                DebugRaycastData data = _rc.Raycast();
                
                var sectorObject = data.hitBodyGameObject.GetComponentInChildren<Sector>()?.gameObject;
                if (sectorObject == null) sectorObject = data.hitBodyGameObject.GetComponentInParent<Sector>()?.gameObject;

                var gameObject = new GameObject("spiral holder");
                spiralMeshHolder = gameObject;
                gameObject.transform.parent = sectorObject.transform;
                gameObject.transform.localPosition = data.pos;
                DebugPropPlacer.SetGameObjectRotation(gameObject, data, this.gameObject.transform.position);
                
                
                
                var rootArc = new SpiralTextArc();
                spiralMesh = rootArc.m;

                rootArc.MakeChild();
                spiralMesh.updateChildren();


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
