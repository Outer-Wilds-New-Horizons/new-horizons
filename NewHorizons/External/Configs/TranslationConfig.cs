using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NewHorizons.External.Configs
{
    public class TranslationConfig
    {
        /// <summary>
        /// Translation table for dialogue
        /// </summary>
        public Dictionary<string, string> DialogueDictionary;

        /// <summary>
        /// Translation table for Ship Log (entries, facts, etc)
        /// </summary>
        public Dictionary<string, string> ShipLogDictionary;

        /// <summary>
        /// Translation table for UI elements
        /// </summary>
        public Dictionary<string, string> UIDictionary;

        public TranslationConfig(string filename)
        {
            var dict = JObject.Parse(File.ReadAllText(filename)).ToObject<Dictionary<string, object>>();

            if (dict.ContainsKey(nameof(DialogueDictionary)))
                DialogueDictionary =
                    (Dictionary<string, string>) (dict[nameof(DialogueDictionary)] as JObject).ToObject(
                        typeof(Dictionary<string, string>));
            if (dict.ContainsKey(nameof(ShipLogDictionary)))
                ShipLogDictionary =
                    (Dictionary<string, string>) (dict[nameof(ShipLogDictionary)] as JObject).ToObject(
                        typeof(Dictionary<string, string>));
            if (dict.ContainsKey(nameof(UIDictionary)))
                UIDictionary =
                    (Dictionary<string, string>) (dict[nameof(UIDictionary)] as JObject).ToObject(
                        typeof(Dictionary<string, string>));
        }
    }
}