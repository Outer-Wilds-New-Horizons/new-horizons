![new horizons thumbnail](https://user-images.githubusercontent.com/22628069/146680547-bd815057-9f4e-42da-a6c4-84d3ff82ff2c.png)

A planet creator for Outer Wilds.

Planets are created using a JSON file format structure, and placed in the `planets` folder.

<!-- TOC -->

- [Roadmap](#roadmap)
- [How to create your own planets using configs](#how-to-create-your-own-planets-using-configs)
  - [Base](#base)
  - [Orbit](#orbit)
  - [Atmosphere](#atmosphere)
  - [HeightMap](#heightmap)
  - [AsteroidBelt](#asteroidbelt)
  - [FocalPoint](#focalpoint)
  - [Props](#props)
  - [Spawn](#spawn)
  - [Star](#star)
  - [How to destroy existing planets](#how-to-destroy-existing-planets)
- [How to use New Horizons in other mods](#how-to-use-new-horizons-in-other-mods)
- [Credits](#credits)

<!-- /TOC -->

## Roadmap
- Procedurally terrain generation (started)
- Asteroid belts (started, needs more customization)
- "Quantum" orbits
- Better terrain and water LOD
- Support satellites (using custom models in the assets folder or in-game ones)
- Surface scatter (rocks, trees, etc, using custom models or in-game ones)
- Stars
- Binary orbits
- Load planet meshes from asset bundle
- Comets
- Edit existing planet orbits
- Black hole / white hole pairs
- Separate solar system scenes accessible via wormhole

## How to create your own planets using configs

There is a template [here](https://github.com/xen-42/ow-new-horizons-config-template) if you want to release your own planet mod using configs. You can learn how the configs work by picking apart the [Real Solar System](https://github.com/xen-42/outer-wilds-real-solar-system) mod or the [New Horizons Examples](https://github.com/xen-42/ow-new-horizons-examples) mod.

Your config file will look something like this:
```
{
	"name" : "Wetrock",
	"Base" : 
	{
		"groundSize" : 100,
		"waterSize" : 101,
		"surfaceSize" : 101,
		"surfaceGravity" : 12,
		"hasMapMarker" : true,
		"lightTint" : 
		{
			"r" : 255,
			"g" : 255,
			"b" : 255,
			"a" : 255
		}
	},
	"Orbit" : 
	{
		"semiMajorAxis" : 1300,
		"inclination" : 0,
		"primaryBody" : "TIMBER_HEARTH",
		"isMoon" : true,
		"isTidallyLocked" : true,
		"longitudeOfAscendingNode" : 0,
		"eccentricity" : 0,
		"argumentOfPeriapsis": 0
	},
	"Atmosphere" : 
	{
		"size" : 150,
		"fogTint" : 
		{
			"r" : 200,
			"g" : 255,
			"b" : 255,
			"a" : 255
		},
		"fogSize": 150,
		"fogDensity":0.2,
		"hasRain" : true,
	},
	"Props" :
	{
		"scatter" : [
			{"path" : "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var", "count" : 12}
		]
	}
}
```

The first field you should have in any config file is the `name`. This should be unique in the solar system. If it isn't, the mod will instead try to modify the planet that already has that name.

The config file is then split into modules, each one with it's own fields that define how that part of the planet will be generated. In the example above I've used the `Base`, `Orbit`, `Atmosphere`, and `Props` modules. A config file must have a `Base` and `Orbit` module, the rest are optional.

Each { must match up with a closing } to denote its section. If you don't know how JSONs work then check Wikipedia.

Modules look like this:

```
"Star" :
{
	"size" : 3000,
	"tint" : 
	{
		"r" : 201,
		"g" : 87,
		"b" : 55,
		"a" : 255
	},
}
```

In this example the `Star` module has a `size` field and a `tint` field. Since the colour is a complex object it needs another set of { and } around it, and then it has its own fields inside it : `r`, `g`, `b`, and `a`. Don't forget to put commas after each field.

Most fields are either true/false, a decimal number, and integer number, or a string (word with quotation marks around it). There are also colours and positions.

A colour is defined like this (with integers from 0 to 255): 

```
{
    "r" : 200,
    "g" : 255,
    "b" : 255,
    "a" : 255
}
```   

A position is defined like this:
```
{
    "x" : 182.4,
    "y" : 227.4,
    "z" : 62.7,
}
```

Now I'll go through the different modules and what they can do. Most fields are optional and will have some default value that will work fine if you don't include them.

### Base

- "hasMapMarker" : (true/false) If the body should have a marker on the map screen.
- "hasAmbientLight" : (true/false) If the dark side of the body should have some slight ammount of light
- "surfaceGravity" : (decimal number) How strong the acceleration due to gravity is at the surface. For example, Timber Hearth has a value of 12.
- "gravityFallOff" : (string) Acceptable values are "linear" or "inverseSquared". Defaults to "linear". Most planets use linear but the sun and some moons use inverseSquared.
- "surfaceSize" : (decimal number) A scale height used for a number of things. Should be the approximate radius of the body.
- "waterSize" : (decimal number) If you want the planet to have water on it, set a value for this. 
- "groundSize" : (decimal number) If you want the planet to have a perfectly spherical surface, set a value for this. 
- "blackHoleSize" : (decimal number) If you want there to be a black hole in the center of the planet, set a value for this. Can't be larger than 100 for now.
- "lavaSize" : (decimal number) If you want the planet to have lava on it, set a value for this. 
- "hasCometTrail" : (true/false) If you want the body to have a trail like the Interloper.
- "hasReferenceFrame" : (true/false) If the body should be target-able from the map screen.

### Orbit
Some of these I don't explain since they are just orbital parameters. If you don't know what they are, ask Wikipedia. Or just don't put a value.

- "semiMajorAxis" : (integer) The semi-major axis of the ellipse that is the body's orbit. For a circular orbit this is the radius.
- "inclination" : (integer) The angle (in degrees) between the planet's orbit and the plane of the solar system.
- "primaryBody" : (string) The name of what your planet should orbit. 
- "isMoon" : (true/false) Self explanatory. 
- "longitudeOfAscendingNode" : (decimal number) 
- "argumentOfPeriapsis" : (decimal number)
- "eccentricity" : (decimal number from 0 to < 1) The closer to 1 it is, the more oval-shaped the orbit is.
- "trueAnomaly" : (decimal number) Where the planet should start off in its orbit in terms of the central angle. From 0 to 360.
- "axialTilt" : (decimal number)
- "siderealPeriod" : (decimal number)
- "isTidallyLocked" : (true/false)
- "showOrbitLine" : (true/false) Referring to the orbit line in the map screen.
- "isStatic" : (true/false) Set to true to have the body not move at all. Good for when placing stars.
- "tint" : (colour) The colour of the orbit line in the map view.

### Atmosphere

- "size" : (decimal number)
- "cloudTint" : (colour) If you want clouds like over Giant's Deep, this will be the colour they are.
- "cloud" : (string) The file path to a texture file that will be given to the clouds.
- "cloudCap" : (string) The file path to a texture that will be put on the north pole over the clouds.
- "cloudRamp" : (string) I don't really know what this does. You don't have to put anything.
- "fogTint" : (colour) Puts tinted fog around the planet.
- "fogSize" : (decimal number)
- "hasRain" : (true/false)
- "hasSnow" : (true/false)
- "hasOxygen" : (true/false)
- "hasAtmosphere" : (true/false) If the planet should have an atmosphere shader like some of the other planets. Purely cosmetic. Will not get rid of any clouds or fog you've put.

### HeightMap
Allows you to generate more interesting terrain than the sphere given by groundSize in the Base module.

- "heightMap" : (string) The file path to a texture that will be used as a heightmap. Image should be greyscale. White is for high terrain and black for low.
- "textureMap" : (string) The file path to a texture that will be applied to the planet.
- "minHeight" : (decimal number) The height corresponding to pure black in the heightmap.
- "maxHeight" : (decimal number) The height corresponding to pure white in the heightmap.

### AsteroidBelt
Let's you put asteroids in orbit around a planet or star. Can probably negatively affect performance if you put too many.

- "innerRadius" : (decimal number)
- "outerRadius" : (decimal number)
- "inclination" : (decimal number)
- "longitudeOfAscendingNode" : (decimal number)
- "randomSeed" : (integer)

### FocalPoint
If you want to have binary planets or stars you have to do a few extra steps. First you make a focal point body. Here's an example (note the "..." means you'd be writing stuff there but it isn't important for the example. Don't literally put "..."):

```
{
	"name" : "Alpha Centauri",
	"Base" :
	{
		...
	},
	"Orbit" :
	{
		"semiMajorAxis" : 120000,
		"primaryBody" : "Sun",
		...
	},
	"FocalPoint" :
	{
		"Primary" : "Alpha Centauri A",
		"Secondary" : "Alpha Centauri B",
	}
}
```
Then you would make config files for the two bodies in the binary pair.
```
{
	"name" : "Alpha Centauri A",
	"Base" :
	{
		...
	},
	"Orbit" :
	{
		"semiMajorAxis" : 0,
		"primaryBody" : "Alpha Centauri",
		...
	},
	"Star" :
	{
		...
	}
}
```
and
```
{
	"name" : "Alpha Centauri B",
	"Base" :
	{
		...
	},
	"Orbit" :
	{
		"semiMajorAxis" : 10000,
		"primaryBody" : "Alpha Centauri",
		...
	},
	"Star" :
	{
		...
	}
}
```
The positions of the binaries will be based off of their masses (as determined by the "surfaceGravity" parameter). However, one of them must have a non-zero semiMajorAxis field else the mod gets confused. This example uses stars, but it will also work for planets. If you want to have other planets orbiting the center of mass, just put the focal point body as the primary body.

### Props
Lets you place items on the surface of the planet. Currently this is a very early release version.

-scatter : (list) I'll just give an example.

```
"scatter" : [
    {"path" : "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var", "count" : 12}
]
```

The path is in the hierarchy of the solar system. Use the [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) mod to find an object you want to copy onto your new body. Some objects work better than others for this. Good luck.

### Ring
- "innerRadius" : (decimal number)
- "outerRadius" : (decimal number)
- "inclination" : (decimal number) 
- "longitudeOfAscendingNode" : (decimal number)
- "texture" : (string) The file path to the image used for the rings. Normally I use a texture that is one pixel wide for this.

### Spawn
- "playerSpawnPoint" : (position) If you want the player to spawn on the new body, set a value for this. Press "P" in game to have the game log the position you're looking at to find a good value for this.
- "shipSpawnPoint" : (position)
- "startWithSuit" : (true/false) If you spawn on a planet with no oxygen, you probably want to set this to true ;)

### Star
Use this if you are creating a star.
- "size" : (decimal number)
- "tint" : (colour) 
- "solarFlareTint" : (colour) The flares are tinted weirdly so this won't make the actual colour of the star. You'll want to use trial and error to find something that matches.

### How to destroy existing planets

You do this (but with the appropriate name) as it's own config.
```
{
	"name" : "Ember Twin",
	"destroy" : true,
}
```

Remember that if you destroy Timber Hearth you better put a [Spawn](#spawn) module on another planet. I haven't tried destroying the sun. Probably don't do that, it will break everything. Probably.

## How to use New Horizons in other mods

First create the following interface in your mod:

```
public interface INewHorizons
{
    void Create(Dictionary<string, object> config, IModBehaviour mod);

    void LoadConfigs(IModBehaviour mod);

    GameObject GetPlanet(string name);
}
```

In your main `ModBehaviour` class you can get the NewHorizons API like so:
```
INewHorizons NewHorizonsAPI = ModHelper.Interaction.GetModApi<INewHorizons>("xen.NewHorizons")
```

You can then use the API's `LoadConfigs()` method to load from a "planets" folder, or use the `Create()` and `GetPlanet` methods to create planets and do whatever with them. Just make sure you create planets in the `Start()` method or at least before the SolarSystem scene loads, or they will not be created.

## Credits
Authors:
- xen (from New Horizons v0.1.0 onwards)
- Mister_Nebula (created original titled Marshmallow)

New Horizons contributors:
- salomj (Implemented [OW_CommonResources](https://github.com/PacificEngine/OW_CommonResources) support)

Marshmallow was made with help from:
- TAImatem
- AmazingAlek
- Raicuparta
- and the Outer Wilds discord server.
