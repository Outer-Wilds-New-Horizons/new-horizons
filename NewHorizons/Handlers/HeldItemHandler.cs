using HarmonyLib;
using NewHorizons.Builder.Props;
using NewHorizons.External.Modules.Props;
using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewHorizons.Handlers;

[HarmonyPatch]
public static class HeldItemHandler
{
    /// <summary>
    /// Dictionary of system name to item path
    /// If we travel to multiple systems within a single loop, this will hold the items we move between systems
    /// </summary>
    private static Dictionary<string, List<string>> _pathOfItemTakenFromSystem = new();

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

    [HarmonyPrefix, HarmonyPatch(typeof(ItemTool), nameof(ItemTool.Awake))]
    private static void Init()
    {
        _trackedPaths.Clear();

        _currentStarSystem = Main.Instance.CurrentStarSystem;

        if (!_isInitialized)
        {
            _isInitialized = true;
            Main.Instance.StarSystemChanging += OnStarSystemChanging;
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
        //go.SetActive(false);

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

    private static void OnStarSystemChanging()
    {
        if (_currentlyHeldItem != null)
        {
            // Track it so that when we return to this system we can delete the original
            if (_trackedPaths.TryGetValue(_currentlyHeldItem, out var path))
            {
                if (!_pathOfItemTakenFromSystem.ContainsKey(Main.Instance.CurrentStarSystem))
                {
                    _pathOfItemTakenFromSystem[Main.Instance.CurrentStarSystem] = new();
                }
                _pathOfItemTakenFromSystem[Main.Instance.CurrentStarSystem].Add(path);
            }

            NHLogger.Log($"Scene unloaded, preserved inactive held item {_currentlyHeldItem.name}");
            // For some reason, the original will get destroyed no matter what do we make a copy
            _currentlyHeldItem = MakePerfectCopy(_currentlyHeldItem).DontDestroyOnLoad();
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
                NHLogger.Log($"Removing item that was taken from this system at {path}");
                var item = SearchUtilities.Find(path)?.GetComponent<OWItem>();
                // Make sure to update the socket it might be in so that it works
                if (item.GetComponentInParent<OWItemSocket>() is OWItemSocket socket)
                {
                    socket.RemoveFromSocket();
                }
                GameObject.Destroy(item.gameObject);
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
}
