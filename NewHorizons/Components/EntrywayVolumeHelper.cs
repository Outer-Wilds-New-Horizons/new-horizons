using NewHorizons.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewHorizons.Components
{
    /// <summary>
    /// Helper to store a list of entryway volumes alongside existing vanilla components; the actual logic lives in <see cref="EntrywayHandler"/> and various patches
    /// </summary>
    public class EntrywayVolumeHelper : MonoBehaviour
    {
        public OWTriggerVolume[] entrywayVolumes;
    }
}
