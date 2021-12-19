using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
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
