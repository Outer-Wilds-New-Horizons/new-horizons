using NewHorizons.Builder.Props;
using NewHorizons.External.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NewHorizons.Utility
{
    
    [RequireComponent(typeof(DebugRaycaster))]
    class DebugPropPlacer : MonoBehaviour
    {
        private struct PropPlacementData
        {
            public string body;

            public string propPath;

            public GameObject gameObject;
            public Vector3 pos { get { return gameObject.transform.localPosition; } }
            public Vector3 rotation { get { return gameObject.transform.localEulerAngles; } }
        }

        // DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var
        public static readonly string DEFAULT_OBJECT = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District1/Props_HangingCity_District1/OtherComponentsGroup/Props_HangingCity_Building_10/Prefab_NOM_VaseThin";

        public string currentObject { get; private set; }
        private bool hasAddedCurrentObjectToRecentsList = false;
        private List<PropPlacementData> props = new List<PropPlacementData>();
        private List<PropPlacementData> deletedProps = new List<PropPlacementData>();
        private DebugRaycaster _rc;

        public List<string> RecentlyPlacedProps = new List<string>();

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

            if (Keyboard.current[Key.Semicolon].wasReleasedThisFrame)
            {
                PrintConfigs();
            }
            
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
            catch (Exception e)
            {
                Logger.Log($"Failed to place object {currentObject} on body ${data.hitObject} at location ${data.pos}.");
            }
        }

        public void FindAndRegisterPropsFromConfig(IPlanetConfig config)
        {
            AstroObject planet = AstroObjectLocator.GetAstroObject(config.Name);

            foreach (var detail in config.Props.Details)
            {
                foreach (Transform child in planet.GetRootSector().transform)
                {
                    bool childMatchesDetail = false; // TODO: this
                    
                    if (childMatchesDetail)
                    {
                        RegisterProp(detail.path, child.gameObject);
                    }
                }        
            }
        }

        public void RegisterProp(string bodyGameObjectName, GameObject prop)
        {
            RegisterProp_WithReturn(bodyGameObjectName, prop);
        }

        private PropPlacementData RegisterProp_WithReturn(string bodyGameObjectName, GameObject prop)
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

        public void PrintConfigs()
        {
            foreach(string configFile in GenerateConfigs())
            {
                Logger.Log(configFile);
            }
        }

        public List<String> GenerateConfigs()
        {
            var groupedProps = props
                .GroupBy(p => p.body)
                .Select(grp => grp.ToList())
                .ToList();
            
            List<string> configFiles = new List<string>();

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

                configFiles.Add(configFile);
            }

            return configFiles;
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
