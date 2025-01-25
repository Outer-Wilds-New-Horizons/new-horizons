using NewHorizons.External.Modules.Props.Item;
using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props
{
    [JsonObject]
    public class SimplifiedDetailInfo : GeneralPropInfo
    {
        public SimplifiedDetailInfo() { }

        public SimplifiedDetailInfo(GeneralPointPropInfo info)
        {
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(info), this);
        }

        /// <summary>
        /// Relative filepath to an asset-bundle to load the prefab defined in `path` from
        /// </summary>
        public string assetBundle;

        /// <summary>
        /// Either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle. 
        /// If empty, will make an empty game object. This can be useful for adding other props to it as its children.
        /// </summary>
        public string path;

        /// <summary>
        /// A list of children to remove from this detail
        /// </summary>
        public string[] removeChildren;

        /// <summary>
        /// Do we reset all the components on this object? Useful for certain props that have dialogue components attached to
        /// them.
        /// </summary>
        public bool removeComponents;

        /// <summary>
        /// Scale the prop
        /// </summary>
        [DefaultValue(1f)] public float scale = 1f;

        /// <summary>
        /// Scale each axis of the prop. Overrides `scale`.
        /// </summary>
        public MVector3 stretch;
    }

    [JsonObject]
    public class DetailInfo : SimplifiedDetailInfo
    {
        public DetailInfo() { }

        public DetailInfo(GeneralPointPropInfo info)
        {
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(info), this);
        }

        public DetailInfo(SimplifiedDetailInfo info)
        {
            JsonConvert.PopulateObject(JsonConvert.SerializeObject(info), this);
        }

        [Obsolete("Use QuantumDetailInfo")]
        public string quantumGroupID;

        /// <summary>
        /// Should this detail stay loaded (visible and collideable) even if you're outside the sector (good for very large props)?
        /// Also makes this detail visible on the map.
        /// Keeping many props loaded is bad for performance so use this only when it's actually relevant
        /// Most logic/behavior scripts will still only work inside the sector, as most of those scripts break if a sector is not provided.
        /// </summary>
        public bool keepLoaded;

        /// <summary>
        /// Should this object dynamically move around?
        /// This tries to make all mesh colliders convex, as well as adding a sphere collider in case the detail has no others.
        /// </summary>
        public bool hasPhysics;

        /// <summary>
        /// The mass of the physics object.
        /// Most pushable props use the default value, which matches the player mass.
        /// </summary>
        [DefaultValue(0.001f)] public float physicsMass = 0.001f;

        /// <summary>
        /// The radius that the added sphere collider will use for physics collision.
        /// If there's already good colliders on the detail, you can make this 0.
        /// </summary>
        [DefaultValue(1f)] public float physicsRadius = 1f;

        /// <summary>
        /// If true, this detail will stay still until it touches something.
        /// Good for zero-g props.
        /// </summary>
        [DefaultValue(false)] public bool physicsSuspendUntilImpact = false;

        /// <summary>
        /// Set to true if this object's lighting should ignore the effects of sunlight
        /// </summary>
        public bool ignoreSun;

        /// <summary>
        /// Activates this game object when the dialogue condition is met
        /// </summary>
        public string activationCondition;

        /// <summary>
        /// Deactivates this game object when the dialogue condition is met
        /// </summary>
        public string deactivationCondition;

        /// <summary>
        /// Should the player close their eyes while the activation state changes. Only relevant if activationCondition or deactivationCondition are set.
        /// </summary>
        [DefaultValue(true)] public bool blinkWhenActiveChanged = true;

        /// <summary>
        /// Should this detail be treated as an interactible item
        /// </summary>
        public ItemInfo item;

        /// <summary>
        /// Should this detail be treated as a socket for an interactible item
        /// </summary>
        public ItemSocketInfo itemSocket;

        [Obsolete("alignToNormal is deprecated. Use alignRadial instead")] public bool alignToNormal;
    }

}
