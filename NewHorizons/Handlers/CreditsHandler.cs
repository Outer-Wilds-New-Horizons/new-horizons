using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Handlers
{
    public static class CreditsHandler
    {
        private static Dictionary<string, string[]> _creditsInfo;

        public static void RegisterCredits(string sectionName, string[] entries)
        {
            if (_creditsInfo == null) _creditsInfo = new();

            Logger.LogVerbose($"Registering credits for {sectionName}");

            _creditsInfo[sectionName] = entries;
        }

        public static void AddCredits(Credits credits)
        {
            Logger.LogVerbose($"Adding to credits");

            var creditsAsset = credits._creditsAsset;

            var xml = new XmlDocument();
            xml.LoadXml(creditsAsset.xml.text);

            foreach (var pair in _creditsInfo)
            {
                AddCreditsSection(pair.Key, pair.Value, ref xml);
            }

            var outerXml = xml.OuterXml.Replace("/n", "&#xD;&#xA;");

            creditsAsset.xml = new TextAsset(outerXml);
        }

        private static void AddCreditsSection(string sectionName, string[] entries, ref XmlDocument xml)
        {
            var finalCredits = xml.SelectSingleNode("Credits/section");

            // Fade credits look wrong right now bc it works with just one column not two
            /*
            var nodeFade = CreateCreditsFromList(xml, sectionName, entries, true);
            finalCredits.InsertAfter(nodeFade, finalCredits.ChildNodes[0]);
            */

            var fastCredits = NodeWhere(finalCredits.ChildNodes, "MainScrollSection");
            var nodeScroll = CreateCreditsFromList(xml, sectionName, entries, false);
            fastCredits.InsertBefore(nodeScroll, fastCredits.ChildNodes[0]);
        }

        private static XmlNode NodeWhere(XmlNodeList list, string name)
        {
            foreach(XmlNode node in list)
            {
                try
                {
                    if (node.Attributes[0].Value == name) return node;
                }
                catch { }
            }
            return null;
        }

        private static XmlNode CreateCreditsFromList(XmlDocument doc, string title, string[] entries, bool fade)
        {
            var rootSection = MakeNode(doc, "section", fade ? new Dictionary<string, string>()
            {
                { "platform", "All" },
                { "type", "Fade" },
                { "fadeInTime", "1.3" },
                { "displayTime", "10" },
                { "fadeOutTime", "1.4" },
                { "waitTime", "0.5" },
                { "padding-bottom", "-8" },
                { "spacing", "16" }
            } : new Dictionary<string, string>()
            {
                { "platform", "All" },
                { "type", "Scroll" },
                { "scrollDuration", "214" },
                { "spacing", "12" },
                { "width", "1590" }
            });

            var titleLayout = MakeNode(doc, "layout", new Dictionary<string, string>()
            {
                { "type", fade ? "SingleColumnFadeCentered" : "SingleColumnScrollCentered" }
            });

            var titleNode = MakeNode(doc, "title", new Dictionary<string, string>()
            {
                {"text-align", "UpperCenter" },
                {"height", "122" }
            });
            titleNode.InnerText = title;
            titleLayout.AppendChild(titleNode);

            var type = fade ? "SingleColumnFadeCentered" : "TwoColumnScrollAlignRightLeft";
            var xmlText = $"<layout type=\"{type}\" spacer-base-height=\"10\">\n";
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (fade) entry = entry.Split('#')[0];

                xmlText += $"{entry}\n";
                if (fade)
                {
                    if (i == entries.Length - 1) xmlText += "<spacer height = \"295\" />\n";
                    else xmlText += "<spacer />\n";
                }
            }
            xmlText += "</layout>";

            rootSection.AppendChild(titleLayout);
            rootSection.AppendChild(StringToNode(doc, xmlText));

            return rootSection;
        }

        private static XmlNode StringToNode(XmlDocument docContext, string text)
        {
            var doc = new XmlDocument();
            doc.LoadXml(text);

            // ArgumentException: The node to be inserted is from a different document context.
            var importedNode = docContext.ImportNode(doc.DocumentElement, true);

            return importedNode;
        }

        private static XmlNode MakeNode(XmlDocument doc, string nodeType, Dictionary<string, string> attributes)
        {
            var xmlNode = doc.CreateElement(nodeType);

            if (attributes != null)
            {
                foreach (var pair in attributes)
                {
                    var attribute = doc.CreateAttribute(pair.Key);
                    attribute.Value = pair.Value;
                    xmlNode.Attributes.Append(attribute);
                }
            }

            return xmlNode;
        }

        internal class CreditsInfo
        {
            string sectionName;
            string[] entries;

            internal CreditsInfo(string sectionName, string[] entries)
            {
                this.sectionName = sectionName;
                this.entries = entries;
            }
        }
    }
}
