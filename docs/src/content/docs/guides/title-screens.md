---
title: Title Screens
description: A guide to creating a custom title screens in New Horizons
---

Welcome! This page outlines how to make a custom title screen.

## Getting Started

Your mod's title screen config is a JSON file named `title-screen.json` that should be placed within your mod folder.

A title screen config file will look something like this:

```json title="title-screen.json"
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/title_screen_schema.json",
    "titleScreens": [
        {
            "disableNHPlanets": false,
            "shareTitleScreen": true,
            "music": "planets/assets/TitleScreenMusic.mp3"
        },
        {
            "disableNHPlanets": true,
            "shareTitleScreen": true,
            "factRequiredForTitle": "EXAMPLES_ARTIFICIAL_GRAVITY",
            "menuTextTint": {
                "r": 128,
                "g": 128,
                "b": 255
            },
            "music": "planets/assets/TitleScreenMusic.mp3",
            "ambience": "planets/assets/TitleScreenAmbience.mp3",
            "Skybox": {
                "destroyStarField": true,
                "rightPath": "systems/New System Assets/Skybox/Right_Large.png",
                "leftPath": "systems/New System Assets/Skybox/Left_Large.png",
                "topPath": "systems/New System Assets/Skybox/Up_Large.png",
                "bottomPath": "systems/New System Assets/Skybox/Down_Large.png",
                "frontPath": "systems/New System Assets/Skybox/Front_Large.png",
                "backPath": "systems/New System Assets/Skybox/Back_Large.png"
            },
            "Background": {
                "details": [
                    {
                        "assetBundle": "assetbundles/test",
                        "path": "Assets/Prefabs/Background.prefab",
                        "position": {"x": 200, "y": 280, "z": -50},
                        "rotation": {"x": 310, "y": 0, "z": 310},
                        "scale": 0.05
                    }
                ],
                "rotationSpeed": 10
            },
            "MenuPlanet": {
                "destroyMenuPlanet": false,
                "removeChildren": ["PlanetRoot/Props"],
                "details": [
                    {
                        "assetBundle": "assetbundles/test",
                        "path": "Assets/Prefabs/ArtificialGravity.prefab",
                        "removeChildren": ["Gravity"],
                        "parentPath": "PlanetRoot",
                        "position": {"x": 0, "y": 32, "z": 0},
                        "rotation": {"x": 90, "y": 0, "z": 0},
                        "scale": 10
                    }
                ],
                "rotationSpeed": 20
            }
        }
    ]
}
```

You can have multiple title screens but only one will be selected from the list. The last title screen in the list, that is unlocked, will always be selected.

## Configs

Title screens from configs are always put first into the list.

### `disableNHPlanets`

If set to `true`, prevents NH-generated planets from appearing on the title screen. Defaults to true.

### `shareTitleScreen`

If set to `true`, this title screen will merge with others that have the same setting enabled. For more info head to the [sharing section](#sharing) of this page. Defaults to false.

### `menuTextTint`

Defines the color of the menu text and logo. Uses RGB values, where `r`, `g`, and `b` range from `0` to `255`.

### `factRequiredForTitle`

Specifies a ship log fact that must be discovered for this title screen to appear.

### `conditionRequiredForTitle`

Specifies a persistent condition required for this title screen to appear.

### `music` and `ambience`

The audio for background music and ambience. Can be a path to a .wav/.ogg/.mp3 file, or taken from the [AudioClip list](/reference/audio-enum).

### `Background` and `MenuPlanet`

A module for the background and main menu planet that include object additions, removal, and rotation speed.

##### `details`

You can add objects to both the background and menu planet. The menu planet objects spin while the background objects are stationary.
These simplified details are just like the details in planet configs except that they only have the basic features.

## Schema

To see all the different things you can put into a config file check out the [Title Screen Schema](/schemas/title-screen-schema).

## API

New Horizons provides an API method to register and build custom title screens dynamically.

These will be put at the end of the list for the selection of all your mod's title screens.

You cannot combine configs with API unfortunately as only the API will be selected.

```csharp title="INewHorizons.cs"
/// <summary>
/// Registers a builder for the main menu.
/// Call this once before the main menu finishes loading
/// </summary>
void RegisterTitleScreenBuilder(IModBehaviour mod, Action<GameObject> builder, bool disableNHPlanets = true, bool shareTitleScreen = false, string conditionRequired = null, string factRequired = null);
```

It shares a few values with the configs but also has an exclusive one.

`builder`: Builder to run when this title screen is selected. The GameObject passed through it is the main scene object containing both the background and menu planet.

### Example API usage

You can run `RegisterTitleScreenBuilder` more than once to add multiple title screen builders.

```csharp title="YourModBehaviour.cs"
NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
NewHorizons.RegisterTitleScreenBuilder(this, BuildTitleScreen, disableNHPlanets: true, shareTitleScreen: true);
```

```csharp title="YourModBehaviour.cs"
public void BuildTitleScreen(GameObject scene)
{
    ModHelper.Console.WriteLine($"Building title screen", MessageType.Success);
    //Add an object to the title screen or do whatever else you want
}
```

## Events

Additionally, New Horizons provides events in the API for tracking title screen loading:

```csharp title="INewHorizons.cs"
/// <summary>
/// An event invoked when NH has finished building a title screen.
/// Gives the unique name of the mod the title screen builder was from and the index for when you have multiple title screens.
/// </summary>
UnityEvent<string, int> GetTitleScreenLoadedEvent();

/// <summary>
/// An event invoked when NH has finished building the title screen.
/// </summary>
UnityEvent GetAllTitleScreensLoadedEvent();
```

### Example event usage

```csharp title="YourModBehaviour.cs"
NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
NewHorizons.GetTitleScreenLoadedEvent().AddListener(OnTitleScreenLoaded);
NewHorizons.GetAllTitleScreensLoadedEvent().AddListener(OnAllTitleScreensLoaded);
```

```csharp title="YourModBehaviour.cs"
public void OnTitleScreenLoaded(string modUniqueName, int index)
{
    ModHelper.Console.WriteLine($"Title screen loaded: {modUniqueName} #{index}", MessageType.Success);
}
public void OnAllTitleScreensLoaded()
{
    ModHelper.Console.WriteLine("All title screens loaded", MessageType.Success);
}
```

## Sharing

New Horizons will randomly select a valid title screen each time the user enters the main menu and then if `shareTitleScreen` is set to `true` it will build all the other shareable title screens (that also have matching `disableNHPlanets` values). If it doesn't have share set to true then it will only show the randomly selected.
