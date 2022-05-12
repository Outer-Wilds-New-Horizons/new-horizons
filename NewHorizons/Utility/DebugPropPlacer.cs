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
            public Vector3 initial_pos;
            public Vector3 initial_rotation;
            public string body;

            public string propPath;

            public GameObject gameObject;
            public Vector3 pos { get { return gameObject.transform.localPosition; } }
            public Vector3 rotation { get { return gameObject.transform.localEulerAngles; } }
        }

        // placeholder currentObject
        public static string currentObject = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District1/Props_HangingCity_District1/OtherComponentsGroup/Props_HangingCity_Building_10/Prefab_NOM_VaseThin";
        private static List<PropPlacementData> props = new List<PropPlacementData>();

        // TODO: RegisterProp function, call it in DetailBuilder.MakeDetail
        // TODO: Add default rotation and position offsets

        public static void PlaceObject(DebugRaycastData data)
        {
            if (!data.hitObject.name.EndsWith("_Body"))
            {
                Logger.Log("Cannot place object on non-body object: " + data.hitObject.name);
            }

            try 
            { 
                // TODO: if currentObject == "" or null, spawn some generic placeholder instead

                GameObject prop = DetailBuilder.MakeDetail(data.hitObject, data.hitObject.GetComponentInChildren<Sector>(), currentObject, data.pos, data.norm, 1, false);
                PropPlacementData propData = RegisterProp_WithReturn(data.bodyName, prop);
                propData.initial_pos = data.pos;
                propData.initial_rotation = data.norm;

                string origEul = prop.transform.localEulerAngles.ToString();
                prop.transform.localRotation = Quaternion.LookRotation(data.norm) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
                Logger.Log($"{data.norm.ToString()}   -=-    {prop.transform.localEulerAngles.ToString()}  =>  {prop.transform.localEulerAngles.ToString()}");
            } 
            catch (Exception e)
            {
                Logger.Log($"Failed to place object {currentObject} on body ${data.hitObject} at location ${data.pos}.");
            }
        }

        public static void RegisterProp(string bodyGameObjectName, GameObject prop)
        {
            RegisterProp_WithReturn(bodyGameObjectName, prop);
        }

        private static PropPlacementData RegisterProp_WithReturn(string bodyGameObjectName, GameObject prop)
        {
            if (Main.Debug)
            {
                // TOOD: make this prop an item
            }

            string bodyName = bodyGameObjectName.Substring(0, bodyGameObjectName.Length-"_Body".Length);
            PropPlacementData data = new PropPlacementData
            {
                body = bodyName,
                propPath = currentObject,
                gameObject = prop
            };

            props.Add(data);
            return data;
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

                    configFile += "			{\"path\" : \"" +prop.propPath+ "\", \"position\": {"+positionString+"}, \"rotation\": {"+rotationString+"}, \"scale\": 1}" + endingString + Environment.NewLine;
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
