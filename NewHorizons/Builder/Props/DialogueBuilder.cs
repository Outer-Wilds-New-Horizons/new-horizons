using NewHorizons.External;
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
        public static void Make(GameObject go, Sector sector, PropModule.DialogueInfo info)
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

            // XML STUFF GOES HERE

            var xml = System.IO.File.ReadAllText(Main.Instance.ModHelper.Manifest.ModFolderPath + info.xmlFile);
            var text = new TextAsset(xml);

            dialogueTree.SetTextXml(text);
            addTranslations(xml);


            conversationZone.transform.parent = sector.transform;
            conversationZone.transform.localPosition = info.position;
            conversationZone.SetActive(true);
        }

        private static void addTranslations(string xml)
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
