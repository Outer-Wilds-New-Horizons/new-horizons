using NewHorizons.Components.Props;
using NewHorizons.External.Modules.Props.Item;
using NewHorizons.Handlers;
using NewHorizons.Utility.OWML;
using OWML.Common;
using OWML.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NewHorizons.Builder.Props
{
    public static class ItemBuilder
    {
        private static Dictionary<string, ItemType> _itemTypes;

        internal static void Init()
        {
            if (_itemTypes != null)
            {
                foreach (var value in _itemTypes.Values)
                {
                    EnumUtils.Remove<ItemType>(value);
                }
            }
            _itemTypes = new Dictionary<string, ItemType>();
        }

        public static NHItem MakeItem(GameObject go, GameObject planetGO, Sector sector, ItemInfo info, IModBehaviour mod)
        {
            var itemName = info.name;
            if (string.IsNullOrEmpty(itemName))
            {
                itemName = go.name;
            }

            var itemTypeName = info.itemType;
            if (string.IsNullOrEmpty(itemTypeName))
            {
                itemTypeName = itemName;
            }

            var itemType = GetOrCreateItemType(itemTypeName);

            var item = go.GetAddComponent<NHItem>();
            item._sector = sector;
            item._interactable = info.interactRange > 0f;
            item._interactRange = info.interactRange;
            item._localDropOffset = info.dropOffset ?? Vector3.zero;
            item._localDropNormal = info.dropNormal ?? Vector3.up;

            item.DisplayName = itemName;
            item.ItemType = itemType;
            item.Droppable = info.droppable;
            item.HoldOffset = info.holdOffset ?? Vector3.zero;
            item.HoldRotation = info.holdRotation ?? Vector3.zero;
            item.SocketOffset = info.socketOffset ?? Vector3.zero;
            item.SocketRotation = info.socketRotation ?? Vector3.zero;
            if (!string.IsNullOrEmpty(info.pickupAudio))
            {
                item.PickupAudio = AudioTypeHandler.GetAudioType(info.pickupAudio, mod);
            }
            if (!string.IsNullOrEmpty(info.dropAudio))
            {
                item.DropAudio = AudioTypeHandler.GetAudioType(info.dropAudio, mod);
            }
            if (!string.IsNullOrEmpty(info.socketAudio))
            {
                item.SocketAudio = AudioTypeHandler.GetAudioType(info.socketAudio, mod);
            }
            else
            {
                item.SocketAudio = item.DropAudio;
            }
            if (!string.IsNullOrEmpty(info.unsocketAudio))
            {
                item.UnsocketAudio = AudioTypeHandler.GetAudioType(info.unsocketAudio, mod);
            }
            else
            {
                item.UnsocketAudio = item.PickupAudio;
            }
            item.PickupCondition = info.pickupCondition;
            item.ClearPickupConditionOnDrop = info.clearPickupConditionOnDrop;
            item.PickupFact = info.pickupFact;

            if (info.colliderRadius > 0f)
            {
                go.AddComponent<SphereCollider>().radius = info.colliderRadius;
                go.GetAddComponent<OWCollider>();
            }

            Delay.FireOnNextUpdate(() =>
            {
                if (item != null && !string.IsNullOrEmpty(info.pathToInitialSocket))
                {
                    var socketGO = planetGO.transform.Find(info.pathToInitialSocket);
                    if (socketGO != null)
                    {
                        var socket = socketGO.GetComponent<OWItemSocket>();
                        if (socket != null)
                        {
                            if (socket.PlaceIntoSocket(item))
                            {
                                // Successfully socketed
                            }
                            else
                            {
                                NHLogger.LogError($"Could not insert item {itemName} into socket at path {socketGO}");
                            }
                        }
                        else
                        {
                            NHLogger.LogError($"Could not find a socket to parent item {itemName} to at path {socketGO}");
                        }
                    }
                    else
                    {
                        NHLogger.LogError($"Could not find a socket to parent item {itemName} to at path {socketGO}");
                    }
                }
            });

            return item;
        }

        public static NHItemSocket MakeSocket(GameObject go, GameObject planetGO, Sector sector, ItemSocketInfo info)
        {
            var itemType = EnumUtils.TryParse(info.itemType, true, out ItemType result) ? result : ItemType.Invalid;
            if (itemType == ItemType.Invalid && !string.IsNullOrEmpty(info.itemType))
            {
                itemType = EnumUtilities.Create<ItemType>(info.itemType);
            }

            var socket = go.GetAddComponent<NHItemSocket>();
            socket._sector = sector;
            socket._interactable = info.interactRange > 0f;
            socket._interactRange = info.interactRange;

            if (!string.IsNullOrEmpty(info.socketPath))
            {
                socket._socketTransform = go.transform.Find(info.socketPath);
            }
            if (socket._socketTransform == null)
            {
                var socketGO = GeneralPropBuilder.MakeNew("Socket", planetGO, sector, info, defaultParent: go.transform);
                socketGO.SetActive(true);
                socket._socketTransform = socketGO.transform;
            }

            socket.ItemType = itemType;
            socket.UseGiveTakePrompts = info.useGiveTakePrompts;
            socket.InsertCondition = info.insertCondition;
            socket.ClearInsertConditionOnRemoval = info.clearInsertConditionOnRemoval;
            socket.InsertFact = info.insertFact;
            socket.RemovalCondition = info.removalCondition;
            socket.ClearRemovalConditionOnInsert = info.clearRemovalConditionOnInsert;
            socket.RemovalFact = info.removalFact;

            Delay.FireInNUpdates(() =>
            {
                if (socket != null && !socket._socketedItem)
                {
                    socket.TriggerRemovalConditions();
                }
            }, 2);

            return socket;
        }

        public static ItemType GetOrCreateItemType(string name)
        {
            var itemType = ItemType.Invalid;
            if (_itemTypes.ContainsKey(name))
            {
                itemType = _itemTypes[name];
            }
            else if (EnumUtils.TryParse(name, true, out ItemType result))
            {
                itemType = result;
            }
            else if (!string.IsNullOrEmpty(name))
            {
                itemType = EnumUtils.Create<ItemType>(name);
                _itemTypes.Add(name, itemType);
            }
            return itemType;
        }

        public static bool IsCustomItemType(ItemType type)
        {
            return _itemTypes.ContainsValue(type);
        }
    }
}
