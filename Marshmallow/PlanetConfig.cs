using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Marshmallow
{
    class PlanetConfig
    {
        [JsonProperty("settings")]
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

		public T GetSettingsValue<T>(string key)
		{
			bool flag = !this.Settings.ContainsKey(key);
			T result;
			if (flag)
			{
				Main.Log("Error: setting not found: " + key);
				result = default(T);
			}
			else
			{
				object obj = this.Settings[key];
				try
				{
					JObject jobject;
					object value = ((jobject = (obj as JObject)) != null) ? jobject["value"] : obj;
					result = (T)((object)Convert.ChangeType(value, typeof(T)));
				}
				catch (InvalidCastException)
				{
					Main.Log(string.Format("Error when converting setting {0} of type {1} to type {2}", key, obj.GetType(), typeof(T)));
					result = default(T);
				}
			}
			return result;
		}

	}
}
