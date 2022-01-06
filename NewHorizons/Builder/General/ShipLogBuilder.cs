using NewHorizons.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.General
{
    public static class ShipLogBuilder
    {
        public static ShipLogStarChartMode ShipLogStarChartMode;

        public static void Init()
        {
            Logger.Log("Fuck");
            var shipLogRoot = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
            Logger.Log("Fuck");
            var starChartLog = new GameObject("StarChartMode");
            starChartLog.SetActive(false);
            starChartLog.transform.parent = shipLogRoot.transform;
            starChartLog.transform.localScale = Vector3.one * 0.5f;
            Logger.Log("Fuck");
            ShipLogStarChartMode = starChartLog.AddComponent<ShipLogStarChartMode>();
            Logger.Log("Fuck");
            var reticleImage = GameObject.Instantiate(GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ReticleImage (1)/"), starChartLog.transform);
            Logger.Log("Fuck");
            var scaleRoot = new GameObject("ScaleRoot");
            scaleRoot.transform.parent = starChartLog.transform;
            scaleRoot.transform.localScale = Vector3.one;
            Logger.Log("Fuck");
            var panRoot = new GameObject("PanRoot");
            panRoot.transform.parent = scaleRoot.transform;
            panRoot.transform.localScale = Vector3.one;
            Logger.Log("Fuck");
            CreateCard("Test", panRoot.transform);
        }

        public static GameObject _cardTemplate = null;

        public static void CreateCard(string name, Transform parent)
        {
            if(_cardTemplate == null)
            {
                Logger.Log("Fuck");
                var panRoot = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ScaleRoot/PanRoot");
                Logger.Log("Fuck");
                _cardTemplate = GameObject.Instantiate(panRoot.GetComponentInChildren<ShipLogEntryCard>().gameObject);
                Logger.Log("Fuck");
                _cardTemplate.SetActive(false);
            }

            var newCard = GameObject.Instantiate(_cardTemplate, parent);
            Logger.Log("Fuck");
            newCard.transform.Find("EntryCardRoot/NameBackground/Name").GetComponent<UnityEngine.UI.Text>().text = name;
        }
    }
}
