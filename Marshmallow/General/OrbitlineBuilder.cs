using Marshmallow.Utility;
using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Logger = Marshmallow.Utility.Logger;

namespace Marshmallow.General
{
    static class OrbitlineBuilder
    {
        public static void Make(GameObject body, AstroObject astroobject)
        {
            GameObject orbit = new GameObject();
            orbit.transform.parent = body.transform;

            var LR = orbit.AddComponent<LineRenderer>();
            //LR.material = GameObject.Find("OrbitLine_TH").GetComponent<LineRenderer>().material;
            LR.useWorldSpace = false;
            LR.loop = false;

            Logger.Log("AO primary body is " + astroobject.GetPrimaryBody().name, Logger.LogType.Log);

            var ol = orbit.AddComponent<OrbitLine>();
            ol.SetValue("_astroObject", astroobject);
            ol.SetValue("_fade", false);
            ol.SetValue("_lineWidth", 5);

            Logger.Log("Finished building orbit line", Logger.LogType.Log);
        }
    }
}
