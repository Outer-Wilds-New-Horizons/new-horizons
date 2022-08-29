---
Title: Extending  Configs
Description: A guide on extending config files with the New Horizons API
Sort_Priority: 5
---

# Extending Configs

This guide will explain how to use the API to add new features to New Horizons.

## How Extending Works

Addon developers will add a key to the `extras` object in the root of the config

```json
{
    "name": "Wetrock",
    "extras": {
        "myCoolExtensionData": {
            "myCoolExtensionProperty": 2
        }
    }
}
```

Your mod will then use the API's `GetExtraModuleForBody` method to obtain the `myCoolExtensionData` object.

**It's up to the addon dev to list your mod as a dependency!**

## Extending Planets

You can extend all planets by hooking into the `OnBodyLoaded` event of the API:

```cs
var api = ModHelper.Interactions.TryGetModApi<INewHorizons>("xen.NewHorizons");
api.GetBodyLoadedEvent().AddListener((name) => {
    ModHelper.Console.WriteLine($"Body: {name} Loaded!");
});
```

In order to get your extra module, first define the module as a class:

```cs
public class MyCoolExtensionData {
    int myCoolExtensionProperty;
}
```

Then, use the `GetExtraModuleForBody` method:

```cs
var api = ModHelper.Interactions.TryGetModApi<INewHorizons>("xen.NewHorizons");
api.GetBodyLoadedEvent().AddListener((name) => {
    ModHelper.Console.WriteLine($"Body: {name} Loaded!");
    var potentialData = api.GetExtraModuleForBody(typeof(MyCoolExtensionData), "myCoolExtensionData", name);
    // Makes sure the module is valid and not null
    if (potentialData is MyCoolExtensionData data) {
        ModHelper.Console.WriteLine($"myCoolExtensionProperty for {name} is {data.myCoolExtensionProperty}!");
    }
});
```

## Extending Systems

Extending systems is the exact same as extending planets, except you use the `GetExtraModuleForSystem` method instead.
