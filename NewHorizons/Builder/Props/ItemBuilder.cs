using NewHorizons.Components.Props;
using NewHorizons.External.Modules.Props.Item;
using NewHorizons.Utility.OWML;
using OWML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static NHItem MakeItem(GameObject go, GameObject planetGO, Sector sector, ItemInfo info)
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
            item.PickupCondition = info.pickupCondition;
            item.ClearPickupConditionOnDrop = info.clearPickupConditionOnDrop;

            Delay.FireInNUpdates(() =>
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
            }, 1);

            return item;
        }

        public static NHItemSocket MakeSocket(GameObject go, GameObject planetGO, Sector sector, ItemSocketInfo info)
        {

            var socketGO = GeneralPropBuilder.MakeNew("Socket", planetGO, sector, info, defaultParent: go.transform);

            var itemType = EnumUtils.TryParse(info.itemType, true, out ItemType result) ? result : ItemType.Invalid;
            if (itemType == ItemType.Invalid && !string.IsNullOrEmpty(info.itemType))
            {
                itemType = EnumUtilities.Create<ItemType>(info.itemType);
            }

            var socket = socketGO.GetAddComponent<NHItemSocket>();
            socket._sector = sector;
            socket._interactable = info.interactRange > 0f;
            socket._interactRange = info.interactRange;

            if (!string.IsNullOrEmpty(info.socketPath))
            {
                socket._socketTransform = go.transform.Find(info.socketPath);
            }
            if (socket._socketTransform == null)
            {
                socket._socketTransform = socket.transform;
            }

            socket.ItemType = itemType;
            socket.UseGiveTakePrompts = info.useGiveTakePrompts;
            socket.InsertCondition = info.insertCondition;
            socket.ClearInsertConditionOnRemoval = info.clearInsertConditionOnRemoval;
            socket.RemovalCondition = info.removalCondition;
            socket.ClearRemovalConditionOnInsert = info.clearRemovalConditionOnInsert;

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
    }
}
