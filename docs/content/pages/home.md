Title: Home
Out-File: index
Sort-Priority: 100

![New Horizons Logo]({{ 'images/home/home_logo.webp'|static }})

# Outer Wilds New Horizons
        
This is the official documentation for [New Horizons](https://github.com/xen-42/outer-wilds-new-horizons){ target="_blank" }

## Getting Started

The [Config Template](https://github.com/xen-42/ow-new-horizons-config-template){ target="_blank" } is available if you want to release your own
planet mod using configs. You can learn how the configs work by picking apart
the [Real Solar System](https://github.com/xen-42/outer-wilds-real-solar-system){ target="_blank" } mod or
the [New Horizons Examples](https://github.com/xen-42/ow-new-horizons-examples){ target="_blank" } mod.

Planets are created using a JSON file format structure, and placed in a folder called planets (or in any subdirectory of
it) in the location where New Horizons is installed (by default this folder doesn't exist, you have to create it within
the xen.NewHorizons directory).

To locate this directory, click the "⋮" symbol next to "New Horizons" in the Outer Wilds Mod Manager and then click "
show in explorer" in the pop-up.

![Click the three dots in the mod manager]({{ 'images/home/mod_manager_dots.webp'|static }})

![Create a new folder named "planets"]({{ 'images/home/create_planets.webp'|static }})

Now that you have created your planets folder, this is where you will put your planet config files. A config file will
look something like this:

```json
{
  "name": "Wetrock",
  "$schema": "https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/master/NewHorizons/schema.json",
  "starSystem": "SolarSystem",
  "Base": {
    "groundSize": 100,
    "surfaceSize": 101,
    "surfaceGravity": 12,
    "hasMapMarker": true
  },
  "Orbit": {
    "semiMajorAxis": 1300,
    "inclination": 0,
    "primaryBody": "TIMBER_HEARTH",
    "isMoon": true,
    "isTidallyLocked": true,
    "longitudeOfAscendingNode": 0,
    "eccentricity": 0,
    "argumentOfPeriapsis": 0
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

The first field you should have in any config file is the `name`. This should be unique in the solar system. If it
isn't, the mod will instead try to modify the planet that already has that name.

After `name` is `starSystem`. You can use this to place the planet in a different system accessible using a black-hole (
see the [Singularity]({{ 'body'|route }}#Singularity) module). To ensure compatibility with other mods this name should be unique. After
setting a value for this, the changes in the config will only affect that body in that star system. By default, it is "
SolarSystem", which is the scene from the stock game.

Including the "$schema" line is optional, but will allow your text editor to highlight errors and auto-suggest words in
your config. I recommend using VSCode as a text editor, but anything that supports Json files will work. Something as
basic as notepad will work but will not highlight any of your errors.

The config file is then split into modules, each one with its own fields that define how that part of the planet will be
generated. In the example above I've used the `Base`, `Orbit`, `Atmosphere`, and `Props` modules. A config file must
have a `Base` and `Orbit` module, the rest are optional.

Each `{` must match up with a closing `}` to denote its section. If you don't know how JSONs work then check Wikipedia.

Modules look like this:

```json
{
  "Star": {
    "size": 3000,
    "tint": {
      "r": 201,
      "g": 87,
      "b": 55,
      "a": 255
    }
  }
}
```

In this example the `Star` module has a `size` field and a `tint` field. Since the colour is a complex object it needs
another set of `{` and `}` around it, and then it has its own fields inside it : `r`, `g`, `b`, and `a`. Don't forget to put
commas after each field.

Most fields are either true/false, a decimal number, and integer number, or a string (word with quotation marks around
it).

Check out the rest of the site for how to format planet, star system, dialogue, ship log, and translation files!

## Helpful Resources

The texturemap/heightmap feature was inspired by the KSP mod Kopernicus. A lot of the same techniques that apply to
planet creation there apply to New Horizons. If you need help with planetary texturing, check out [The KSP texturing guide](https://forum.kerbalspaceprogram.com/index.php?/topic/165285-planetary-texturing-guide-repository/){ target="_blank" }.

[Photopea](https://www.photopea.com/){ target="_blank" } is a free browser-based photo editor which has useful features like
rectangular-to-polar coordinate transformation, useful for fixing abnormalities at the poles of your planets. 