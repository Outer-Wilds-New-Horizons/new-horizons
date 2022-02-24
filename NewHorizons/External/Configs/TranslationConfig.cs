using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.External.Configs
{
    public class TranslationConfig
    {
        public Dictionary<string, string> DialogueDictionary;
        public Dictionary<string, string> ShipLogDictionary;

        public TranslationConfig(string filename)
        {
            Dictionary<string, object> dict = JObject.Parse(File.ReadAllText(filename)).ToObject<Dictionary<string, object>>();

            if(dict.ContainsKey(nameof(DialogueDictionary)))
            {
                DialogueDictionary = (Dictionary<string, string>)(dict[nameof(DialogueDictionary)] as Newtonsoft.Json.Linq.JObject).ToObject(typeof(Dictionary<string, string>));
            }
            if (dict.ContainsKey(nameof(ShipLogDictionary)))
            {
                ShipLogDictionary = (Dictionary<string, string>)(dict[nameof(ShipLogDictionary)] as Newtonsoft.Json.Linq.JObject).ToObject(typeof(Dictionary<string, string>));
            }            
        }
    }
}
