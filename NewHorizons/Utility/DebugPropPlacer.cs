using NewHorizons.Builder.Props;
using NewHorizons.External.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static NewHorizons.External.PropModule;

namespace NewHorizons.Utility
{
    
    [RequireComponent(typeof(DebugRaycaster))]
    class DebugPropPlacer : MonoBehaviour
    {
        private struct PropPlacementData
        {
            public string body;
            public string system;

            public string propPath;

            public GameObject gameObject;
            public Vector3 pos { get { return gameObject.transform.localPosition; } }
            public Vector3 rotation { get { return gameObject.transform.localEulerAngles; } }

            public string assetBundle;
            public string[] removeChildren;
        }

        // DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var
        public static readonly string DEFAULT_OBJECT = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District1/Props_HangingCity_District1/OtherComponentsGroup/Props_HangingCity_Building_10/Prefab_NOM_VaseThin";

        public string currentObject { get; private set; }
        private bool hasAddedCurrentObjectToRecentsList = false;
        private List<PropPlacementData> props = new List<PropPlacementData>();
        private List<PropPlacementData> deletedProps = new List<PropPlacementData>();
        private DebugRaycaster _rc;

        public HashSet<string> RecentlyPlacedProps = new HashSet<string>();

        private void Awake()
        {
            _rc = this.GetRequiredComponent<DebugRaycaster>();
            currentObject = DEFAULT_OBJECT;
        }

        private void Update()
        {
            if (!Main.Debug) return;

            if (Keyboard.current[Key.Q].wasReleasedThisFrame)
            {
                PlaceObject();
            }

            //if (Keyboard.current[Key.Semicolon].wasReleasedThisFrame)
            //{
            //    PrintConfigs();
            //}
            
            if (Keyboard.current[Key.Minus].wasReleasedThisFrame)
            {
                DeleteLast();
            }
            
            if (Keyboard.current[Key.Equals].wasReleasedThisFrame)
            {
                UndoDelete();
            }
        }

        public void SetCurrentObject(string s)
        {
            currentObject = s;
            hasAddedCurrentObjectToRecentsList = false;
        }

        internal void PlaceObject()
        {
            DebugRaycastData data = _rc.Raycast();
            PlaceObject(data, this.gameObject.transform.position);

            if (!hasAddedCurrentObjectToRecentsList)
            {
                hasAddedCurrentObjectToRecentsList = true;

                if (!RecentlyPlacedProps.Contains(currentObject))
                {
                    RecentlyPlacedProps.Add(currentObject);
                }
            }
        }
        
        public void PlaceObject(DebugRaycastData data, Vector3 playerAbsolutePosition)
        {
            if (!data.hitObject.name.EndsWith("_Body"))
            {
                Logger.Log("Cannot place object on non-body object: " + data.hitObject.name);
            }

            try 
            { 
                // TODO: if currentObject == "" or null, spawn some generic placeholder instead

                if (currentObject == "" || currentObject == null)
                {
                    SetCurrentObject(DEFAULT_OBJECT);
                }

                GameObject prop = DetailBuilder.MakeDetail(data.hitObject, data.hitObject.GetComponentInChildren<Sector>(), currentObject, data.pos, data.norm, 1, false);
                PropPlacementData propData = RegisterProp_WithReturn(data.bodyName, prop);

                // TODO: rotate around vertical axis to face player
                //var dirTowardsPlayer = playerAbsolutePosition - prop.transform.position;
                //dirTowardsPlayer.y = 0;
                
                // align with surface normal
                Vector3 alignToSurface = (Quaternion.LookRotation(data.norm) * Quaternion.FromToRotation(Vector3.up, Vector3.forward)).eulerAngles;
                prop.transform.localEulerAngles = alignToSurface;     
        
                // rotate facing dir
                GameObject g = new GameObject();
                g.transform.parent = prop.transform.parent;
                g.transform.localPosition = prop.transform.localPosition;
                g.transform.localRotation = prop.transform.localRotation;
                
                System.Random r = new System.Random();
                prop.transform.parent = g.transform;

                var dirTowardsPlayer = prop.transform.parent.transform.InverseTransformPoint(playerAbsolutePosition) - prop.transform.localPosition;
                dirTowardsPlayer.y = 0;
                float rotation = Quaternion.LookRotation(dirTowardsPlayer).eulerAngles.y;
                prop.transform.localEulerAngles = new Vector3(0, rotation, 0);
                
                prop.transform.parent = g.transform.parent;
                GameObject.Destroy(g);
            } 
            catch 
            {
                Logger.Log($"Failed to place object {currentObject} on body ${data.hitObject} at location ${data.pos}.");
            }
        }

        public void FindAndRegisterPropsFromConfig(IPlanetConfig config)
        {
            AstroObject planet = AstroObjectLocator.GetAstroObject(config.Name);

            if (planet == null || planet.GetRootSector() == null) return;
            if (config.Props == null || config.Props.Details == null) return;

            List<Transform> potentialProps = new List<Transform>();
            foreach (Transform child in planet.GetRootSector().transform) potentialProps.Add(child);
            potentialProps.Where(potentialProp => potentialProp.gameObject.name.EndsWith("(Clone)")).ToList();

            foreach (var detail in config.Props.Details)
            {
                var propPathElements = detail.path.Split('/');
                string propName = propPathElements[propPathElements.Length-1];

                potentialProps
                    .Where(potentialProp => potentialProp.gameObject.name == propName+"(Clone)")
                    .OrderBy(potentialProp => Vector3.Distance(potentialProp.localPosition, detail.position))
                    .ToList();

                if (potentialProps.Count <= 0)
                {
                    Logger.LogError($"No candidate found for prop {detail.path} on planet ${config.Name}.");
                    continue;
                }

                Transform spawnedProp = potentialProps[0];
                PropPlacementData data = RegisterProp_WithReturn(config.Name, spawnedProp.gameObject, detail.path);
                data.assetBundle = detail.assetBundle;
                data.removeChildren = detail.removeChildren;
                potentialProps.Remove(spawnedProp);

                if (!RecentlyPlacedProps.Contains(data.propPath))
                {
                    RecentlyPlacedProps.Add(data.propPath);
                }
            }
        }

