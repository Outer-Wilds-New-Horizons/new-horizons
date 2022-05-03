using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.Utility;
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
        private static GameObject _arcPrefab;
        private static GameObject _scrollPrefab;

        //TODO Scrolls
        public static void Make(GameObject go, Sector sector, PropModule.NomaiTextInfo info, IModBehaviour mod)
        {
            if (_arcPrefab == null)
            {
                _arcPrefab = GameObject.Find("TimberHearth_Body/Sector_TH/Sector_Village/Sector_Observatory/Interactables_Observatory/NomaiEyeExhibit/NomaiEyePivot/Arc_TH_Museum_EyeSymbol/Arc 1").InstantiateInactive();
                _arcPrefab.name = "Arc";
            }
            if (_scrollPrefab == null)
            {
                _scrollPrefab = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District2/Interactables_HangingCity_District2/Prefab_NOM_Scroll").InstantiateInactive();
                _scrollPrefab.name = "Prefab_NOM_Scroll";
            }

            if (info.type == "wall")
            {
                var nomaiWallTextObj = MakeWallText(go, sector, info, mod).gameObject;

                nomaiWallTextObj.transform.parent = sector?.transform ?? go.transform;
                nomaiWallTextObj.transform.localPosition = info.position;
                nomaiWallTextObj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, info.normal ?? Vector3.forward);

                nomaiWallTextObj.SetActive(true);
            }
            else if (info.type == "scroll")
            {
                var customScroll = _scrollPrefab.InstantiateInactive();

                var nomaiWallText = MakeWallText(go, sector, info, mod);
                nomaiWallText.transform.parent = customScroll.transform;
                nomaiWallText.transform.localPosition = Vector3.zero;
                nomaiWallText.transform.localRotation = Quaternion.identity;

                // Don't want to be able to translate until its in a socket
                nomaiWallText.GetComponent<Collider>().enabled = false;

                nomaiWallText.gameObject.SetActive(true);

                var scrollItem = customScroll.GetComponent<ScrollItem>();

                // Idk why this thing is always around
                GameObject.Destroy(customScroll.transform.Find("Arc_BH_City_Forum_2").gameObject);

                // This variable is the bane of my existence i dont get it
                scrollItem._nomaiWallText = nomaiWallText;

                // Because the scroll was already awake it does weird shit in Awake and makes some of the entries in this array be null
                scrollItem._colliders = new OWCollider[] { scrollItem.GetComponent<OWCollider>() };

                // Else when you put them down you can't pick them back up
                customScroll.GetComponent<OWCollider>()._physicsRemoved = false;

                // Place scroll
                customScroll.transform.parent = sector?.transform ?? go.transform;
                customScroll.transform.localPosition = info.position ?? Vector3.zero;

                var up = customScroll.transform.localPosition.normalized;
                customScroll.transform.rotation = Quaternion.FromToRotation(customScroll.transform.up, up) * customScroll.transform.rotation;

                customScroll.SetActive(true);

                // Enable the collider and renderer
                Main.Instance.ModHelper.Events.Unity.RunWhen(
                    () => Main.IsSystemReady,
                    () =>
                    {
                        Logger.Log("Fixing scroll!");
                        scrollItem._nomaiWallText = nomaiWallText;
                        scrollItem.SetSector(sector);
                        customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Geo").GetComponent<MeshRenderer>().enabled = true;
                        customScroll.transform.Find("Props_NOM_Scroll/Props_NOM_Scroll_Collider").gameObject.SetActive(true);
                        nomaiWallText.gameObject.GetComponent<Collider>().enabled = false;
                        customScroll.GetComponent<CapsuleCollider>().enabled = true;
                    }
                );
            }
            else
            {
                Logger.LogError($"Unsupported NomaiText type {info.type}");
            }
        }

        private static NomaiWallText MakeWallText(GameObject go, Sector sector, PropModule.NomaiTextInfo info, IModBehaviour mod)
        {
            GameObject nomaiWallTextObj = new GameObject("NomaiWallText");
            nomaiWallTextObj.SetActive(false);

            // TODO better bounds
            var box = nomaiWallTextObj.AddComponent<BoxCollider>();
            box.center = new Vector3(-0.0643f, 1.1254f, 0f);
            box.size = new Vector3(6.1424f, 5.2508f, 0.5f);

            box.isTrigger = true;

            nomaiWallTextObj.AddComponent<OWCollider>();
            var nomaiWallText = nomaiWallTextObj.AddComponent<NomaiWallText>();

            var xml = System.IO.File.ReadAllText(mod.ModHelper.Manifest.ModFolderPath + info.xmlFile);
            var text = new TextAsset(xml);

            nomaiWallText._dictNomaiTextData = new Dictionary<int, NomaiText.NomaiTextData>(ComparerLibrary.intEqComparer);
            nomaiWallText._nomaiTextAsset = text;

            BuildArcs(xml, nomaiWallText, nomaiWallTextObj);
            AddTranslation(xml);

            nomaiWallText.SetTextAsset(text);

            return nomaiWallText;
        }

        private static void BuildArcs(string xml, NomaiWallText nomai, GameObject conversationZone)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode rootNode = xmlDocument.SelectSingleNode("NomaiObject");

            var i = 1;
            foreach (object obj in rootNode.SelectNodes("TextBlock"))
            {
                XmlNode xmlNode = (XmlNode)obj;
                int textEntryID = -1;
                XmlNode xmlNode2 = xmlNode.SelectSingleNode("ID");
                XmlNode textNode = xmlNode.SelectSingleNode("Text");
                XmlNode xmlNode3 = xmlNode.SelectSingleNode("ParentID");
                int parentID = -1;
                if (xmlNode2 != null && !int.TryParse(xmlNode2.InnerText, out textEntryID))
                {
                    textEntryID = -1;
                }
                if (xmlNode3 != null && !int.TryParse(xmlNode3.InnerText, out parentID))
                {
                    parentID = -1;
                }

                NomaiText.NomaiTextData value = new NomaiText.NomaiTextData(textEntryID, parentID, textNode, false, NomaiText.Location.UNSPECIFIED);
                nomai._dictNomaiTextData.Add(textEntryID, value);

                var arc = _arcPrefab.InstantiateInactive();
                arc.name = $"Arc {i++}";
                arc.transform.parent = conversationZone.transform;
                arc.transform.localPosition = Vector3.zero;
                arc.transform.LookAt(Vector3.forward);
                arc.GetComponent<NomaiTextLine>().SetEntryID(textEntryID);
                arc.GetComponent<MeshRenderer>().enabled = false;
                arc.SetActive(true);
            }
        }

        private static void AddTranslation(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            XmlNode xmlNode = xmlDocument.SelectSingleNode("NomaiObject");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("TextBlock");

            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                var text = xmlNode2.SelectSingleNode("Text").InnerText;
                TranslationHandler.AddDialogue(text);
            }
        }
    }
}
