---
title: Detailing
description: A guide to adding details to planets in New Horizons
---

For physical objects there are currently two ways of setting them up: specify an asset bundle and path to load a custom asset you created, or specify the path to the item you want to copy from the game in the scene hierarchy. Use the [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) mod to find an object you want to copy onto your new body. Some objects work better than others for this. Good luck. Some pointers:

- Use "Object Explorer" to search
- Generally you can find planets by writing their name with no spaces/punctuation followed by "\_Body".
- There's also [this community-maintained list of props](https://docs.google.com/spreadsheets/d/1VJaglB1kRL0VqaXhvXepIeymo93zqhWex-j7_QDm6NE/edit?usp=sharing) which you can use to find interesting props and check to see if they have collision.

## Using the Prop Placer

The Prop Placer is a convenience tool that lets you manually place details from inside the game. Once enabled, press "G" and your currently selected prop will be placed wherever your crosshair is pointing.

### Enabling

1. Pause the game. You will see an extra menu option titled "Toggle Prop Placer Menu". Click it
2. The prop placer menu should now be open. At the bottom of the menu, you will see a list of mods. Click yours.
    1. This menu scrolls. If you do not see your mod, it may be further down the list.
3. The Prop Placer is now active! Unpause the game, and you can now place Nomai vases using "G"

### How to Select Props

1. Pause the game again. The prop placer menu should still be visible.
2. At the top of the menu, you'll see a text box containing the path for the vase. Replace this with the path for the prop you want to place. For example: `DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var`
3. Tip: use the Unity Explorer mod to find the path for the object you want to place. You only have to do this once.
4. Unpause the game and press "G". Say hello to your new tree!
5. Pause the game again. You will now see the prop you just placed on the list of recently placed props just below the "path" text box.
6. Click on the button titled "Prefab_NOM_VaseThin". You can now place vases again.

### Extra features

1. Made a mistake? **Press the "-" key to undo.** Press the "+" key to redo.
2. If you have the Unity Explorer mod enabled, you can use this to tweak the position, rotation, and scale of your props. Your changes will be saved.
3. Want to save some recently placed props between game launches? On the recently placed props list, click the star next to the prop's name to favorite it.
4. Found a bug that ruined your configs? Check `AppData\Roaming\OuterWildsModManager\OWML\Mods\xen.NewHorizons\configBackups` for backup saves of your work. Folders are titled "\[date\]T\[time\]".
5. Want to add props to Ember Twin but don't feel like making a config file for it? We got you! Place that prop and the config file will be created automatically on your next save.
6. This even works for planets that were created by other mods!

## Asset Bundles

Here is a template project: [Outer Wilds Unity Template](https://github.com/xen-42/outer-wilds-unity-template)

The template project contains ripped versions of all the game scripts, meaning you can put things like DirectionalForceVolumes in your Unity project to have artificial gravity volumes loaded right into the game.

If for whatever reason you want to set up a Unity project manually instead of using the template, follow these instructions:

1. Start up a Unity 2019.4.39f1 project
2. In the "Assets" folder in Unity, create a new folder called "Editor". In it create a file called "CreateAssetBundle.cs" with the following code in it:

```cs title="Editor/CreateAssetBundle.cs"
using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
```

3. Create your object in the Unity scene and save it as a prefab.
4. Add all files used (models, prefabs, textures, materials, etc.) to an asset bundle by selecting them and using the dropdown in the bottom right. Here I am adding a rover model to my "rss" asset bundle for the Real Solar System add-on.

![setting asset bundle](@/assets/docs-images/details/asset_bundle.webp)

1. In the top left click the "Assets" drop-down and select "Build AssetBundles". This should create your asset bundle in a folder in the root directory called "StreamingAssets".
2. Copy the asset bundle and asset bundle .manifest files from StreamingAssets into your mod's "planets" folder. If you did everything properly they should work in game. To double-check everything is included, open the .manifest file in a text editor to see the files included and their paths.

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
