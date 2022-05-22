using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
namespace NewHorizons.External.Configs
{
    public class TranslationConfig
    {
        public Dictionary<string, string> DialogueDictionary;
        public Dictionary<string, string> ShipLogDictionary;
        public Dictionary<string, string> UIDictionary;

        public TranslationConfig(string filename)
        {
            Dictionary<string, object> dict = JObject.Parse(File.ReadAllText(filename)).ToObject<Dictionary<string, object>>();

            if (dict.ContainsKey(nameof(DialogueDictionary)))
            {
                DialogueDictionary = (Dictionary<string, string>)(dict[nameof(DialogueDictionary)] as Newtonsoft.Json.Linq.JObject).ToObject(typeof(Dictionary<string, string>));
            }
            if (dict.ContainsKey(nameof(ShipLogDictionary)))
            {
                ShipLogDictionary = (Dictionary<string, string>)(dict[nameof(ShipLogDictionary)] as Newtonsoft.Json.Linq.JObject).ToObject(typeof(Dictionary<string, string>));
            }
            if (dict.ContainsKey(nameof(UIDictionary)))
            {
                UIDictionary = (Dictionary<string, string>)(dict[nameof(UIDictionary)] as Newtonsoft.Json.Linq.JObject).ToObject(typeof(Dictionary<string, string>));
            }
        }
    }
}
