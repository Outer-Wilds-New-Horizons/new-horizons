using NewHorizons.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class DialogueBuilder
    {
        public static void Make(GameObject go, Sector sector, PropModule.DialogueInfo info)
        {
            GameObject conversationZone = new GameObject("ConversationZone");
            conversationZone.SetActive(false);

            conversationZone.layer = LayerMask.NameToLayer("Interactible");

            var sphere = conversationZone.AddComponent<SphereCollider>();
            sphere.radius = info.radius;
            sphere.isTrigger = true;

            conversationZone.AddComponent<OWCollider>();

            var dialogueTree = conversationZone.AddComponent<CharacterDialogueTree>();

            // XML STUFF GOES HERE




            conversationZone.transform.parent = sector.transform;
            conversationZone.transform.localPosition = Vector3.zero;
            conversationZone.SetActive(true);
        }
    }
}
