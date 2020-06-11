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
    static class MakeOrbitLine
    {
        public static void Make(GameObject body, AstroObject astroobject)
        {
            Logger.Log("MakeOrbitLine not finished!", Logger.LogType.Todo);

            GameObject orbit = new GameObject();
            orbit.transform.parent = body.transform;

            orbit.AddComponent<LineRenderer>();

            var ol = orbit.AddComponent<OrbitLine>();
            ol.SetValue("_astroObject", astroobject);
            ol.SetValue("_fade", false);

            var lr = orbit.AddComponent<LineRenderer>();
        }
    }
}
