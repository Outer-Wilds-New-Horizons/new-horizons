using NewHorizons.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Handlers
{
    public static class StarChartHandler
    {
        public static ShipLogStarChartMode ShipLogStarChartMode;

        public static void Init()
        {
            var shipLogRoot = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas");

            var starChartLog = new GameObject("StarChartMode");
            starChartLog.SetActive(false);
            starChartLog.transform.parent = shipLogRoot.transform;
            starChartLog.transform.localScale = Vector3.one * 1f;
            starChartLog.transform.localPosition = Vector3.zero;
            starChartLog.transform.localRotation = Quaternion.Euler(0, 0, 0);

            ShipLogStarChartMode = starChartLog.AddComponent<ShipLogStarChartMode>();

            var reticleImage = GameObject.Instantiate(GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/ShipLogPivot/ShipLogCanvas/DetectiveMode/ReticleImage (1)/"), starChartLog.transform);

            var scaleRoot = new GameObject("ScaleRoot");
            scaleRoot.transform.parent = starChartLog.transform;
            scaleRoot.transform.localScale = Vector3.one;
            scaleRoot.transform.localPosition = Vector3.zero;
            scaleRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);

            var panRoot = new GameObject("PanRoot");
            panRoot.transform.parent = scaleRoot.transform;
            panRoot.transform.localScale = Vector3.one;
            panRoot.transform.localPosition = Vector3.zero;
            panRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);

            var centerPromptList = shipLogRoot.transform.Find("ScreenPromptListScaleRoot/ScreenPromptList_Center")?.GetComponent<ScreenPromptList>();
            var upperRightPromptList = shipLogRoot.transform.Find("ScreenPromptListScaleRoot/ScreenPromptList_UpperRight")?.GetComponent<ScreenPromptList>();
            var oneShotSource = GameObject.Find("Ship_Body/Module_Cabin/Systems_Cabin/ShipLogPivot/ShipLog/OneShotAudio_ShipLog")?.GetComponent<OWAudioSource>();

            ShipLogStarChartMode.Initialize(
                centerPromptList,
                upperRightPromptList,
                oneShotSource);
        }
    }
}
