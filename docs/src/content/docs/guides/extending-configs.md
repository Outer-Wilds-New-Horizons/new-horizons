---
title: Extending  Configs
description: A guide on extending config files with the New Horizons API
---

This guide will explain how to use the API to add new features to New Horizons.

## How Extending Works

Addon developers will add a key to the `extras` object in the root of the config

```json title="wetrock.json"
{
    "name": "Wetrock",
    "extras": {
        "myCoolExtensionData": {
            "myCoolExtensionProperty": 2
        }
    }
}
```

Your mod will then use the APIs `QueryBody` method to obtain the `myCoolExtensionData` object.

**It's up to the addon dev to list your mod as a dependency!**

## Extending Planets

You can extend all planets by hooking into the `OnBodyLoaded` event of the API:

```csharp
var api = ModHelper.Interactions.TryGetModApi<INewHorizons>("xen.NewHorizons");
api.GetBodyLoadedEvent().AddListener((name) => {
    ModHelper.Console.WriteLine($"Body: {name} Loaded!");
});
```

In order to get your extra module, first define the module as a class:

```csharp
public class MyCoolExtensionData {
    int myCoolExtensionProperty;
}
```

Then, use the `QueryBody` method:

```csharp
var api = ModHelper.Interactions.TryGetModApi<INewHorizons>("xen.NewHorizons");
api.GetBodyLoadedEvent().AddListener((name) => {
    ModHelper.Console.WriteLine($"Body: {name} Loaded!");
    var data = api.QueryBody<MyCoolExtensionData>(name, "$.extras.myCoolExtensionData");
    // Makes sure the module is not null
    if (data != null) {
        ModHelper.Console.WriteLine($"myCoolExtensionProperty for {name} is {data.myCoolExtensionProperty}!");
    }
});
```

## Extending Systems

Extending systems is the exact same as extending planets, except you use the `QuerySystem` method instead.

## Accessing Other Values

You can also use the `QueryBody` method to get values of the config outside your extension object

```csharp
var primaryBody = api.QueryBody<string>("Wetrock", "$.Orbit.primaryBody");
ModHelper.Console.WriteLine($"Primary of {bodyName} is {primaryBody ?? "NULL"}!");
```
