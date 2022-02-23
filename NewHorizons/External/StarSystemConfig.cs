using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External
{
    public class StarSystemConfig
    {
        public bool canEnterViaWarpDrive = true;
        public bool startHere = false;
        public string factRequiredForWarp;
        public NomaiCoordinates coords;

        public class NomaiCoordinates
        {
            public int[] x;
            public int[] y;
            public int[] z;
        }

        public StarSystemConfig(Dictionary<string, object> dict)
        {
            if (dict == null) return;

            foreach (var item in dict)
            {
                var property = typeof(PlanetConfig).GetProperty(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null)
                    property = typeof(PlanetConfig).GetProperty(item.Key.ToCamelCase(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (property == null)
                    property = typeof(PlanetConfig).GetProperty(item.Key.ToTitleCase(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

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
