Title: API
Sort-Priority: 40

## How to use the API

First create the following interface in your mod:

```cs
public interface INewHorizons
{
    void Create(Dictionary<string, object> config, IModBehaviour mod);

    void LoadConfigs(IModBehaviour mod);

    GameObject GetPlanet(string name);
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

You can then use the APIs `LoadConfigs()` method to load from a "planets" folder, or use the `Create()` and `GetPlanet` methods to create planets and do whatever with them. Just make sure you create planets in the `Start()` method or at least before the SolarSystem scene loads, or they will not be created.
