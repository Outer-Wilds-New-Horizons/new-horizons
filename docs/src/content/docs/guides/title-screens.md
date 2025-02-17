---
title: Title Screens
description: A guide to creating a custom title screens in New Horizons
---

Welcome! This page outlines how to edit a custom title screen.

## Getting Started

Your mod's title screen config is a JSON file named `title-screen.json` that should be placed within your mod folder.

A title screen config file will look something like this:

```json title="title-screen.json"
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/title_screen_schema.json",
    "disableNHPlanets": true,
    "factRequiredForTitle": "EXAMPLES_ARTIFICIAL_GRAVITY",
    "shareTitleScreen": true,
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
```

To see all the different things you can put into a config file check out the [Title Screen Schema](/schemas/title-screen-schema).