using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Marshmallow.Utility
{
    public static class MarshmallowExtensions
    {
        public static Vector3 ToVector3(this JArray ja)
        {
            var array = (JArray)ja;
            var items = array.Select(jv => (float)jv).ToArray();
            var output = new Vector3(items[0], items[1], items[2]);

            return output;
        }

        public static Color32 ToColor32(this JArray ja)
        {
            var array = (JArray)ja;
            var items = array.Select(jv => (byte)jv).ToArray();
            var output = new Color32(items[0], items[1], items[2], items[3]);

            return output;
        }
    }
}
