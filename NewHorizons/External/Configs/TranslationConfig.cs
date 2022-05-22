using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace NewHorizons.External.Configs
{
    public class TranslationConfig
    {
        public Dictionary<string, string> dialogueDictionary;
        public Dictionary<string, string> shipLogDictionary;
        public Dictionary<string, string> uiDictionary;

        public TranslationConfig(string filename)
        {
            var dict = JObject.Parse(File.ReadAllText(filename)).ToObject<Dictionary<string, object>>();

            if (dict.ContainsKey(nameof(dialogueDictionary)))
                dialogueDictionary =
                    (Dictionary<string, string>) (dict[nameof(dialogueDictionary)] as JObject).ToObject(
                        typeof(Dictionary<string, string>));
            if (dict.ContainsKey(nameof(shipLogDictionary)))
                shipLogDictionary =
                    (Dictionary<string, string>) (dict[nameof(shipLogDictionary)] as JObject).ToObject(
                        typeof(Dictionary<string, string>));
            if (dict.ContainsKey(nameof(uiDictionary)))
                uiDictionary =
                    (Dictionary<string, string>) (dict[nameof(uiDictionary)] as JObject).ToObject(
                        typeof(Dictionary<string, string>));
        }
    }
}