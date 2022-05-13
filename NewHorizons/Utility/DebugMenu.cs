using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NewHorizons.Utility
{
    [RequireComponent(typeof(DebugRaycaster))]
    [RequireComponent(typeof(DebugPropPlacer))]
    class DebugMenu : MonoBehaviour
    {
        GUIStyle _editorMenuStyle;
        Vector2 EditorMenuSize = new Vector2(600, 900);
        bool menuOpen = false;

        DebugPropPlacer _dpp;
        DebugRaycaster _drc;

        private void Awake()
        {  
            _dpp = this.GetRequiredComponent<DebugPropPlacer>();
            _drc = this.GetRequiredComponent<DebugRaycaster>();
        }

        private void Update()
        {
            if (!Main.Debug) return;

            if (Keyboard.current[Key.Escape].wasPressedThisFrame)
            {
                menuOpen = !menuOpen;
                if (menuOpen) InitMenu();
            }
        }
        
        private void OnGUI()
        {
            if (!menuOpen) return;
            if (!Main.Debug) return;

            Vector2 menuPosition = Vector2.zero; // new Vector2(Screen.width - EditorMenuSize.x - 10, 10);
            
            //TODO: add gui for stuff https://github.com/Bwc9876/OW-SaveEditor/blob/master/SaveEditor/SaveEditor.cs
            // https://docs.unity3d.com/ScriptReference/GUI.TextField.html

            GUILayout.BeginArea(new Rect(menuPosition.x, menuPosition.y, EditorMenuSize.x, EditorMenuSize.y), _editorMenuStyle);
            _dpp.currentObject = GUILayout.TextArea(_dpp.currentObject);

            // TODO: maintain list of objects here
            // TODO: field to provide name of mod to load configs from, plus button to load those into the PropPlaecr (make sure not to load more than once, once the button has been pushed, disable it)
            // TODO: add a warning that the button cannot be pushed more than once

            // TODO: put a text field here to print all the configs in
            // TODO: put a button here to save configs to file

            GUILayout.Label("*: Restart Required");
            GUILayout.Space(20);
            GUILayout.Label("*: Restart Required");
            GUILayout.EndArea();
        }

        private void InitMenu()
        {
            if (_editorMenuStyle != null) return;
            
            _dpp = this.GetRequiredComponent<DebugPropPlacer>();
            _drc = this.GetRequiredComponent<DebugRaycaster>();


            Texture2D bgTexture = MakeTexture((int)EditorMenuSize.x, (int)EditorMenuSize.y, Color.black);

            _editorMenuStyle = new GUIStyle
            {
                normal =
                {
                    background = bgTexture
                }
            };
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width*height];
 
            for(int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
 
            Texture2D newTexture = new Texture2D(width, height);
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return newTexture;
        }
    }
}
