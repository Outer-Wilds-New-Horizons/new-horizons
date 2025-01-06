---
title: Detailing
description: A guide to adding details to planets in New Horizons
---

For physical objects there are currently two ways of setting them up: specify an asset bundle and path to load a custom asset you created, or specify the path to the item you want to copy from the game in the scene hierarchy. Use the [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) mod to find an object you want to copy onto your new body. Some objects work better than others for this. Good luck. Some pointers:

- Use "Object Explorer" to search
- Generally you can find planets by writing their name with no spaces/punctuation followed by "\_Body".
- There's also [this community-maintained list of props](https://docs.google.com/spreadsheets/d/1VJaglB1kRL0VqaXhvXepIeymo93zqhWex-j7_QDm6NE/edit?usp=sharing) which you can use to find interesting props and check to see if they have collision.

## Debug Raycast

If you turn on debug mode (the mod option), you can press P to shoot a ray where you're looking. This will print location info to the console that you can paste into your configs, as well as paths that you can explore further in Unity Explorer.
Of note: the rotation of the raycast will have the up direction facing away from the ground/wall/ceiling and the forward direction facing you.

## [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer/)

You can use this to tweak the position, rotation, and scale of your props. These docs will not elaborate too much on this tool. There are other tutorials out there.

## Asset Bundles

There is an [old unity template](https://github.com/xen-42/outer-wilds-unity-template) and a [new one](https://github.com/ow-mods/outer-wilds-unity-wiki/wiki#outer-wilds-unity-assets)

The project contains ripped versions of all the game scripts, meaning you can put things like DirectionalForceVolumes in your Unity project to have artificial gravity volumes loaded right into the game.
Either one works, but the tool one has more tools and more feature-full versions of the scripts (in exchange for being invite-only)

Read [this guide](https://github.com/ow-mods/outer-wilds-unity-wiki/wiki/Tutorials-%E2%80%90-Using-asset-bundles) on how to work with asset bundles in editor.

## Importing a planet's surface from Unity

Making a planet's entire surface from a Unity prefab is the exact same thing as adding one single big detail at position (0, 0, 0).

## Examples

To add a Mars rover to the red planet in [RSS](https://github.com/xen-42/outer-wilds-real-solar-system), its model was put in an asset bundle as explained above, and then the following was put into the `Props` module:

```json {5-6}
{
    "Props": {
        "details": [
            {
                "assetBundle": "planets/assetbundle/rss",
                "path": "Assets/RSS/Prefabs/Rover.prefab",
                "position": {
                    "x": 146.5099,
                    "y": -10.83688,
                    "z": -36.02736
                },
                "alignRadial": true
            }
        ]
    }
}
```

To scatter 12 trees from the Dream World around Wetrock in [NH Examples](https://github.com/xen-42/ow-new-horizons-examples) , the following was put into the `Props` module:

```json
{
    "Props": {
        "scatter": [
            {
                "path": "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var",
                "count": 12
            }
        ]
    }
}
```

You can swap these around too. The following would scatter 12 Mars rovers across the planet and place a single tree at a given position:

```json
{
    "Props": {
        "details": [
            {
                "path": "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var",
                "position": {
                    "x": 146.5099,
                    "y": -10.83688,
                    "z": -36.02736
                },
                "alignRadial": true
            }
        ],
        "scatter": [
            {
                "assetBundle": "planets/assetbundle/rss",
                "path": "Assets/RSS/Prefabs/Rover.prefab",
                "count": 12
            }
        ]
    }
}
```

## Use the schema

To view additional options for detailing, check [the schema](/schemas/body-schema/defs/propmodule#details)
