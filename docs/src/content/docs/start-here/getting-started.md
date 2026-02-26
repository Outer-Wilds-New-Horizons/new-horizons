---
title: Getting Started
description: A guide for getting started with New Horizons
---

## What Is New Horizons?

New Horizons is a mod creation framework for creating custom planets in the game [Outer Wilds](https://www.mobiusdigitalgames.com/outer-wilds.html) by Mobius Digital. It primarily uses JSON files, along with a few XML files to define content.

## Recommended Tools

It's strongly recommended you get [VSCode](https://code.visualstudio.com/) to edit your files, as it can provide syntax and error highlighting.

## Try Out New Horizons

Making an entirely separate addon can get a little complicated, so New Horizons provides a way to play around without the need to set up a full addon. If you want to make a full project, see [Creating An Addon](#creating-an-addon).

To get started, navigate to your mod manager and click the ⋮ symbol, then select "Show In Explorer".

![Select "Show in explorer"](@/assets/docs-images/getting_started/mod_manager_show_in_explorer.webp)

Now, create a new folder named "planets". As the name suggests, New Horizons will search the files in this folder for planets to generate.

### Making Your First Planet

To get started, create a new file in this folder called `wetrock.json`, we'll explain what that .json at the end means soon.
Open this file in VSCode (you can do so by right-clicking the file and clicking "Open with Code")
Once in VSCode, paste this code into the file:

```json title="wetrock.json"
{
    "name": "Wetrock",
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "starSystem": "SolarSystem",
    "Base": {
        "groundSize": 100,
        "surfaceSize": 101,
        "surfaceGravity": 12
    },
    "Orbit": {
        "semiMajorAxis": 1300,
        "primaryBody": "TIMBER_HEARTH",
        "isMoon": true,
        "isTidallyLocked": true
    },
    "Atmosphere": {
        "size": 150,
        "fogTint": {
            "r": 200,
            "g": 255,
            "b": 255,
            "a": 255
        },
        "fogSize": 150,
        "fogDensity": 0.2,
        "hasRain": true
    },
    "MapMarker": {
        "enabled": true
    }
}
```

Here we can see we have a planet object, which name is "Wetrock", and is in the "SolarSystem" (Base-game) star system.  
It has an object called Base, which has a groundSize of 100, and a surfaceSize of 101, and the list continues on.

Alright so now that we understand how the file is structures, let's look into what each value actually does:

- `name` simply sets the name of the planet
- `$schema` we'll get to in a second
- `starSystem` specifies what star system this planet is located in, in this case we're using the base game star system, so we put "SolarSystem"
- Then it has an object called `Base`
    - Base has a `groundSize` of 100, this generates a perfect sphere that is 100 units in radius as the ground of our planet
    - It also has a `surfaceSize` of 101, surface size is used in many calculations, it's generally good to set it to a bit bigger than ground size.
    - `surfaceGravity` describes the strength of gravity on this planet, in this case it's 12 which is the same as Timber Hearth
- Next it has another object called `Orbit`
    - `semiMajorAxis` specifies the radius of the orbit (how far away the body is from its parent)
    - `primaryBody` is set to `TIMBER_HEARTH``, this makes our planet orbit timber hearth
    - `isMoon` simply tells the game how close you have to be to the planet in map mode before its name appears
    - `isTidallyLocked` makes sure that one side of our planet is always facing timber hearth (the primary body)
- Next, we have `Atmosphere`
    - Its `size` is 150, this simply sets how far away from the planet our atmosphere stretches
    - Its `fogTint` is set to a color which is an object with r, g, b, and a properties (properties is another word for keys)
    - `fogSize` determines how far away the fog stretches from the planet
    - `fogDensity` is simply how dense the fog is
    - `hasRain` makes rainfall on the planet
- Finally, we have `MapMarker`
    - `enabled` tells New Horizons that we want this planet to have a marker on the map screen

#### What's a Schema?

That `$schema` property is a bit special, it instructs VSCode to use a pre-made schema to provide a better editing experience.
With the schema you get:

- Automatic descriptions for properties when hovering over keys
- Automatic error detection for incorrect data types or values
- Autocomplete, also called IntelliSense

The schema we're using here is the [Celestial Body Schema](/schemas/body-schema), but there are many others available in the Schemas section of the left sidebar.

### Testing The Planet

With the new planet created (_and saved!_), launch the game through the mod manager and click resume expedition. If all went well you should be able to open your map and see wetrock orbiting Timber Hearth.

If you run into issues please make sure:

- You placed the JSON file in a folder called `planets` in the New Horizons mod folder
- There are no red or yellow squiggly lines in your file

Now that you are familiar with the basics, it's time to [create an addon](/docs/src/content/docs/start-here/creating-an-addon.md)!
