---
title: API
description: A guide on using New Horizons' API
---

## How to use the API

First create the following interface in your mod:

```cs
public interface INewHorizons
{
    [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
    void Create(Dictionary<string, object> config);

    [Obsolete("Create(Dictionary<string, object> config) is deprecated, please use LoadConfigs(IModBehaviour mod) instead")]
    void Create(Dictionary<string, object> config, IModBehaviour mod);

    /// <summary>
    /// Will load all configs in the regular folders (planets, systems, translations, etc) for this mod.
    /// The NH addon config template is just a single call to this API method.
    /// </summary>
    void LoadConfigs(IModBehaviour mod);

    /// <summary>
    /// Retrieve the root GameObject of a custom planet made by creating configs. 
    /// Will only work if the planet has been created (see GetStarSystemLoadedEvent)
    /// </summary>
    GameObject GetPlanet(string name);

    /// <summary>
    /// The name of the current star system loaded.
    /// </summary>
    string GetCurrentStarSystem();

    /// <summary>
    /// An event invoked when the player begins loading the new star system, before the scene starts to load.
    /// Gives the name of the star system being switched to.
    /// </summary>
    UnityEvent<string> GetChangeStarSystemEvent();

    /// <summary>
    /// An event invoked when NH has finished generating all planets for a new star system.
    /// Gives the name of the star system that was just loaded.
    /// </summary>
    UnityEvent<string> GetStarSystemLoadedEvent();

    /// <summary>
    /// An event invoked when NH has finished a planet for a star system.
    /// Gives the name of the planet that was just loaded.
    /// </summary>
    UnityEvent<string> GetBodyLoadedEvent();

    /// <summary>
    /// Uses JSONPath to query a body
    /// </summary>
    object QueryBody(Type outType, string bodyName, string path);

    ///<summary>
    /// Uses JSONPath to query a body
    /// </summary>
    T QueryBody<T>(string bodyName, string path);

    /// <summary>
    /// Uses JSONPath to query the current star system
    /// </summary>
    object QuerySystem(Type outType, string path);

    ///<summary>
    /// Uses JSONPath to query the current star system
    ///</summary>
    T QuerySystem<T>(string path);

    /// <summary>
    /// Allows you to overwrite the default system. This is where the player is respawned after dying.
    /// </summary>
    bool SetDefaultSystem(string name);

    /// <summary>
    /// Allows you to instantly begin a warp to a new star system.
    /// Will return false if that system does not exist (cannot be warped to).
    /// </summary>
    bool ChangeCurrentStarSystem(string name);

    /// <summary>
    /// Returns the uniqueIDs of each installed NH addon.
    /// </summary>
    string[] GetInstalledAddons();

    /// <summary>
    /// Allows you to spawn a copy of a prop by specifying its path.
    /// This is the same as using Props->details in a config, but also returns the spawned gameObject to you.
    /// </summary>
    GameObject SpawnObject(GameObject planet, Sector sector, string propToCopyPath, Vector3 position, Vector3 eulerAngles, 
        float scale, bool alignWithNormal);

    /// <summary>
    /// Allows you to spawn an AudioSignal on a planet.
    /// This is the same as using Props->signals in a config, but also returns the spawned AudioSignal to you.
    /// This method will not set its position. You will have to do that with the returned object.
    /// </summary>
    AudioSignal SpawnSignal(IModBehaviour mod, GameObject root, string audio, string name, string frequency,
        float sourceRadius = 1f, float detectionRadius = 20f, float identificationRadius = 10f, bool insideCloak = false,
        bool onlyAudibleToScope = true, string reveals = "");

    /// <summary>
    /// Allows you to spawn character dialogue on a planet. Also returns the RemoteDialogueTrigger if remoteTriggerRadius is specified.
    /// This is the same as using Props->dialogue in a config, but also returns the spawned game objects to you.
    /// This method will not set the position of the dialogue or remote trigger. You will have to do that with the returned objects.
    /// </summary>
    (CharacterDialogueTree, RemoteDialogueTrigger) SpawnDialogue(IModBehaviour mod, GameObject root, string xmlFile, float radius = 1f,
        float range = 1f, string blockAfterPersistentCondition = null, float lookAtRadius = 1f, string pathToAnimController = null,
        float remoteTriggerRadius = 0f);
}
```

In your main `ModBehaviour` class you can get the NewHorizons API like so:

```cs
public class MyMod : ModBehaviour 
{
    void Start() 
    {
        INewHorizons NewHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
    }
}
```

You can then use the APIs `LoadConfigs()` method to load from a "planets" folder, or use the `GetPlanet()` method to get planets and do whatever with them. Just make sure you create planets in the `Start()` method or at least before the SolarSystem scene loads, or they will not be created.

The `GetChangeStarSystemEvent` and `GetStarSystemLoadedEvent` events let you listen in for when the player starts changing to a new system (called when entering a black hole or using the warp drive) and when the system is fully loaded in, respectively.

You can also use the `GetInstalledAddons` method to get a list of addons that are installed and enabled.

You can also use `SpawnObject` to directly copy a base-game GameObject to the specified position and rotation.
