using NewHorizons.Utility.OWML;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class CreditsHandler
    {
        private static Dictionary<string, string[]> _creditsInfo;

        public static void RegisterCredits(string sectionName, string[] entries)
        {
            if (_creditsInfo == null) _creditsInfo = new();

            NHLogger.LogVerbose($"Registering credits for {sectionName}");

            _creditsInfo[sectionName] = entries;
        }

        public static void AddCredits(Credits credits)
        {
            NHLogger.LogVerbose($"Adding to credits");

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
            var finalCredits = xml.SelectSingleNode("Credits/section[@name='CreditsFinal']");

            /*
             * Looks bad, would need more customization, complicated, messes up music timing, wont do for now
            var nodeFade = CreateFadeCreditsFromList(xml, sectionName, entries);
            finalCredits.InsertAfter(nodeFade, finalCredits.ChildNodes[0]);
            */

            var fastCredits = NodeWhere(finalCredits.ChildNodes, "MainScrollSection");
            var nodeScroll = CreateScrollCreditsFromList(xml, sectionName, entries);
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

        // Looked bad so not used
        /*
        private static XmlNode CreateFadeCreditsFromList(XmlDocument doc, string title, string[] entries)
        {
            var rootSection = MakeNode(doc, "section", new Dictionary<string, string>()
            {
                { "platform", "All" },
                { "type", "Fade" },
                { "fadeInTime", "1.3" },
                { "displayTime", "10" },
                { "fadeOutTime", "1.4" },
                { "waitTime", "0.5" },
                { "padding-bottom", "-8" },
                { "spacing", "16" }
            });

            var titleLayout = MakeNode(doc, "layout", new Dictionary<string, string>()
            {
                { "type", "SingleColumnFadeCentered" }
            });

            var titleNode = MakeNode(doc, "title", new Dictionary<string, string>()
            {
                {"text-align", "UpperCenter" },
                {"height", "122" }
            });
            titleNode.InnerText = title;
            titleLayout.AppendChild(titleNode);

            var type = "SingleColumnFadeCentered";
            var xmlText = $"<layout type=\"{type}\" spacer-base-height=\"10\">\n";
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                if (entry.Contains("#"))
                {
                    // Replace first one with a space
                    entry = RemoveExcessSpaces(entry);
                    var indexOfColon = entry.IndexOf(":");
                    var firstPart = entry.Substring(0, Math.Min(entry.IndexOf("#"), indexOfColon == -1 ? int.MaxValue : indexOfColon));
                    entry = firstPart + ": " + entry.Substring(entry.IndexOf("#") + 1);
                }
                entry = entry.Replace("#", ", ").Replace("/n", "");

                xmlText += $"{entry}\n";
                xmlText += "<spacer />\n";
            }
            xmlText += "<spacer height = \"295\" />\n";
            xmlText += "</layout>";

            rootSection.AppendChild(titleLayout);
            foreach (var node in StringToNodes(doc, xmlText)) rootSection.AppendChild(node);

            return rootSection;
        }

        private static string RemoveExcessSpaces(string s)
        {
            var options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            return regex.Replace(s, " ");
        }
        */

        private static XmlNode CreateScrollCreditsFromList(XmlDocument doc, string title, string[] entries)
        {
            var rootSection = MakeNode(doc, "section", new Dictionary<string, string>()
            {
                { "name", title },
                { "credits-type", "Final Fast Krazy" }
            });

            var titleLayout = MakeNode(doc, "layout", new Dictionary<string, string>()
            {
                { "type", "SingleColumnScrollCentered" }
            });

            var titleNode = MakeNode(doc, "title", new Dictionary<string, string>()
            {
                {"text-align", "UpperCenter" },
                {"height", "122" }
            });
            titleNode.InnerText = title;
            titleLayout.AppendChild(titleNode);


            var xmlText = "";
            bool? flag = null;
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];

                var twoColumn = entry.Contains("#");
                if (flag != twoColumn)
                {
                    if (i != 0) xmlText += "</layout>";
                    var type = twoColumn ? "TwoColumnScrollAlignRightLeft" : "SingleColumnScrollCentered";
                    xmlText += $"<layout type=\"{type}\" spacer-base-height=\"10\">\n";
                    flag = twoColumn;
                }

                xmlText += $"{entry}\n";
                xmlText += "<spacer/>";
            }
            xmlText += "<spacer height = \"295\" />\n";
            xmlText += "</layout>";

            rootSection.AppendChild(titleLayout);
            foreach(var node in StringToNodes(doc, xmlText)) rootSection.AppendChild(node);

            return rootSection;
        }

        private static XmlNode[] StringToNodes(XmlDocument docContext, string text)
        {
            var doc = new XmlDocument();
            // Doing this funny thing so that theres a single parent root thing
            doc.LoadXml("<root>" + text + "</root>");

            // ArgumentException: The node to be inserted is from a different document context.
            var nodes = new List<XmlNode>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                nodes.Add(docContext.ImportNode(node, true));
            }

            return nodes.ToArray();
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
    }
}
