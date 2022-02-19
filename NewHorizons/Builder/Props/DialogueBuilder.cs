using NewHorizons.External;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class DialogueBuilder
    {
        public static void Make(GameObject go, Sector sector, PropModule.DialogueInfo info, IModHelper mod)
        {
            if (info.blockAfterPersistentCondition != null && PlayerData._currentGameSave.GetPersistentCondition(info.blockAfterPersistentCondition)) return;

            var dialogue = MakeConversationZone(go, sector, info, mod);
            if (info.remoteTriggerPosition != null) MakeRemoteDialogueTrigger(go, sector, info, dialogue);
        }

        public static void MakeRemoteDialogueTrigger(GameObject go, Sector sector, PropModule.DialogueInfo info, CharacterDialogueTree dialogue)
        {
            GameObject conversationTrigger = new GameObject("ConversationTrigger");
            conversationTrigger.SetActive(false);

            var remoteDialogueTrigger = conversationTrigger.AddComponent<RemoteDialogueTrigger>();
            var boxCollider = conversationTrigger.AddComponent<BoxCollider>();
            conversationTrigger.AddComponent<OWCollider>();

            remoteDialogueTrigger._listDialogues = new RemoteDialogueTrigger.RemoteDialogueCondition[]
            {
                new RemoteDialogueTrigger.RemoteDialogueCondition()
                {
                    priority = 1,
                    dialogue = dialogue,
                    prereqConditionType = RemoteDialogueTrigger.MultiConditionType.AND,
                    prereqConditions = new string[]{ },
                    onTriggerEnterConditions = new string[]{ }
                }  
            };
            remoteDialogueTrigger._activatedDialogues = new bool[1];
            remoteDialogueTrigger._deactivateTriggerPostConversation = true;

            boxCollider.size = Vector3.one * info.radius / 2f;

            conversationTrigger.transform.parent = sector?.transform ?? go.transform;
            conversationTrigger.transform.localPosition = info.remoteTriggerPosition;
            conversationTrigger.SetActive(true);
        }

        public static CharacterDialogueTree MakeConversationZone(GameObject go, Sector sector, PropModule.DialogueInfo info, IModHelper mod)
        {
            GameObject conversationZone = new GameObject("ConversationZone");
            conversationZone.SetActive(false);

            conversationZone.layer = LayerMask.NameToLayer("Interactible");

            var sphere = conversationZone.AddComponent<SphereCollider>();
            sphere.radius = info.radius;
            sphere.isTrigger = true;

            conversationZone.AddComponent<OWCollider>();
            conversationZone.AddComponent<InteractReceiver>();

            var dialogueTree = conversationZone.AddComponent<CharacterDialogueTree>();

            var xml = System.IO.File.ReadAllText(mod.Manifest.ModFolderPath + info.xmlFile);
            var text = new TextAsset(xml);

            dialogueTree.SetTextXml(text);
            AddTranslation(xml);

            conversationZone.transform.parent = sector?.transform ?? go.transform;
            conversationZone.transform.localPosition = info.position;
            conversationZone.SetActive(true);

            return dialogueTree;
        }

        private static void AddTranslation(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("DialogueTree");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("DialogueNode");
            string NameField = xmlNode.SelectSingleNode("NameField").InnerText;
            var translationTable = TextTranslation.Get().m_table.theTable;
            translationTable[NameField] = NameField;


            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                var name = xmlNode2.SelectSingleNode("Name").InnerText;

                XmlNodeList xmlText = xmlNode2.SelectNodes("Dialogue/Page");
                foreach (object Page in xmlText)
                {

                    XmlNode pageData = (XmlNode)Page;
                    translationTable[name + pageData.InnerText] = pageData.InnerText;

                }

                xmlText = xmlNode2.SelectNodes("DialogueOptionsList/DialogueOption/Text");

                foreach (object Page in xmlText)
                {
                    XmlNode pageData = (XmlNode)Page;
                    translationTable[NameField + name + pageData.InnerText] = pageData.InnerText;
                }
            }
        }
    }
}
