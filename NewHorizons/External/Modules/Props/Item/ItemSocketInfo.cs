using Newtonsoft.Json;
using System.ComponentModel;

namespace NewHorizons.External.Modules.Props.Item
{
    [JsonObject]
    public class ItemSocketInfo : GeneralPropInfo
    {
        /// <summary>
        /// The relative path to a child game object of this detail that will act as the socket point for the item. Will be used instead of this socket's positioning info if set.
        /// </summary>
        public string socketPath;
        /// <summary>
        /// The type of item allowed in this socket. This can be a custom string, or a vanilla ItemType (Scroll, WarpCode, SharedStone, ConversationStone, Lantern, SlideReel, DreamLantern, or VisionTorch).
        /// </summary>
        public string itemType;
        /// <summary>
        /// The furthest distance where the player can interact with this item socket. Defaults to two meters, same as most vanilla item sockets. Set this to zero to disable all interaction by default.
        /// </summary>
        [DefaultValue(2f)] public float interactRange = 2f;
        /// <summary>
        /// Whether to use "Give Item" / "Take Item" prompts instead of "Insert Item" / "Remove Item".
        /// </summary>
        public bool useGiveTakePrompts;
        /// <summary>
        /// A dialogue condition to set when inserting an item into this socket.
        /// </summary>
        public string insertCondition;
        /// <summary>
        /// Whether the insert condition should be cleared when removing the socketed item. Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool clearInsertConditionOnRemoval = true;
        /// <summary>
        /// A ship log fact to reveal when inserting an item into this socket.
        /// </summary>
        public string insertFact;
        /// <summary>
        /// A dialogue condition to set when removing an item from this socket, or when the socket is empty.
        /// </summary>
        public string removalCondition;
        /// <summary>
        /// Whether the removal condition should be cleared when inserting a socketed item. Defaults to true.
        /// </summary>
        [DefaultValue(true)] public bool clearRemovalConditionOnInsert = true;
        /// <summary>
        /// A ship log fact to reveal when removing an item from this socket, or when the socket is empty.
        /// </summary>
        public string removalFact;
        /// <summary>
        /// Default collider radius when interacting with the socket
        /// </summary>
        [DefaultValue(0f)]
        public float colliderRadius = 0f;
    }
}
