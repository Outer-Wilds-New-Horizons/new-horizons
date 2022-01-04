using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Builder.General
{
    public static class ShipLogBuilder
    {
        public static ShipLogDetectiveMode StarChartMode;

        public static void Init()
        {
            /*
            var shipLogRoot = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");
            var starChartLog = GameObject.Instantiate(shipLogRoot.transform.Find("DetectiveMode"), shipLogRoot.transform);
            starChartLog.transform.name = "StarChartMode";

            var cardRoot = starChartLog.transform.Find("ScaleRoot").Find("PanRoot");
            foreach(Transform child in cardRoot)
            {
                GameObject.Destroy(child.gameObject);
            }

            var cardPrefab = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ScaleRoot/PanRoot/TH_VILLAGE");

            var detectiveMode = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/");
            var mapMode = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/MapMode/");

            StarChartMode = starChartLog.GetComponent<ShipLogDetectiveMode>();

            StarChartMode._cardDict = new Dictionary<string, ShipLogEntryCard>();
            StarChartMode._cardList = new List<ShipLogEntryCard>();
            StarChartMode._centerPromptList = detectiveMode.GetComponent<ShipLogDetectiveMode>()._centerPromptList;
            */
        }
    }
}
