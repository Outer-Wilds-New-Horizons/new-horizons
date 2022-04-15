Title: API
Sort_Priority: 40

## How to use the API

First create the following interface in your mod:

```cs
public interface INewHorizons
{
    void Create(Dictionary<string, object> config, IModBehaviour mod);

    void LoadConfigs(IModBehaviour mod);

    GameObject GetPlanet(string name);
	
	string GetCurrentStarSystem(); 

    UnityEvent<string> GetChangeStarSystemEvent();

    UnityEvent<string> GetStarSystemLoadedEvent();
}
```

In your main `ModBehaviour` class you can get the NewHorizons API like so:
```cs
public class MyMod : ModBehaviour 
{
    void Start() 
    {
        INewHorizons NewHorizonsAPI = ModHelper.Interaction.GetModApi<INewHorizons>("xen.NewHorizons");
    }
}
```

You can then use the APIs `LoadConfigs()` method to load from a "planets" folder, or use the `Create()` and `GetPlanet()` methods to create planets and do whatever with them. Just make sure you create planets in the `Start()` method or at least before the SolarSystem scene loads, or they will not be created.

The `GetChangeStarSystemEvent` and `GetStarSystemLoadedEvent` events let you listen in for when the player starts changing to a new system (called when entering a black hole or using the warp drive) and when the system is fully loaded in, respectively.
