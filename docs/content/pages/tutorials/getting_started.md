---
Title: Getting Started
Sort_Priority: 100
---

# Getting Started

Congrats on taking the first step to becoming an addon developer!
This tutorial will outline how to begin learning to use new horizons.

## Recommended Tools

It's strongly recommended you get [VSCode](https://code.visualstudio.com/){ target="_blank" } to edit your files, as it can provide syntax and error highlighting.

## Using The Sandbox

Making an entirely separate addon can get a little complicated, so New Horizons provides a way to play around without the need to set up a full addon.
To get started, navigate to your mod manager and click the ⋮ symbol, then select "Show In Explorer".

![Select "Show in explorer"]({{ "images/getting_started/mod_manager_show_in_explorer.webp"|static }})

Now, in explorer and create a new folder named "planets".  As the name suggests, New Horizons will search the files in this folder for planets to generate.

## Making Your First Planet

To get started, create a new file in this folder called `wetrock.json`, we'll explain what that .json at the end means soon.
Open this file in VSCode (you can do so by right-clicking the file and clicking "Open with Code")
Once in VSCode, paste this code into the file:

```json
{
  "name": "Wetrock",
  "$schema": "https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/main/NewHorizons/Schemas/body_schema.json",
  "starSystem": "SolarSystem",
  "Base": {
    "groundSize": 100,
    "surfaceSize": 101,
    "surfaceGravity": 12,
    "hasMapMarker": true
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
  }
}
```

This language is **J**ava**S**cript **O**bject **N**otation, or JSON.
It's a common way to convey data in many programs.

## Understanding JSON

All JSON files start out with an `object`, or a set of key value mappings, for example is we represent a person as JSON it might look like:

```json
{
    "name": "Jim"
}
```

Those braces (`{}`) denote an object, and by doing `"name": "Jim"` we're saying that the name of this object (in this case person) is Jim
`"name"` is the key, and `"Jim"` is the value.

Objects can have multiple keys as well, as long as you separate them by commas:

```json
{
    "name": "Jim",
    "age": 23
}
```

But wait! why is `Jim` in quotation marks while `23` isn't? that's because of a little something called data types. 
Each value has a datatype, in this case `"Jim"` is a `string`, because it represents a *string* of characters.
Age is a `number`, it represents a numerical value.  If we put 23 in quotation marks, its data type switches from a number to a string.
And if we remove the quotation marks from `"Jim"` we get a syntax error.  Datatypes are a common source of errors, which is why we recommend using an editor like VSCode.

### JSON Data Types

Here's a list of data types you'll use when making your addons:

#### String

A string of characters surrounded in quotation marks

```json
"Im a string!"
```

If you need to use quotation marks within your string, place a backslash (`\`) before them

```json
"\"Im a string!\" - Mr. String Stringerton"
```

#### Number

A numerical value, can be negative and have decimals, **not** surrounded in quotation marks

```json
-25.3
```

#### Boolean

A `true` or `false` value, think of it like an on or off switch

```json
true
```

#### Array

A set of values, values can be of any data type. Items are seperated by commas

```json
[23, 45, 56]
```

```json
["Bob", "Suzy", "Mark"]
```

And they can be empty like so:

```json
[]
```

#### Object

A set of key value pairs, where each key is a string and each value can be of any data type (even other objects!)

```json
{
    "name": "Jim",
    "age": 23,
    "isMarried": false,
    "clothes": {
        "shirtColor": "red",
        "pantsColor": "blue"
    },
    "friends": ["Bob", "Wade"],
    "enemies": []
}
```

## Back to Wetrock

Now that we understand JSON better, let's look at that config file again:

```json
{
  "name": "Wetrock",
  "$schema": "https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/main/NewHorizons/Schemas/body_schema.json",
  "starSystem": "SolarSystem",
  "Base": {
    "groundSize": 100,
    "surfaceSize": 101,
    "surfaceGravity": 12,
    "hasMapMarker": true
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
    - `hasMapMarker` tells new horizons that we want this planet to have a marker on the map screen
  - Next it has another object called `Orbit`
    - `semiMajorAxis` specifies the radius of the orbit (how far away the body is from its parent)
    - `primaryBody` is set to TIMBER_HEARTH, this makes our planet orbit timber hearth
    - `isMoon` simply tells the game how close you have to be to the planet in map mode before its name appears
    - `isTidallyLocked` makes sure that one side of our planet is always facing timber hearth (the primary body)
  - Finally, we have `Atmosphere`
    - Its `size` is 150, this simply sets how far away from the planet our atmosphere stretches
      - Its `fogTint` is set to a color which is an object with r, g, b, and a properties (properties is another word for keys)
      - `fogSize` determines how far away the fog stretches from the planet
      - `fogDensity` is simply how dense the fog is
      - `hasRain` makes rainfall on the planet

### What's a Schema?

That `$schema` property is a bit special, it instructs VSCode to use a pre-made schema to provide a better editing experience.
With the schema you get:

- Automatic descriptions for properties when hovering over keys
- Automatic error detection for incorrect data types or values
- Autocomplete, also called IntelliSense

## Testing The Planet

With the new planet created (*and saved!*), launch the game through the mod manager and click resume expedition. If all went well you should be able to open your map and see wetrock orbiting Timber Hearth.

If you run into issues please make sure:

- You placed the JSON file in a folder called `planets` in the New Horizons mod folder
- There are no red or yellow squiggly lines in your file

## Experiment!

With that, try tweaking some value like groundSize and semiMajorAxis, get a feel for how editing JSON works.

## Reloading Configs

It can get annoying when you have to keep closing and opening the game over and over again to test changes, that's why New Horizons has a "Reload Configs" feature.
To enable it, head to your Mods menu and select New Horizons and check the box that says Debug, this will cause a "Reload Configs" option to appear in your pause menu which will reload changes from your filesystem.
You may also notice blue and yellow logs start appearing in your console, this is New Horizons providing additional info on what it's currently doing, it can be helpful when you're trying to track down an issue.

## More Objects

Base, Atmosphere, and Orbit aren't all the objects (or "modules") there are to use, to learn about these other objects, you'll need to learn how to use the "Schemas" section of this site, which lists every possible property you can put in your files.

**Next Up: [Reading Schemas]({{ "Reading Schemas"|route }})**