        public void RegisterProp(string bodyGameObjectName, GameObject prop)
        {
            RegisterProp_WithReturn(bodyGameObjectName, prop);
        }

        private PropPlacementData RegisterProp_WithReturn(string bodyGameObjectName, GameObject prop, string propPath = null, string systemName = null)
        {
            if (Main.Debug)
            {
                // TOOD: make this prop an item
            }

            string bodyName = bodyGameObjectName.EndsWith("_Body")
                ? bodyGameObjectName.Substring(0, bodyGameObjectName.Length-"_Body".Length)
                : bodyGameObjectName;
            PropPlacementData data = new PropPlacementData
            {
                body = bodyName,
                propPath = propPath == null ? currentObject : propPath,
                gameObject = prop,
                system = systemName
            };

            props.Add(data);
            return data;
        }

        //public void PrintConfigs()
        //{
        //    foreach(string configFile in GenerateConfigs())
        //    {
        //        Logger.Log(configFile);
        //    }
        //}

        //public List<String> GenerateConfigs()
        //{
        //    var groupedProps = props
        //        .GroupBy(p => AstroObjectLocator.GetAstroObject(p.body).name)
        //        .Select(grp => grp.ToList())
        //        .ToList();
            
        //    List<string> configFiles = new List<string>();

        //    foreach (List<PropPlacementData> bodyProps in groupedProps)
        //    {
        //        string configFile = 
        //            "{" + Environment.NewLine +
        //            "	\"$schema\": \"https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/master/NewHorizons/schema.json\"," + Environment.NewLine +
        //            $"	\"name\" : \"{bodyProps[0].body}\"," + Environment.NewLine +
        //            "	\"Props\" :" + Environment.NewLine +
        //            "	{" + Environment.NewLine +
        //            "		\"details\": [" + Environment.NewLine;
                    
        //        for(int i = 0; i < bodyProps.Count; i++)
        //        {
        //            PropPlacementData prop = bodyProps[i];

        //            string positionString = $"\"x\":{prop.pos.x},\"y\":{prop.pos.y},\"z\":{prop.pos.z}";
        //            string rotationString = $"\"x\":{prop.rotation.x},\"y\":{prop.rotation.y},\"z\":{prop.rotation.z}";
        //            string endingString = i == bodyProps.Count-1 ? "" : ",";

        //            configFile += "			{" +
        //                "\"path\" : \"" +prop.propPath+ "\", " +
        //                "\"position\": {"+positionString+"}, " +
        //                "\"rotation\": {"+rotationString+"}, " +
        //                "\"scale\": 1"+
        //                (prop.assetBundle == null    ? "" : $", \"assetBundle\": \"{prop.assetBundle}\"") +
        //                (prop.removeChildren == null ? "" : $", \"removeChildren\": \"[{string.Join(",",prop.removeChildren)}]\"") +
        //                "}" + endingString + Environment.NewLine;
        //        }

        //        configFile += 
        //            "		]" + Environment.NewLine +
        //            "    }" + Environment.NewLine +
        //            "}";

        //        configFiles.Add(configFile);
        //    }

        //    return configFiles;
        //}

        public Dictionary<string, DetailInfo[]> GetPropsConfigByBody(bool useAstroObjectName = false)
        {
            var groupedProps = props
                .GroupBy(p => p.system + "." + p.body)
                .Select(grp => grp.ToList())
                .ToList();
            
            Dictionary<string, DetailInfo[]> propConfigs = new Dictionary<string, DetailInfo[]>();

            foreach (List<PropPlacementData> bodyProps in groupedProps)
            {
                
                if (bodyProps == null || bodyProps.Count == 0) continue; 
                if ( AstroObjectLocator.GetAstroObject(bodyProps[0].body) == null ) continue;
                string bodyName = useAstroObjectName ? AstroObjectLocator.GetAstroObject(bodyProps[0].body).name : bodyProps[0].body;
                
                DetailInfo[] infoArray = new DetailInfo[bodyProps.Count];
                propConfigs[bodyProps[0].system + DebugMenu.separatorCharacter + bodyName] = infoArray;
        
                for(int i = 0; i < bodyProps.Count; i++)
                {
                    infoArray[i] = new DetailInfo()
                    {
                        path = bodyProps[i].propPath,
                        assetBundle = bodyProps[i].assetBundle,
                        position = bodyProps[i].gameObject.transform.localPosition,
                        rotation = bodyProps[i].gameObject.transform.localEulerAngles,
                        scale = bodyProps[i].gameObject.transform.localScale.x,
                        //public bool alignToNormal; // TODO: figure out how to recover this (or actually, rotation should cover it)
                        removeChildren = bodyProps[i].removeChildren
                    };
                }
            }

            return propConfigs;
        }

        public void DeleteLast()
        {
            if (props.Count <= 0) return;

            PropPlacementData last = props[props.Count-1];
            props.RemoveAt(props.Count-1);
            
            last.gameObject.SetActive(false);

            deletedProps.Add(last);
        }

        public void UndoDelete()
        {
            if (deletedProps.Count <= 0) return;

            PropPlacementData last = deletedProps[deletedProps.Count-1];
            deletedProps.RemoveAt(deletedProps.Count-1);
            
            last.gameObject.SetActive(true);

            props.Add(last);
        }
    }
}
