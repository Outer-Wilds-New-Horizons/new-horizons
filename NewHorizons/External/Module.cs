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
                Logger.Log($"{item.Key} : {item.Value}", Logger.LogType.Log);

                var field = GetType().GetField(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, Convert.ChangeType(item.Value, field.FieldType));
            }
        }
    }
}
