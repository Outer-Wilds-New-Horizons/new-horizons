
using NewHorizons.Utility;
using System.ComponentModel;
using Newtonsoft.Json;

namespace NewHorizons.External.Props
{
    [JsonObject]
    public class DetailInfo : GeneralPropInfo
    {

        /// <summary>
        /// Do we override rotation and try to automatically align this object to stand upright on the body's surface?
        /// </summary>
        public bool alignToNormal;

        /// <summary>
        /// Relative filepath to an asset-bundle to load the prefab defined in `path` from
        /// </summary>
        public string assetBundle;

        /// <summary>
        /// Either the path in the scene hierarchy of the item to copy or the path to the object in the supplied asset bundle
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

        /// <summary>
        /// If this value is not null, this prop will be quantum. Assign this field to the id of the quantum group it should be a part of. The group it is assigned to determines what kind of quantum object it is
        /// </summary>
        public string quantumGroupID;

        /// <summary>
        /// Should this detail stay loaded even if you're outside the sector (good for very large props)
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
    }
}
