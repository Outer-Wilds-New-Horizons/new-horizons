using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.EchoesOfTheEye
{
    [JsonObject]
    public class GrappleTotemInfo : GeneralPropInfo
    {
        /// <summary>
        /// The minimum distance that the player must be from the grapple totem for it to activate.
        /// </summary>
        [DefaultValue(10f)] public float minDistance = 10f;

        /// <summary>
        /// The distance from the grapple totem that the player will stop at when it activates.
        /// </summary>
        [DefaultValue(4f)] public float arrivalDistance = 4f;

        /// <summary>
        /// The maximum angle in degrees allowed between the grapple totem's face and the player's lantern in order to activate the totem.
        /// </summary>
        [DefaultValue(45f)] public float maxAngle = 45f;

        /// <summary>
        /// The maximum distance allowed between the grapple totem's face and the player's lantern in order to activate the totem.
        /// </summary>
        [DefaultValue(29f)] public float maxDistance = 29f;

        /// <summary>
        /// Allows the grapple totem to be activated by the player's flashlight (when placed outside of the dream world). The player must still be holding an artifact, but it can be unlit.
        /// </summary>
        public bool allowFlashlight;
    }
}
