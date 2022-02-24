using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Configs
{
    public class Config
    {
        public Config(Dictionary<string, object> dict)
        {
            if (dict == null) return;

            foreach (var item in dict)
            {
                var property = GetType().GetProperty(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (property == null) property = GetType().GetProperty(item.Key.ToCamelCase(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null) property = GetType().GetProperty(item.Key.ToTitleCase(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                Logger.Log($"Couldn't find [{item.Key}] in [{string.Concat(GetType().GetProperties().Select(x => x.Name))}] for [{GetType()}]");

                if (property != null)
                {
                    if (property.PropertyType.BaseType == typeof(Module))
                    {
                        if (property.GetValue(this) == null)
                        {
                            var module = Activator.CreateInstance(property.PropertyType);
                            property.SetValue(this, module);
                        }
                        ((Module)property.GetValue(this)).Build(item.Value as Dictionary<string, object>);
                    }
                    else
                    {
                        property.SetValue(this, Convert.ChangeType(item.Value, property.PropertyType));
                    }
                }
                else Logger.LogError($"{item.Key} {item.Value} is not valid. Is your config formatted correctly?");
            }
        }
    }
}
