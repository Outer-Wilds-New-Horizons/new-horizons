using NewHorizons.Utility;
using System;
using System.Collections.Generic;

namespace NewHorizons.External
{
    public class PlanetConfig : IPlanetConfig
    {
        public string Name { get; set; }
        public bool Destroy { get; set; }
        public MVector3 SpawnPoint { get; set; }
        public BaseModule Base { get; set; }
        public AtmosphereModule Atmosphere { get; set; }
        public OrbitModule Orbit { get; set; }
        public RingModule Ring { get; set; }
        public HeightMapModule HeightMap { get; set; }
        public SpawnModule Spawn { get; set; }

        public PlanetConfig(Dictionary<string, object> dict)
        {
            // Always have to have a base module
            Base = new BaseModule();
            if (dict == null)
            {
                return;
            }
            foreach (var item in dict)
            {
                Logger.Log($"{item.Key} : {item.Value}", Logger.LogType.Log);

                switch(item.Key)
                {
                    case "Base":
                        Base.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Atmosphere":
                        Atmosphere = new AtmosphereModule();
                        Atmosphere.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Orbit":
                        Orbit = new OrbitModule();
                        Orbit.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Ring":
                        Ring = new RingModule();
                        Ring.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "HeightMap":
                        HeightMap = new HeightMapModule();
                        HeightMap.Build(item.Value as Dictionary<string, object>);
                        break;
                    case "Spawn":
                        Spawn = new SpawnModule();
                        Spawn.Build(item.Value as Dictionary<string, object>);
                        break;
                    default:
                        var field = GetType().GetField(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (field != null)
                            field.SetValue(this, Convert.ChangeType(item.Value, field.FieldType));
                        else
                            Logger.LogError($"{item.Key} is not valid. Is your config file formatted correctly?");
                        break;
                }

                /*
                var field = GetType().GetField(item.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, Convert.ChangeType(item.Value, field.FieldType));
                */
            }
        }
    }
}
