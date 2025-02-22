using NewHorizons.External.SerializableData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NewHorizons.External.Modules.Props.Item
{
    [JsonObject]
    public class ItemInfo
    {
        /// <summary>
        /// The name of the item to be displayed in the UI. Defaults to the name of the detail object.
        /// </summary>
        public string name;
        /// <summary>
        /// The type of the item, which determines its orientation when held and what sockets it fits into. This can be a custom string, or a vanilla ItemType (Scroll, WarpCore, SharedStone, ConversationStone, Lantern, SlideReel, DreamLantern, or VisionTorch). Defaults to the item name.
        /// </summary>
        public string itemType;
        /// <summary>
        /// The furthest distance where the player can interact with this item. Defaults to two meters, same as most vanilla items. Set this to zero to disable all interaction by default.
        /// </summary>
        [DefaultValue(2f)] public float interactRange = 2f;
        /// <summary>
        /// The radius that the added sphere collider will use for collision and hover detection.
        /// If there's already a collider on the detail, you can make this 0.
        /// </summary>
        [DefaultValue(0.5f)] public float colliderRadius = 0.5f;
        /// <summary>
        /// Whether the added sphere collider will be a trigger (interactible but does not collide). Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool colliderIsTrigger = true;
        /// <summary>
        /// Whether the item can be dropped. Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool droppable = true;
        /// <summary>
        /// A relative offset to apply to the item's position when dropping it on the ground.
        /// </summary>
        public MVector3 dropOffset;
        /// <summary>
        /// The direction the item will be oriented when dropping it on the ground. Defaults to up (0, 1, 0).
        /// </summary>
        public MVector3 dropNormal;
        /// <summary>
        /// A relative offset to apply to the item's position when holding it. The initial position varies for vanilla item types.
        /// </summary>
        public MVector3 holdOffset;
        /// <summary>
        /// A relative offset to apply to the item's rotation when holding it.
        /// </summary>
        public MVector3 holdRotation;
        /// <summary>
        /// A relative offset to apply to the item's position when placing it into a socket.
        /// </summary>
        public MVector3 socketOffset;
        /// <summary>
        /// A relative offset to apply to the item's rotation when placing it into a socket.
        /// </summary>
        public MVector3 socketRotation;
        /// <summary>
        /// The audio to play when this item is picked up. Only applies to custom/non-vanilla item types.
        /// Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// Defaults to "ToolItemWarpCorePickUp". Set to "None" to disable the sound entirely.
        /// </summary>
        public string pickupAudio;
        /// <summary>
        /// The audio to play when this item is dropped. Only applies to custom/non-vanilla item types.
        /// Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// Defaults to "ToolItemWarpCoreDrop". Set to "None" to disable the sound entirely.
        /// </summary>
        public string dropAudio;
        /// <summary>
        /// The audio to play when this item is inserted into a socket. Only applies to custom/non-vanilla item types.
        /// Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// Defaults to the pickup audio. Set to "None" to disable the sound entirely.
        /// </summary>
        public string socketAudio;
        /// <summary>
        /// The audio to play when this item is removed from a socket. Only applies to custom/non-vanilla item types.
        /// Can be a path to a .wav/.ogg/.mp3 file, or taken from the AudioClip list.
        /// Defaults to the drop audio. Set to "None" to disable the sound entirely.
        /// </summary>
        public string unsocketAudio;
        /// <summary>
        /// A dialogue condition to set when picking up this item.
        /// </summary>
        public string pickupCondition;
        /// <summary>
        /// Whether the pickup condition should be cleared when dropping the item. Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool clearPickupConditionOnDrop = true;
        /// <summary>
        /// A ship log fact to reveal when picking up this item.
        /// </summary>
        public string pickupFact;
        /// <summary>
        /// A relative path from the planet to a socket that this item will be automatically inserted into.
        /// </summary>
        public string pathToInitialSocket;
    }
}
