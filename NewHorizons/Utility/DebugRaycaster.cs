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

        public string PropToPlace = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District1/Props_HangingCity_District1/OtherComponentsGroup/Props_HangingCity_Building_10/Prefab_NOM_VaseThin";

        private void Awake()
        {
            _rb = this.GetRequiredComponent<OWRigidbody>();
        }

        //private void OnGui()
        //{
        //    //TODO: add gui for stuff https://github.com/Bwc9876/OW-SaveEditor/blob/master/SaveEditor/SaveEditor.cs
        //    // https://docs.unity3d.com/ScriptReference/GUI.TextField.html
        //    GUILayout.BeginArea(new Rect(menuPosition.x, menuPosition.y, EditorMenuSize.x, EditorMenuSize.y), _editorMenuStyle);
        //}

        private void Update()
        {
            if (!Main.Debug) return;
            if (Keyboard.current == null) return;

            if (Keyboard.current[Key.P].wasReleasedThisFrame)
            {
                PrintRaycast();
            }

            if (Keyboard.current[Key.L].wasReleasedThisFrame)
            {
                DebugPropPlacer.currentObject = PropToPlace;
                PlaceObject();
            }

            if (Keyboard.current[Key.Semicolon].wasReleasedThisFrame)
            {
                DebugPropPlacer.PrintConfig();
            }
            
            if (Keyboard.current[Key.Minus].wasReleasedThisFrame)
            {
                DebugPropPlacer.DeleteLast();
            }
            
            if (Keyboard.current[Key.Equals].wasReleasedThisFrame)
            {
                DebugPropPlacer.UndoDelete();
            }
        }

        internal void PlaceObject()
        {
            DebugRaycastData data = Raycast();
            DebugPropPlacer.PlaceObject(data, this.gameObject.transform.position);
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
