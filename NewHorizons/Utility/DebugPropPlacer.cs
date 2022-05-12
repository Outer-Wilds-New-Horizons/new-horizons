using NewHorizons.Builder.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Utility
{

    class DebugPropPlacer
    {
        private struct PropPlacementData
        {
            public Vector3 pos;
            public Vector3 rotation;
            public string body;

        }

        // placeholder currentObject
        public static string currentObject = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District1/Props_HangingCity_District1/OtherComponentsGroup/Props_HangingCity_Building_10/Prefab_NOM_VaseThin";
        private static List<PropPlacementData> props = new List<PropPlacementData>();

        public static void PlaceObject(DebugRaycastData data)
        {
            if (!data.hitObject.name.EndsWith("_Body"))
            {
                Logger.Log("Cannot place object on non-body object: " + data.hitObject.name);
            }

            try 
            { 
                GameObject prop = DetailBuilder.MakeDetail(data.hitObject, data.hitObject.GetComponentInChildren<Sector>(), currentObject, data.pos, data.norm, 1, false);

                string bodyName = data.hitObject.name.Substring(0, data.bodyName.Length-"_Body".Length);
                props.Add(new PropPlacementData
                    {
                        pos = data.pos,
                        rotation = data.norm,
                        body = bodyName
                    }
                );
            } 
            catch (Exception e)
            {
                Logger.Log($"Failed to place object {currentObject} on body ${data.hitObject} at location ${data.pos}.");
            }
        }

        public static void PrintConfig()
        {
            var groupedProps = props
                .GroupBy(p => p.body)
                .Select(grp => grp.ToList())
                .ToList();

            foreach (List<PropPlacementData> bodyProps in groupedProps)
            {
                string configFile = 
                    "{" + Environment.NewLine +
                    "	\"$schema\": \"https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/master/NewHorizons/schema.json\"," + Environment.NewLine +
                    $"	\"name\" : \"{bodyProps[0].body}\"," + Environment.NewLine +
                    "	\"Props\" :" + Environment.NewLine +
                    "	{" + Environment.NewLine +
                    "		\"details\": [" + Environment.NewLine;
                    
                for(int i = 0; i < bodyProps.Count; i++)
                {
                    PropPlacementData prop = bodyProps[i];

                    string positionString = $"\"x\":{prop.pos.x},\"y\":{prop.pos.y},\"z\":{prop.pos.z}";
                    string rotationString = $"\"x\":{prop.rotation.x},\"y\":{prop.rotation.y},\"z\":{prop.rotation.z}";
                    string endingString = i == bodyProps.Count-1 ? "" : ",";

                    configFile += "			{\"path\" : \"" + "\", \"position\": {"+positionString+"}, \"rotation\": {"+rotationString+"}, \"scale\": 1}" + endingString + Environment.NewLine;
                }

                configFile += 
                    "		]" + Environment.NewLine +
                    "    }" + Environment.NewLine +
                    "}";

            
                Logger.Log(configFile);
            }
        }
    }
}
