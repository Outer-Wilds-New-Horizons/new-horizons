using NewHorizons.External;
using NewHorizons.Handlers;
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
    public static class NomaiTextBuilder
    {
        //TODO Scrolls
        public static void Make(GameObject go, Sector sector, PropModule.NomaiTextInfo info, IModBehaviour mod)
        {

            GameObject conversationZone = new GameObject("ConversationZone");
            conversationZone.SetActive(false);
            conversationZone.name = "NomaiText";


            var box = conversationZone.AddComponent<BoxCollider>();
            //TODO better bounds
            box.size = new Vector3(3f, 6f, 1f);
            box.isTrigger = true;

            conversationZone.AddComponent<OWCollider>();
            var NomaiWall = conversationZone.AddComponent<NomaiWallText>();


            var xml = System.IO.File.ReadAllText(mod.ModHelper.Manifest.ModFolderPath + info.xmlFile);
            var text = new TextAsset(xml);

            NomaiWall._dictNomaiTextData = new Dictionary<int, NomaiText.NomaiTextData>(ComparerLibrary.intEqComparer);
            NomaiWall._nomaiTextAsset = text;

            conversationZone.transform.parent = sector?.transform ?? go.transform;
            conversationZone.transform.localPosition = info.position;

            buildArcs(xml, NomaiWall, conversationZone);
            NomaiWall.SetTextAsset(text);


            conversationZone.SetActive(true);

        }

        public static void buildArcs(string xml, NomaiWallText nomai, GameObject conversationZone)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode rootNode = xmlDocument.SelectSingleNode("NomaiObject");

            
            foreach (object obj in rootNode.SelectNodes("TextBlock"))
            {
                XmlNode xmlNode = (XmlNode)obj;
                int num = -1;
                XmlNode xmlNode2 = xmlNode.SelectSingleNode("ID");
                XmlNode textNode = xmlNode.SelectSingleNode("Text");
                XmlNode xmlNode3 = xmlNode.SelectSingleNode("ParentID");
                int parentID = -1;
                if (xmlNode2 != null && !int.TryParse(xmlNode2.InnerText, out num))
                {
                    num = -1;
                }
                if (xmlNode3 != null && !int.TryParse(xmlNode3.InnerText, out parentID))
                {
                    parentID = -1;
                }
                //TODO translation table
                //TODO verify shiplogs
                NomaiText.NomaiTextData value = new NomaiText.NomaiTextData(num, parentID, textNode, false, NomaiText.Location.UNSPECIFIED);
                nomai._dictNomaiTextData.Add(num, value);

                GameObject Arc = new GameObject($"Arc {num}");
                Arc.SetActive(false);
                var textLine = Arc.AddComponent<NomaiTextLine>();
                textLine.SetEntryID(num);

                Arc.transform.parent = conversationZone.transform;

                //TODO Modify so spirals are connected
                Vector3 center = new Vector3(Mathf.Sin((num * 90 * Mathf.PI) / 180), Mathf.Cos((num * 90 * Mathf.PI) / 180), 0);
                Arc.transform.localPosition = center;
                //TODO fix centering
                textLine._center = center;
                textLine._radius = 1;

                textLine.SetPoints(makePoints(5, num));

                //TODO pull assets directly instead of ripping off existing text
                GameObject copiedArc = GameObject.Find("TimberHearth_Body/Sector_TH/Sector_Village/Sector_Observatory/Interactables_Observatory/NomaiEyeExhibit/NomaiEyePivot/Arc_TH_Museum_EyeSymbol/Arc 1");
                
                MeshFilter meshFilter = Arc.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = copiedArc.GetComponent<MeshFilter>().sharedMesh;


                MeshRenderer meshRenderer = Arc.AddComponent<MeshRenderer>();
                meshRenderer.materials = copiedArc.GetComponent<MeshRenderer>().materials;
                Arc.SetActive(true);
            }

        }


        private static Vector3[] makePoints(int length, int num)
        {
            Vector3[] array = new Vector3[length];
            for (int i = 0; i < length; i++)
            {
                //TODO figure out an actual formula
                array[i] = new Vector3((i + num + 1) / 50F, (i + num + 1) / 10F, 0);

            }
            return array;
        }
    }
}
