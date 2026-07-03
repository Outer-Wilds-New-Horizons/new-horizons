using NewHorizons.Utility.OWML;
using UnityEngine;

namespace NewHorizons.Components
{
    public class ConditionalNomaiComputerToggle : MonoBehaviour
    {
        public string dialogueCondition;
        public bool turnOnWithCondition;

        private NomaiComputer nomaiComputer;
        private NomaiVesselComputer nomaiVesselComputer;

        public static void SetUp(GameObject computerObject, string condition, bool turnOnWithCondition)
        {
            var conditionalNomaiComputerToggleGO = new GameObject($"{computerObject.name}_{condition}");
            var component = conditionalNomaiComputerToggleGO.AddComponent<ConditionalNomaiComputerToggle>();
            component.transform.parent = computerObject.transform.parent;
            component.dialogueCondition = condition;
            component.turnOnWithCondition = turnOnWithCondition;
            component.nomaiComputer = computerObject.GetComponent<NomaiComputer>();
            component.nomaiVesselComputer = computerObject.GetComponent<NomaiVesselComputer>();
        }

        public void Awake()
        {
            GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
        }

        public void OnDestroy()
        {
            GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
        }

        public void OnDialogueConditionChanged(string condition, bool state)
        {
            if (condition != dialogueCondition) return;

            if (nomaiComputer != null)
            {
                if (state == turnOnWithCondition)
                {
                    nomaiComputer.DisplayAllEntries();
                }
                else
                {
                    nomaiComputer.ClearAllEntries();
                }
            }

            if (nomaiVesselComputer != null)
            {
                if (state == turnOnWithCondition)
                {
                    nomaiVesselComputer.TurnOn();
                }
                else
                {
                    nomaiVesselComputer.TurnOff();
                }
            }
        }
    }
}
