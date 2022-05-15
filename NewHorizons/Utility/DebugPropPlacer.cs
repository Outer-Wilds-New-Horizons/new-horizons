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
            public GameObject gameObject;
            public DetailInfo detailInfo;
        }

        public static readonly string DEFAULT_OBJECT = "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District1/Props_HangingCity_District1/OtherComponentsGroup/Props_HangingCity_Building_10/Prefab_NOM_VaseThin";

        public string currentObject { get; private set; }
        private bool hasAddedCurrentObjectToRecentsList = false;
        private List<PropPlacementData> props = new List<PropPlacementData>();
        private List<PropPlacementData> deletedProps = new List<PropPlacementData>();
        private DebugRaycaster _rc;

        public static HashSet<string> RecentlyPlacedProps = new HashSet<string>();

        public static bool active = false;

        private void Awake()
        {
            _rc = this.GetRequiredComponent<DebugRaycaster>();
            currentObject = DEFAULT_OBJECT;
        }

        private void Update()
        {
            if (!Main.Debug) return;
            if (!active) return;

            if (Keyboard.current[Key.X].wasReleasedThisFrame)
            {
                PlaceObject();
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
            // TODO: implement sectors
            // if this hits a sector, store that sector and add a config file option for it

            if (!data.hitObject.name.EndsWith("_Body"))
            {
                Logger.Log("Cannot place object on non-body object: " + data.hitObject.name);
            }

            try 
            { 
                if (currentObject == "" || currentObject == null)
                {
                    SetCurrentObject(DEFAULT_OBJECT);
                }

                GameObject prop = DetailBuilder.MakeDetail(data.hitObject, data.hitObject.GetComponentInChildren<Sector>(), currentObject, data.pos, data.norm, 1, false);
                PropPlacementData propData = RegisterProp_WithReturn(data.bodyName, prop);
                
                // align with surface normal
                Vector3 alignToSurface = (Quaternion.LookRotation(data.norm) * Quaternion.FromToRotation(Vector3.up, Vector3.forward)).eulerAngles;
                prop.transform.localEulerAngles = alignToSurface;     
        
                // rotate facing dir towards player
                GameObject g = new GameObject();
                g.transform.parent = prop.transform.parent;
                g.transform.localPosition = prop.transform.localPosition;
                g.transform.localRotation = prop.transform.localRotation;
                
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

        public static string GetAstroObjectName(string bodyName)
        {
            if (bodyName.EndsWith("_Body")) bodyName = bodyName.Substring(0, bodyName.Length-"_Body".Length);

            var astroObject = AstroObjectLocator.GetAstroObject(bodyName);
            if (astroObject == null) return null;

            var astroObjectName = astroObject.name;
            if (astroObjectName.EndsWith("_Body")) astroObjectName = astroObjectName.Substring(0, astroObjectName.Length-"_Body".Length);

            return astroObjectName;
        }

        public void FindAndRegisterPropsFromConfig(IPlanetConfig config)
        {
            if (config.StarSystem != Main.Instance.CurrentStarSystem) return;

            AstroObject planet = AstroObjectLocator.GetAstroObject(config.Name);

            if (planet == null) return;
            if (config.Props == null || config.Props.Details == null) return;

            var astroObjectName = GetAstroObjectName(config.Name);

            foreach (var detail in config.Props.Details)
            {
                GameObject spawnedProp = DetailBuilder.GetSpawnedGameObjectByDetailInfo(detail);

                if (spawnedProp == null)
                {
                    Logger.LogError("No spawned prop found for " + detail.path);
                    continue;
                }

                PropPlacementData data = RegisterProp_WithReturn(astroObjectName, spawnedProp, detail.path, detail);

                if (!RecentlyPlacedProps.Contains(data.detailInfo.path))
                {
                    RecentlyPlacedProps.Add(data.detailInfo.path);
                }
            }
        }

        public void RegisterProp(string bodyGameObjectName, GameObject prop)
        {
            RegisterProp_WithReturn(bodyGameObjectName, prop);
        }

        private PropPlacementData RegisterProp_WithReturn(string bodyGameObjectName, GameObject prop, string propPath = null, DetailInfo detailInfo = null)
        {
            if (Main.Debug)
            {
                // TOOD: make this prop an item
            }


            string bodyName = GetAstroObjectName(bodyGameObjectName);
            
            Logger.Log("Adding prop to " + Main.Instance.CurrentStarSystem + "::" + bodyName);
            

            detailInfo = detailInfo == null ? new DetailInfo() : detailInfo;
            detailInfo.path = propPath == null ? currentObject : propPath;

            PropPlacementData data = new PropPlacementData
            {
                body = bodyName,
                gameObject = prop,
                system = Main.Instance.CurrentStarSystem,
                detailInfo = detailInfo
            };

            props.Add(data);
            return data;
        }

        public Dictionary<string, DetailInfo[]> GetPropsConfigByBody()
        {
            var groupedProps = props
                .GroupBy(p => p.system + "." + p.body)
                .Select(grp => grp.ToList())
                .ToList();
            
            Dictionary<string, DetailInfo[]> propConfigs = new Dictionary<string, DetailInfo[]>();

            foreach (List<PropPlacementData> bodyProps in groupedProps)
            {
                if (bodyProps == null || bodyProps.Count == 0) continue; 
                Logger.Log("getting prop group for body " + bodyProps[0].body);
                if (AstroObjectLocator.GetAstroObject(bodyProps[0].body) == null) continue;
                string bodyName = GetAstroObjectName(bodyProps[0].body);

                DetailInfo[] infoArray = new DetailInfo[bodyProps.Count];
                propConfigs[bodyName] = infoArray;
        
                for(int i = 0; i < bodyProps.Count; i++)
                {
                    bodyProps[i].detailInfo.position = bodyProps[i].gameObject.transform.localPosition;
                    bodyProps[i].detailInfo.rotation = bodyProps[i].gameObject.transform.localEulerAngles;
                    bodyProps[i].detailInfo.scale = bodyProps[i].gameObject.transform.localScale.x;

                    infoArray[i] = bodyProps[i].detailInfo;
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
