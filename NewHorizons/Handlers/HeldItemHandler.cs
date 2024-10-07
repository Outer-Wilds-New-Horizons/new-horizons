using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Props;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NewHorizons.Handlers;

[HarmonyPatch]
public static class HeldItemHandler
{
    /// <summary>
    /// Dictionary of system name to item path
    /// If we travel to multiple systems within a single loop, this will hold the items we move between systems
    /// </summary>
    private static Dictionary<string, HashSet<string>> _pathOfItemTakenFromSystem = new();

    public static bool WasAWCTakenFromATP => _pathOfItemTakenFromSystem.TryGetValue("SolarSystem", out var list) && list.Contains(ADVANCED_WARP_CORE);

    private static GameObject _currentlyHeldItem;

    /// <summary>
    /// Track the path of any item we ever pick up, in case we take it out of the system
    /// </summary>
    private static Dictionary<GameObject, string> _trackedPaths = new();

    private static bool _isInitialized = false;
    private static bool _isSystemReady = false;

    /// <summary>
    /// We keep our own reference to this because when Unload gets called it might have already been updated
    /// </summary>
    private static string _currentStarSystem;

    private const string ADVANCED_WARP_CORE = "TowerTwin_Body/Sector_TowerTwin/Sector_TimeLoopInterior/Interactables_TimeLoopInterior/WarpCoreSocket/Prefab_NOM_WarpCoreVessel";

