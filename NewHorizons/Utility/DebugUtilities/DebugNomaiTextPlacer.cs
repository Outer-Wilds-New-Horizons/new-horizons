using NewHorizons.Builder.Props;
using NewHorizons.Handlers;
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

        private ScreenPrompt _placePrompt;
        
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

                m = new SpiralMesh(NomaiTextBuilder.adultSpiralProfile);
                m.Randomize();
                m.updateMesh();

                g.AddComponent<MeshFilter>().sharedMesh = m.mesh;
                g.AddComponent<MeshRenderer>().sharedMaterial = GetArcPrefabs()[0].GetComponent<MeshRenderer>().sharedMaterial;

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

            _placePrompt = new ScreenPrompt(TranslationHandler.GetTranslation("DEBUG_PLACE_TEXT", TranslationHandler.TextType.UI) + " <CMD>", ImageUtilities.GetButtonSprite(KeyCode.G));
            Locator.GetPromptManager().AddScreenPrompt(_placePrompt, PromptPosition.UpperRight, false);
        }

        private void OnDestroy()
        {
            Locator.GetPromptManager()?.RemoveScreenPrompt(_placePrompt, PromptPosition.UpperRight);
        }

        private void Update()
        {
            UpdatePromptVisibility();
            if (!Main.Debug) return;
            if (Keyboard.current == null) return;

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
                rootArc.MakeChild();
                spiralMesh = rootArc.m.children[0];
                spiralMesh.updateChildren();

                // // add a sphere at each skeleton point
                //spiralMesh.skeleton.ForEach(point => {
                //    var go = AddDebugShape.AddSphere(gameObject, 0.02f, Color.green);
                //    go.transform.localPosition = new Vector3(point.x, 0, point.y);
                //});
            }

            if (spiralMesh != null)
            {
                if (Keyboard.current[Key.Equals].wasReleasedThisFrame) { spiralMesh.a += 0.05f; spiralMesh.updateChildren(); }
                if (Keyboard.current[Key.Minus].wasReleasedThisFrame) { spiralMesh.a -= 0.05f; spiralMesh.updateChildren(); }
            }

            if (!active) return;

            if (Keyboard.current[Key.G].wasReleasedThisFrame)
            {
                DebugRaycastData data = _rc.Raycast();
                if (onRaycast != null) onRaycast.Invoke(data);
            }
        }

        public void UpdatePromptVisibility()
        {
            _placePrompt.SetVisibility(!OWTime.IsPaused() && Main.Debug && active);
        }
    }
}
