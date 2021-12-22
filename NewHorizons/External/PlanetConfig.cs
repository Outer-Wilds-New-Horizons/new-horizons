using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NewHorizons.External
{
    public class PlanetConfig : IPlanetConfig
    {
        public string Name { get; set; }
        public bool Destroy { get; set; }
        public int BuildPriority { get; set; } = -1;
        public MVector3 SpawnPoint { get; set; }
        public BaseModule Base { get; set; }
        public AtmosphereModule Atmosphere { get; set; }
        public OrbitModule Orbit { get; set; }
        public RingModule Ring { get; set; }
        public HeightMapModule HeightMap { get; set; }
        public ProcGenModule ProcGen { get; set; }
        public AsteroidBeltModule AsteroidBelt { get; set; }
        public StarModule Star { get; set; }
        public FocalPointModule FocalPoint { get; set; }
        public SpawnModule Spawn { get; set; }

        public PlanetConfig(Dictionary<string, object> dict)
        {
            // Always have to have a base module and orbit module
            Base = new BaseModule();
            Orbit = new OrbitModule();

            if (dict == null) return;

            foreach (var item in dict)
            {
                var property = typeof(PlanetConfig).GetProperty(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if(property == null)
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

                /*
                switch(item.Key)
                {
                    case "Base":
                        Base.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Orbit":
                        Orbit.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Atmosphere":
                        Atmosphere = new AtmosphereModule();
                        Atmosphere.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Ring":
                        Ring = new RingModule();
                        Ring.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "HeightMap":
                        HeightMap = new HeightMapModule();
                        HeightMap.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "ProcGen":
                        ProcGen = new ProcGenModule();
                        ProcGen.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "AsteroidBelt":
                        AsteroidBelt = new AsteroidBeltModule();
                        AsteroidBelt.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Spawn":
                        Spawn = new SpawnModule();
                        Spawn.Build(item.Value as Dictionary<string, object>);
                        break;
                    default:
                        var property = typeof(PlanetConfig).GetProperty(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (property != null) property.SetValue(this, Convert.ChangeType(item.Value, property.PropertyType));
                        else Logger.LogError($"{item.Key} {item.Value} is not valid. Is your config formatted correctly?");
                        break;
                }
                */
            }
        }
    }
}