    [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.Awake))]
    private static void Init()
    {
        _trackedPaths.Clear();

        _currentStarSystem = Main.Instance.CurrentStarSystem;

        if (!_isInitialized)
        {
            _isInitialized = true;
            Main.Instance.OnChangeStarSystem.AddListener(OnStarSystemChanging);
            Main.Instance.OnStarSystemLoaded.AddListener(OnSystemReady);
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", OnPlayerDeath);
        }
    }

    private static void OnPlayerDeath(DeathType _)
    {
        NHLogger.Log("Player died, resetting held items");

        // Destroy everything
        _pathOfItemTakenFromSystem.Clear();
        GameObject.Destroy(_currentlyHeldItem);
    }

    private static GameObject MakePerfectCopy(GameObject go)
    {
        var owItem = go.GetComponent<OWItem>();

        var tempParent = new GameObject();
        tempParent.transform.position = new Vector3(100000, 0, 0);
        var newObject = DetailBuilder.Make(tempParent, tempParent.AddComponent<Sector>(), null, go, new DetailInfo() { keepLoaded = true });
        newObject.SetActive(false);
        newObject.transform.parent = null;
        newObject.name = go.name;

        var newOWItem = newObject.GetComponent<OWItem>();

        NHLogger.Log($"Cloned {go.name}, original item has component: [{owItem != null}] new item has component: [{newOWItem != null}]");

        return newObject;
    }

    private static void TrackPath(string path)
    {
        if (!_pathOfItemTakenFromSystem.ContainsKey(Main.Instance.CurrentStarSystem))
        {
            _pathOfItemTakenFromSystem[Main.Instance.CurrentStarSystem] = new();
        }
        _pathOfItemTakenFromSystem[Main.Instance.CurrentStarSystem].Add(path);
    }

    private static void OnStarSystemChanging(string _)
    {
        if (_currentlyHeldItem != null)
        {
            // Track it so that when we return to this system we can delete the original
            if (_trackedPaths.TryGetValue(_currentlyHeldItem, out var path))
            {
                TrackPath(path);
            }

            NHLogger.Log($"Scene unloaded, preserved inactive held item {_currentlyHeldItem.name}");
            // For some reason, the original will get destroyed no matter what we do. To avoid, we make a copy
            _currentlyHeldItem = MakePerfectCopy(_currentlyHeldItem).DontDestroyOnLoad();
        }

        // If warping with a vessel, make sure to also track the path to the advanced warp core (assuming the player actually removed it)
        if (Main.Instance.CurrentStarSystem == "SolarSystem" && Main.Instance.IsWarpingFromVessel)
        {
            // Making sure its actually gone
            var warpCoreSocket = GameObject.FindObjectOfType<TimeLoopCoreController>()._warpCoreSocket;
            if (!warpCoreSocket.IsSocketOccupied() || warpCoreSocket.GetWarpCoreType() != WarpCoreType.Vessel)
            {
                TrackPath(ADVANCED_WARP_CORE);
            }
        }

        _trackedPaths.Clear();
        _isSystemReady = false;
    }

    private static void OnSystemReady(string _)
    {
        // If something was taken from this system during this life, remove it
        if (_pathOfItemTakenFromSystem.TryGetValue(Main.Instance.CurrentStarSystem, out var paths))
        {
            foreach (var path in paths)
            {
                // Have to wait two frames for the sockets to Awake and Start
                Delay.FireInNUpdates(() =>
                {
                    try
                    {
                        NHLogger.Log($"Removing item that was taken from this system at {path}");
                        var item = SearchUtilities.Find(path)?.GetComponent<OWItem>();
                        // Make sure to update the socket it might be in so that it works
                        if (item.GetComponentInParent<OWItemSocket>() is OWItemSocket socket)
                        {
                            socket.RemoveFromSocket();
                            // Time loop core controller doesn't have time to hook up its events yet so we call this manually
                            if (path == ADVANCED_WARP_CORE)
                            {
                                var controller = GameObject.FindObjectOfType<TimeLoopCoreController>();
                                controller.OpenCore();
                                controller.OnSocketableRemoved(item);
                            }
                            NHLogger.Log($"Unsocketed {item.name}");
                        }
                        item.gameObject.SetActive(false);
                    }
                    catch (Exception e)
                    {
                        NHLogger.LogError($"Failed to remove item at {path}: {e}");
                    }
                }, 2);
            }
        }

        // Give whatever item we were previously holding
        if (_currentlyHeldItem != null)
        {
            NHLogger.Log($"Giving player held item {_currentlyHeldItem.name}");
            // Else its spawning the item inside the player and for that one frame it kills you
            var newObject = MakePerfectCopy(_currentlyHeldItem);

            // We wait a bit because at some point after not something resets your held item to nothing
            Delay.FireInNUpdates(() => 
            {
                try
                {
                    Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(newObject.GetComponent<OWItem>());
                    // For some reason picking something up messes up the input mode
                    if (PlayerState.AtFlightConsole())
                    {
                        Locator.GetToolModeSwapper().UnequipTool();
                        Locator.GetToolModeSwapper().OnEnterFlightConsole(Locator.GetShipBody());
                    }
                    newObject.SetActive(true);
                }
                catch(Exception e)
                {
                    NHLogger.LogError($"Failed to take item {newObject.name} to a new system: {e}");
                    GameObject.Destroy(_currentlyHeldItem);
                    _currentlyHeldItem = null;
                }

            }, 5);
        }

        _isSystemReady = true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.MoveItemToCarrySocket))]
    private static void HeldItemChanged(ItemTool __instance, OWItem item)
    {
        if (!_isSystemReady)
        {
            return;
        }

        if (item != null)
        {
            var path = item.transform.GetPath();
            if (!_trackedPaths.ContainsKey(item.gameObject))
            {
                _trackedPaths[item.gameObject] = path;
            }
        }

        NHLogger.Log($"Player is now holding {item?.name ?? "nothing"}");
        _currentlyHeldItem = item?.gameObject;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ItemTool))]
    [HarmonyPatch(nameof(ItemTool.SocketItem))]
    [HarmonyPatch(nameof(ItemTool.DropItem))]
    [HarmonyPatch(nameof(ItemTool.StartUnsocketItem))]
    private static void HeldItemChanged2(ItemTool __instance)
    {
        if (!_isSystemReady)
        {
            return;
        }

        NHLogger.Log($"Player is now holding nothing");
        _currentlyHeldItem = null;
    }
}
