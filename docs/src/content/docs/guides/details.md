---
title: Detailing
description: A guide to adding details to planets in New Horizons
---

For physical objects there are currently two ways of setting them up: specify an asset bundle and path to load a custom asset you created, or specify the path to the item you want to copy from the game in the scene hierarchy. Use the [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) mod to find an object you want to copy onto your new body. Some objects work better than others for this. Good luck. Some pointers:

-   Use "Object Explorer" to search
-   Generally you can find planets by writing their name with no spaces/punctuation followed by "\_Body".
-   There's also [this community-maintained list of props](https://docs.google.com/spreadsheets/d/1VJaglB1kRL0VqaXhvXepIeymo93zqhWex-j7_QDm6NE/edit?usp=sharing) which you can use to find interesting props and check to see if they have collision.

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

## Custom Items and Item Sockets

### Items

You can convert details into custom items for the player to pick up and hold by adding the `item` properties to your detail:

```json
{
    "Props": {
        "details": [
            {
				"path": "Moon_Body/Sector_THM/Interactables_THM/Prefab_HEA_Recorder/Props_HEA_Recorder_Geo",
				"position": {"x": -35.30206, "y": -79.12967, "z": 182.912},
				"rotation": {"x": 300.8207, "y": 32.93826, "z": 141.4214},
				"item": {
					"name": "Tape Recorder",
					"itemType": "Decoration"
				}
            }
        ]
    }
}
```

The `name` and `itemType` can be anything you want. The `name` will be what's displayed to the player when they mouse over it ("Pick up X"), and `itemType` determines the kinds of item sockets that the item can be placed in.

> Note: Outer Wilds is unfortunately very picky about the types of objects you can use as items. There must be a collider on the object itself which the game will use to check if the cursor is over it or not to allow you to pick it up. Colliders on child objects will have collision as normal but won't allow you to pick up the item when you mouse over them. By default, New Horizons will add a spherical collider to your detail to act as this mouse-over collider. If your object already has a working collider on it, you can disable the New Horizons spherical collider by setting `"colliderRadius": 0`.

Here's a more complex example, with every property filled out:

```json
{
    "Props": {
        "details": [
            {
				"path": "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/Props_BlackHoleForge/Prefab_NOM_VaseThick",
				"position": {"x": -33.30206, "y": -79.12967, "z": 182.912},
				"rotation": {"x": 300.8207, "y": 32.93826, "z": 141.4214},
				"item": {
					"name": "Nomai Vase",
					"itemType": "Decoration",
					"interactRange": 3,
					"colliderRadius": 0,

					"droppable": true,
					"dropOffset": {"x": 0, "y": 0, "z": 0},
					"dropNormal": {"x":0, "y": 1, "z": 0},

					"holdOffset": {"x": 0, "y": -0.3, "z": 0},
					"holdRotation": {"x": 0, "y": 45, "z": 0},

					"socketOffset": {"x": 0, "y": 0.2, "z": 0},
					"socketRotation": {"x": 0, "y": 45, "z": 0},

					"pickupAudio": "ToolProbeRetrieve",
					"dropAudio": "ToolProbeLaunch",
					"socketAudio": "PlayerSuitLockOn",
					"unsocketAudio": "PlayerSuitLockOff",

					"pickupCondition": "VASE_PICKED_UP",
					"clearPickupConditionOnDrop": true,
					"pickupFact": "EXAMPLES_VASE",

					"pathToInitialSocket": "Sector_TH/Strucutre_NOM_Shelf"
				}
            }
        ]
    }
}
```

To see the full list of item properties and descriptions of what each property does, check [the ItemInfo schema](/schemas/body-schema/defs/iteminfo/).

### Item Sockets

You can also designate a detail as an "item socket," which will allow certain items to be placed into it. For example, the Nomai whiteboards have a socket for scrolls, and the Vessel's warp core slot is also a socket. You do this by adding the `itemSocket` properties to your detail:

```json
{
    "Props": {
        "details": [
            {
				"path": "QuantumIsland_Body/Sector_QuantumIsland/Sector_QuantumTowerInterior/Sector_QuantumTowerFinalRoom/Interactables_FinalRoom/FinalRoom_QProps/FinalRoom_Shelf/Q_Shelf/Prefab_NOM_Shelf/Strucutre_NOM_Shelf",
				"position": {"x": -29.89541, "y": -79.87562, "z": 183.1785},
				"rotation": {"x": 314.8642, "y": 286.1038, "z": 235.4039},
				"itemSocket": {
					"itemType": "Decoration",
					"interactRange": 3,
                    "colliderRadius": 0.5,
					"useGiveTakePrompts": true,

					"insertCondition": "VASE_INSERTED",
                    "clearInsertConditionOnRemoval": true,
                    "insertFact": "EXAMPLES_VASE_INSERTED",

					"removalCondition": "VASE_REMOVED",
                    "clearRemovalConditionOnInsert": true,
                    "removalFact": "EXAMPLES_VASE_REMOVED",

                    "position": {"x": 0, "y": 0, "z": 0.5},
                    "rotation": {"x": 0, "y": 0, "z": 0},
					"isRelativeToParent": true
				}
            }
        ]
    }
}
```

Item sockets will allow any item with a matching `itemType` to be placed into them; all other item types will show the "item does not fit" message.

> Item sockets are also picky about colliders in the same way items are. The detail object must have a collider on it; colliders on child objects will not work for placing and removing items when the cursor is over them. New Horizons will add a spherical collider for you if you set `colliderRadius` to a non-zero value.

The `position`, `rotation`, and `isRelativeToParent` properties on the `itemSocket` don't describe the location of the detail itself, but rather the point on the detail where items will be inserted. This will likely not be at the exact center of the detail, so you should use this properties to customize the location of the socket. If there is already an child object you want to use as a pivot point, you can put a relative path from the detail to the child object in the `socketPath` property to use instead of the generated socket point.

To see the full list of item socket properties and descriptions of what each property does, check [the ItemSocketInfo schema](/schemas/body-schema/defs/itemsocketinfo/).

### Making Puzzles with Items and Sockets

You can use items and sockets to create simple puzzles purely with New Horizons configs! Consider this cut-down example. It describes a "Rusty Key" object, and a "Locked Door" object. The locked door will disappear when the key is inserted into it, allowing the player to move through the doorway.

```json
{
    "Props": {
        "details": [
            {
				"path": "BlahBlahBlah",
				"item": {
					"name": "Rusty Key",
					"itemType": "Key"
				}
            },
            {
				"path": "BlahBlahBlah",
                "rename": "Locked Door",
                "deactivationCondition": "DOOR_UNLOCKED",
				"itemSocket": {
					"itemType": "Key",
					"insertCondition": "DOOR_UNLOCKED"
				}
            }
        ]
    }
}
```

When the "Rusty Key" item (or any other item with `"itemType": "Key"`, for that matter) is inserted into the "Locked Door" item socket, a condition we've named `DOOR_UNLOCKED` is set to true. These conditions are mainly used for dialogue in Outer Wilds, but New Horizons also allows us to control objects with them. The locked door object has the `DOOR_UNLOCKED` condition as its `deactivationCondition`, which will cause it to disappear ("deactivate") when the condition is set.

There are a handful of other properties that can be used like this: `activationCondition` and `deactivationCondition` on all details, `insertCondition` and `removalCondition` on item sockets, and `pickupCondition` on items. There are other properties that can further customize these behaviors.


## Use the schema

To view additional options for detailing, check [the schema](/schemas/body-schema/defs/propmodule#details)
