using System;
using System.Collections.Generic;
namespace NewHorizons.External.Modules
{
    public abstract class Module
    {
        public void Build(Dictionary<string, object> dict)
        {
            if (dict == null)
            {
                return;
            }
            foreach (var item in dict)
            {
                var property = GetType().GetProperty(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                property.SetValue(this, Convert.ChangeType(item.Value, property.PropertyType));
            }
        }
    }
}
