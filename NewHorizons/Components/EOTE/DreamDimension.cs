using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components.EOTE
{
    public class DreamDimension : MonoBehaviour
    {
        private bool initialized;
        private bool active;
        private List<GameObject> toggledObjects = [];

        public void Initialize()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.name == "FieldDetector") continue;
                toggledObjects.Add(child.gameObject);
            }

            initialized = true;
            UpdateState();
        }

        public void SetActive(bool active)
        {
            if (this.active != active)
            {
                this.active = active;
                UpdateState();
            }
        }

        void Awake()
        {
            GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void OnDestroy()
        {
            GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamWorld);
            GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamWorld);
        }

        void UpdateState()
        {
            foreach (var obj in toggledObjects) obj.SetActive(active);
        }

        void OnEnterDreamWorld()
        {
            SetActive(true);
        }

        void OnExitDreamWorld()
        {
            SetActive(false);
        }
    }
}
