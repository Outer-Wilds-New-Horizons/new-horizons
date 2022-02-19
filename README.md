![new horizons thumbnail 2](https://user-images.githubusercontent.com/22628069/154112130-b777f618-245f-44c9-9408-e11141fc5fde.png)

![Current version](https://img.shields.io/github/manifest-json/v/xen-42/outer-wilds-new-horizons?color=gree&filename=NewHorizons%2Fmanifest.json)
![Downloads](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/total)
![Downloads of newest version](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/latest/total)

A custom world creation tool for Outer Wilds.

You can view the addons creators have made (or upload one yourself) [here](https://outerwildsmods.com/custom-worlds)!

Check the ship's log for how to use your warp drive to travel between star systems!

<!-- TOC -->

- [Incompatible mods](#incompatible-mods)
- [Roadmap](#roadmap)
- [How to create your own planets using configs](#how-to-create-your-own-planets-using-configs)
  - [Base](#base)
  - [Orbit](#orbit)
  - [Atmosphere](#atmosphere)
  - [HeightMap](#heightmap)
  - [AsteroidBelt](#asteroidbelt)
  - [FocalPoint](#focalpoint)
  - [Props](#props)
  	- [Details/Scatterer](#detailsscatterer)	
  		- [Asset Bundles](#asset-bundles)
  	- [Dialogue](#dialogue)
  		- [XML](#xml)
  - [Ring](#ring)
  - [Spawn](#spawn)
  - [Star](#star)
  - [Signal](#signal)
  - [Singularity](#singularity)
  - [Water](#water)
  - [Lava](#lava)
  - [Sand](#sand)
  - [How to destroy existing planets](#how-to-destroy-existing-planets)
  - [How to update existing planets](#how-to-update-existing-planets)
- [How to use New Horizons in other mods](#how-to-use-new-horizons-in-other-mods)
- [Helpful resources](#helpful-resources)
- [Contact](#contact)
- [Credits](#credits)

<!-- /TOC -->

## Incompatible mods
- Autoresume (breaks the ship's warp drive).
- Multiplayer mods like QSB and OWO.

## Roadmap
- Heightmaps/texturemaps (Done)
- Remove existing planets (Done)
- Stars (Done)
- Binary orbits (Done)
- Comets (Done)
- Signalscope signals (Done)
- Asteroid belts (Done)
- Support satellites (Done)
- Surface scatter: rocks, trees, etc, using in-game models (done) or custom ones (done)
- Load planet meshes from asset bundle (done)
- Black hole / white hole pairs (done)
- Separate solar system scenes accessible via wormhole (done)
- Warp drive with target set in ship's log (done)
- Implement custom dialogue (done)
- Make a template Unity project to use with NH, including all game scripts recovered using UtinyRipper to make AssetBundle creation easier ([done](https://github.com/xen-42/outer-wilds-unity-template))
- Procedural terrain generation (started)
- "Quantum" planet parameters
- Better terrain and water LOD
- Edit existing planet orbits
- Implement all planet features:
 	- Funnels (sand/water/lava/star) (done)
	- Variable surface height (sand/water/lava/star) (done)
	- Zero-g volumes (done, with Unity template)
	- Ghost matter (done, by copying props via game hierarchy)
	- Tornados + floating islands
	- Let any star go supernova
	- Geysers
	- Meteors
	- Pocket dimensions
	- Timed position/velocity changes
- Implement custom Nomai scrolls
- Implement custom translatable writing
- Destroy planets that fall into a star

## How to create your own planets using configs

**Note that I'm more than happy to help you debug your planet config files, but please don't just use notepad to edit them. Use something like Visual Studio Code that will check your .json file for errors. It will save both of us time.**

There is a template [here](https://github.com/xen-42/ow-new-horizons-config-template) if you want to release your own planet mod using configs. You can learn how the configs work by picking apart the [Real Solar System](https://github.com/xen-42/outer-wilds-real-solar-system) mod or the [New Horizons Examples](https://github.com/xen-42/ow-new-horizons-examples) mod.

Planets are created using a JSON file format structure, and placed in a folder called `planets` (or in any sub-directory of it) in the location where New Horizons is installed (by default this folder doesn't exist, you have to create it within the `xen.NewHorizons` directory).

To locate this directory, click the "..." symbol next to "New Horizons" in the Outer Wilds Mod Manager and then click "show in explorer" in the pop-up.

![Where to click](https://user-images.githubusercontent.com/22628069/149637969-827fccfe-b746-4515-a040-9369a9f37268.png)

![Create a planets folder in the mod directory](https://user-images.githubusercontent.com/22628069/149638007-26b872ab-f02e-455f-a7fd-99d0d2a96de8.png)

Now that you have created your planets folder, this is where you will put your planet config files. A config file will look something like this:
```json
{
	"name" : "Wetrock",
	"$schema": "https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/master/NewHorizons/schema.json",
	"starSystem" : "SolarSystem",
	"Base" : 
	{
		"groundSize" : 100,
		"surfaceSize" : 101,
		"surfaceGravity" : 12,
		"hasMapMarker" : true,
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

After `name` is `starSystem`. You can use this to place the planet in a different system accessible using a black-hole (see the [Singularity](#singularity) module). To ensure compatibility with other mods this name should be unique. After setting a value for this, the changes in the config will only affect that body in that star system. By default it is "SolarSystem", which is the scene from the stock game.

Including the "$schema" line is optional, but will allow your text editor to highlight errors and auto-suggest words in your config. I recommend using VSCode as a text editor, but anything that supports Json files will work. Something as basic as notepad will work but will not highlight any of your errors.

The config file is then split into modules, each one with it's own fields that define how that part of the planet will be generated. In the example above I've used the `Base`, `Orbit`, `Atmosphere`, and `Props` modules. A config file must have a `Base` and `Orbit` module, the rest are optional.

Each { must match up with a closing } to denote its section. If you don't know how JSONs work then check Wikipedia.

Modules look like this:

```json
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

Most fields are either true/false, a decimal number, and integer number, or a string (word with quotation marks around it). There are also the following types of values:

#### Colour:
```json
{
	"r" : 200,
	"g" : 255,
	"b" : 255,
	"a" : 255
}
```   

#### Position:
```json
{
	"x" : 182.4,
	"y" : 227.4,
	"z" : 62.7,
}
```

#### Scale curve:
```json
[
	{"time":0, "value":1},
	{"time":5, "value":0},
	{"time":12, "value":1}
]
```
Specifically it is a list of `time` and `value` pairs. The time is given in minutes and the value goes from 0 to 1. What the example means if that when the loop starts the object will be at its max size, after 5 minutes it will shrink to nothing, then 7 minutes later (so 12 minutes total in the loop) it will have grown back to full size. You can put as many values in the list as you want. 

Now I'll go through the different modules and what they can do. Most fields are optional and will have some default value that will work fine if you don't include them.

### Base

- "hasMapMarker" : (true/false) If the body should have a marker on the map screen.
- "hasAmbientLight" : (true/false) If the dark side of the body should have some slight ammount of light
- "surfaceGravity" : (decimal number) How strong the acceleration due to gravity is at the surface. For example, Timber Hearth has a value of 12.
- "gravityFallOff" : (string) Acceptable values are "linear" or "inverseSquared". Defaults to "linear". Most planets use linear but the sun and some moons use inverseSquared.
- "surfaceSize" : (decimal number) A scale height used for a number of things. Should be the approximate radius of the body.
- "groundSize" : (decimal number) If you want the planet to have a perfectly spherical surface, set a value for this. 
- "hasCometTrail" : (true/false) If you want the body to have a trail like the Interloper.
- "hasReferenceFrame" : (true/false) If the body should be target-able from the map screen.
- "centerOfSolarSystem" : (true/false) If the body is the new center of the solar system be sure to set this to true.

### Orbit
Some of these I don't explain since they are just orbital parameters. If you don't know what they are, ask Wikipedia. Or just don't put a value.

- "semiMajorAxis" : (integer) The semi-major axis of the ellipse that is the body's orbit. For a circular orbit this is the radius.
- "inclination" : (integer) The angle (in degrees) between the planet's orbit and the plane of the solar system.
- "primaryBody" : (string) The name of what your planet should orbit. 
- "isMoon" : (true/false) Self explanatory. 
- "longitudeOfAscendingNode" : (decimal number) An angle (in degrees) defining the point where the orbit of the body rises above the orbital plane if it has nonzero inclination.
- "argumentOfPeriapsis" : (decimal number) An angle (in degrees) defining the location of the periapsis (closest distance to it's primary body) if it has nonzero eccentricity.
- "eccentricity" : (decimal number from 0 to < 1) The closer to 1 it is, the more oval-shaped the orbit is.
- "trueAnomaly" : (decimal number) Where the planet should start off in its orbit in terms of the central angle. From 0 to 360.
- "axialTilt" : (decimal number) The angle between the normal to the orbital plane and its axis of rotation.
- "siderealPeriod" : (decimal number) Rotation period in minutes
- "isTidallyLocked" : (true/false) Should the body always have one side facing its primary?
- "showOrbitLine" : (true/false) Referring to the orbit line in the map screen.
- "isStatic" : (true/false) Set to true to have the body not move at all. Good for when placing stars.
- "tint" : (colour) The colour of the orbit line in the map view.

### Atmosphere

- "size" : (decimal number)
- "cloudTint" : (colour) If you want clouds like over Giant's Deep, this will be the colour they are.
- "cloud" : (string) The file path to a texture file that will be given to the clouds.
- "cloudCap" : (string) The file path to a texture that will be put on the north pole over the clouds.
- "cloudRamp" : (string) I don't really know what this does. You don't have to put anything.
- "useBasicCloudShader" : (true/false) By default we use the same shader as Giant's deep. Set this to true if you just want your cloud texture to be applied as is.
- "shadowsOnClouds" : (true/false) By default clouds have shadows on them. Set to false if you want the clouds to have flat colouring (useful for brown dwarfs)
- "fogTint" : (colour) Puts tinted fog around the planet.
- "fogSize" : (decimal number)
- "fogDensity" : (decimal number) How dense the fog is, from 0 to 100.
- "hasRain" : (true/false)
- "hasSnow" : (true/false)
- "hasOxygen" : (true/false)
- "hasAtmosphere" : (true/false) If the planet should have an atmosphere shader like some of the other planets. Purely cosmetic. Will not get rid of any clouds or fog you've put.

### HeightMap
Allows you to generate more interesting terrain than the sphere given by groundSize in the Base module. Textures should be at least 2048 x 1024 resolution.

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

```json
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
```json
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
```json
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
Lets you place features on the surface of the planet.

#### Details/Scatterer
For physical objects there are currently two ways of setting them up: specify an asset bundle and path to load a custom asset you created, or specify the path to the item you want to copy from the game in the scene hierarchy. Use the [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) mod to find an object you want to copy onto your new body. Some objects work better than others for this. Good luck. Some pointers:
- Use "Object Explorer" to search
- Do not use the search functionality on Scene Explorer, it is really really slow. Use the "Object Search" tab instead.
- Generally you can find planets by writing their name with no spaces/punctuation followed by "_Body".

The different things you can specify in the props section are:

- "scatter" : (list) I'll just give an example. 

```json
"scatter" : [
    {"path" : "DreamWorld_Body/Sector_DreamWorld/Sector_DreamZone_1/Props_DreamZone_1/OtherComponentsGroup/Trees_Z1/DreamHouseIsland/Tree_DW_M_Var", "count" : 12}
]
```

- "details" : (list of detail info objects)

A detail info object can have the following parameters:
- "path" : (string) either the location of it in the scene hierarchy or in the asset bundle provided
- "assetBundle" : (string) the asset bundle containing the object
- "objFilePath" : (string) the file path to a .obj 3d model
- "mtlFilePath" : (string) the file path to the material for the .obj model
- "position" : (x, y, z)
- "rotation" : (x, y, z) the euler angle rotation from a 3d vector (in degrees)
- "scale" : (decimal number)
- "alignToNormal" : (true/false) If it should align with the normal vector of the surface its own (overwrites rotation)
- "removeChildren" : (list of strings) relative paths of children to get rid of

You have three options: Load from the scene hierarchy by setting "path", load from an asset bundle by setting "path" and "assetBundle", or load an obj file by setting "objFilePath" and "mtlFilePath". Asset bundles give much better results than .obj's.

#### Asset Bundles

Here is a template project: [Outer Wilds Unity Template](https://github.com/xen-42/outer-wilds-unity-template)

The template project contains ripped versions of all the game scripts, meaning you can put things like DirectionalForceVolumes in your Unity project to have artificial gravity volumes loaded right into the game.

If for whatever reason you want to set up a Unity project manually instead of using the template, follow these instructions:

1. Start up a Unity 2017 project (I use Unity 2017.4.40f1 (64-bit), so if you use something else I can't guarantee it will work). The DLC updated Outer Wilds to 2019.4.27 so that probably works but I personally haven't tried it.
2. In the "Assets" folder in Unity, create a new folder called "Editor". In it create a file called "CreateAssetBundle.cs" with the following code in it:

```cs
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
4. Add all files used (models, prefabs, textures, materials, etc) to an asset bundle by selecting them and using the drop down in the bottom right. Here I am adding a rover model to my "rss" asset bundle for the Real Solar System add-on.
![setting asset bundle](https://user-images.githubusercontent.com/22628069/147954146-e1d610c0-0336-428f-8b32-bfc01090061c.png)

5. In the top left click the "Assets" drop-down and select "Build AssetBundles". This should create your asset bundle in a folder in the root directory called "StreamingAssets".
6. Copy the asset bundle and asset bundle .manifest files from StreamingAssets into your mod's "planets" folder. If you did everything properly they should work in game. To double check everything is included, open the .manifest file in a text editor to see the files included and their paths.

#### Dialogue
An array of object, each one has the following properties:
- "position": (position) 
- "radius" : (decimal number) the spherical volume the player must look at to talk to this character
- "xmlFile" : (string) The filepath to the XML file which defines this dialogue
- "remoteTriggerPosition" : (position) This is optional. If you want a certain volume to trigger the player to engage with the dialogue from a distance, set this position.

#### XML
Here's an example dialogue XML:

```xml
<DialogueTree>
	<NameField>EXAMPLE NPC</NameField>

	<DialogueNode>
		<Name>Start</Name>
		<EntryCondition>DEFAULT</EntryCondition>
		<Dialogue>
			<Page>Start</Page>
      		<Page>Start Part 2</Page>
		</Dialogue>
		
		<DialogueOptionsList>
			<DialogueOption>
				<Text>Goto 1</Text>
				<DialogueTarget>1</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto 2</Text>
				<DialogueTarget>2</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto End</Text>
				<DialogueTarget>End</DialogueTarget>
			</DialogueOption>
		</DialogueOptionsList>
	</DialogueNode>
	
	<DialogueNode>
		<Name>1</Name>
		<Dialogue>
			<Page>This is 1</Page>
		</Dialogue>
		
		<DialogueOptionsList>
			<DialogueOption>
				<Text>Goto 2</Text>
				<DialogueTarget>2</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto End</Text>
				<DialogueTarget>End</DialogueTarget>
			</DialogueOption>
		</DialogueOptionsList>
	</DialogueNode>


	<DialogueNode>
		<Name>2</Name>
		<Dialogue>
			<Page>This is 2</Page>
		</Dialogue>

		<DialogueOptionsList>
			<DialogueOption>
				<Text>Goto 1</Text>
				<DialogueTarget>1</DialogueTarget>
			</DialogueOption>
			<DialogueOption>
				<Text>Goto End</Text>
				<DialogueTarget>End</DialogueTarget>
			</DialogueOption>
		</DialogueOptionsList>

	</DialogueNode>
	
	<DialogueNode>
		<Name>End</Name>
		<Dialogue>
			<Page>This is the end</Page>
		</Dialogue>
	</DialogueNode>

</DialogueTree>
```	  

### Ring
- "innerRadius" : (decimal number)
- "outerRadius" : (decimal number)
- "inclination" : (decimal number) 
- "longitudeOfAscendingNode" : (decimal number)
- "texture" : (string) The file path to the image used for the rings. You can either give it a full image of the rings or a 1 pixel wide strip.
- "rotationSpeed" : (decimal number)
- "curve" : (scale curve) Allows for changing the ring's size over time.

### Spawn
- "playerSpawnPoint" : (position) If you want the player to spawn on the new body, set a value for this. Press "P" in game to have the game log the position you're looking at to find a good value for this.
- "shipSpawnPoint" : (position)
- "startWithSuit" : (true/false) If you spawn on a planet with no oxygen, you probably want to set this to true ;)

### Star
Use this if you are creating a star.
- "size" : (decimal number)
- "tint" : (colour) 
- "solarFlareTint" : (colour) The flares are tinted weirdly so this won't make the actual colour of the star. You'll want to use trial and error to find something that matches.
- "solarLuminosity" : (decimal number) Relative luminosity of this star compared to the sun.
- "curve" : (scale curve)

### Signal
- "signals" : (list of signal info objects)

Signal info objects can then have the following values set:
- "position" : (position) To find a good value for this, fly to the planet, look directly at where you want the signal to come from, and press "P" to have the game log the position you're looking at.
- "frequency" : (string) There are 7 acceptible values for this:
	- "Default" : appears in game as ???
	- "Traveler" : appears in game as "Outer Wilds Ventures"
	- "Quantum" : appears in game as "Quantum Fluctuations"
	- "EscapePod" : appears in game as "Distress Signal"
	- "Statue" : appears in game as "Nomai Statue"
	- "WarpCore" : appears in game as "Anti-Graviton Flux" 
	- "HideAndSeek" : appears in game as "Hide and Seek"
	- "Radio" : appears in game as "Deep Space Radio"
- "name" : (string) The name as it will appear in game
- "audioClip" : (string) The audio clip from the game you want to use (can find these using Unity Explorer or by datamining)
- "audioFilePath" : (string) The file path to a .wav you want to use as the audio clip
- "sourceRadius" : (decimal number) The radius of the spherical volume the signal appears to come from
- "detectionRadius" : (decimal number) How close you must be to get the "Unidentified signal detected" pop-up
- "identiicationRadius" : (decimal number) How close you must get to identify the signal
- "onlyAudibleToScope" : (true/false) 
- "insideCloak" : (true/false) You have to set this to true if the signal is inside a cloaking field

Here's an example of what all this looks like, for more check my [Signals+](https://github.com/xen-42/outer-wilds-signals-plus) add-on:
```json
"Signal" : 
{
	"Signals" : 
	[
		{ "Frequency" : "Statue", "Name" : "Museum Statue", "AudioClip" : "OW NM Flashback 082818 AP loop", "SourceRadius" : 1, "Position" : {"x": -76.35, "y": 12, "z": 214.7 } },
		{ "Frequency" : "WarpCore", "Name" : "Timber Hearth Receiver", "AudioClip" : "OW_NM_WhiteHoleAmbienceL", "SourceRadius" : 0.5, "Position" : {"x": -237.8, "y": -50.8, "z": -59.2 } }
	]
}
```

### Singularity
This allows you to make black holes and white holes, and to pair them.
- "size" : (decimal number)
- "pairedSingularity" : (string) The singularity you want this one to pair to. Must be the opposite type. If you don't set this, the singularity will not transport you, and if it is a black hole it will kill you on entry.
- "type" : (string) Put either "BlackHole" or "WhiteHole".
- "targetStarSystem" : (string) You can have a black hole bring you to a different star system scene using this. 

### Water
- "size" : (decimal number) highest radius of the water volume
- "tint" : (colour)
- "curve" : (scale curve)

### Lava
- "size" : (decimal number) height radius of the lava volume
- "curve" : (scale curve)

### Sand
- "size" : (decimal number) highest radius of the sand volume
- "tint" : (colour)
- "curve" : (scale curve)

### Funnel
- "target" : (string) The name of the body that the funnel will flow onto.
- "type" : (string) Can be sand, water, lava, or star.
- "tint" : (colour)
- "curve": (scale curve)

### How to destroy existing planets

You do this (but with the appropriate name) as it's own config.
```json
{
	"name" : "Ember Twin",
	"destroy" : true,
}
```

Remember that if you destroy Timber Hearth you better put a [Spawn](#spawn) module on another planet. If you want to entirely replace the solar system you can restroy everything, including the sun. You can use the prefabs from my [Real Solar System](https://github.com/xen-42/outer-wilds-real-solar-system) addon, in the `planets/0 - original planets` folder. Also, deleting a planet destroys anything orbiting it, so if you want to replace the solar system you can just destroy the sun.

### How to update existing planets

Similar to above, make a config where "Name" is the name of the planet. The name should be able to just match their in-game english names, however if you encounter any issues with that here are the in-code names for planets that are guaranteed to work: `SUN`, `CAVE_TWIN` (Ember Twin), `TOWER_TWIN` (Ash Twin), `TIMBER_HEARTH`, `BRITTLE_HOLLOW`, `GIANTS_DEEP`, `DARK_BRAMBLE`, `COMET` (Interloper), `WHITE_HOLE`, `WHITE_HOLE_TARGET` (Whitehole station I believe), `QUANTUM_MOON`, `ORBITAL_PROBE_CANNON`, `TIMBER_MOON` (Attlerock), `VOLCANIC_MOON` (Hollow's Lantern), `DREAMWORLD`, `MapSatellite`, `RINGWORLD` (the Stranger).

Only some of the above modules are supported (currently) for existing planets. Things you cannot modify for existing planets include: heightmaps, procedural generation, gravity, or their orbits. You also can't make them into stars or binary focal points (but why would you want to, just delete them and replace them entirely). However this still means there are many things you can do: completely change their atmospheres, give them rings, asteroid belts, comet tails, lava, water, prop details, or signals. 

You can also delete parts of an existing planet. Here's part of an example config which would delete the rising sand from Ember Twin:
```json
"name" : "Ember Twin",
"childrenToDestroy" : ["SandSphere_Rising"],
```

In `childrenToDestroy` you list the relative paths for the children of the planet's gameObject that you want to delete.

## How to use New Horizons in other mods

First create the following interface in your mod:

```cs
public interface INewHorizons
{
    void Create(Dictionary<string, object> config, IModBehaviour mod);

    void LoadConfigs(IModBehaviour mod);

    GameObject GetPlanet(string name);
}
```

In your main `ModBehaviour` class you can get the NewHorizons API like so:
```cs
INewHorizons NewHorizonsAPI = ModHelper.Interaction.GetModApi<INewHorizons>("xen.NewHorizons")
```

You can then use the API's `LoadConfigs()` method to load from a "planets" folder, or use the `Create()` and `GetPlanet` methods to create planets and do whatever with them. Just make sure you create planets in the `Start()` method or at least before the SolarSystem scene loads, or they will not be created.

## Helpful resources

The texturemap/heightmap feature was inspired by the KSP mod Kopernicus. A lot of the same techniques that apply to planet creation there apply to New Horizons. You can check out a planetary texturing guide repository [here](https://forum.kerbalspaceprogram.com/index.php?/topic/165285-planetary-texturing-guide-repository/).

[Photopea](https://www.photopea.com/) is a free browser-based photo editor which has useful features like rectangular-to-polar coordinate transformation, useful for fixing abnormalities at the poles of your planets. 

## Contact

Join the [Outer Wilds Modding Discord](https://discord.gg/MvbCbBz6Q6) if you have any questions or just want to chat about modding! Theres a New Horizons category there dedicated to discussion of this mod.

## Credits
Authors:
- xen (New Horizons v0.1.0 onwards)
- [Mister_Nebula](https://github.com/misternebula) (Marshmallow v0.1 to v1.1.0)

New Horizons was made with help from:
- [jtsalomo](https://github.com/jtsalomo): Implemented [OW_CommonResources](https://github.com/PacificEngine/OW_CommonResources) support introduced in v0.5.0
- [Raicuparta](https://github.com/Raicuparta): Integrated the [New Horizons Template](https://github.com/xen-42/ow-new-horizons-config-template) into the Outer Wilds Mods website
- [Nageld](https://github.com/Nageld): Set up xml reading for custom dialogue in v0.8.0
- [Bwc9876](https://github.com/Bwc9876): Set up ship log entires for planets in v0.8.2

Marshmallow was made with help from:
- TAImatem
- AmazingAlek
- Raicuparta
- and the Outer Wilds discord server.
