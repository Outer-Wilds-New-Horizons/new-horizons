![new horizons thumbnail 2](https://user-images.githubusercontent.com/22628069/154112130-b777f618-245f-44c9-9408-e11141fc5fde.png)

[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dxen42%26type%3Dpatrons&style=flat)](https://patreon.com/xen42)
[![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://www.paypal.com/paypalme/xen42)
![Current version](https://img.shields.io/github/manifest-json/v/xen-42/outer-wilds-new-horizons?color=gree&filename=NewHorizons%2Fmanifest.json)
![Downloads](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/total)
![Downloads of newest version](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/latest/total)
![Latest release date](https://img.shields.io/github/release-date/xen-42/outer-wilds-new-horizons)
[![Build](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml/badge.svg)](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml)

_Do you want to create your own story mod using New Horizons?_ Then check out our [website](https://nh.outerwildsmods.com/) for all our documentation!

If you want to see examples of what NH can do check out the [examples add-on](https://github.com/xen-42/ow-new-horizons-examples) or [real solar system add-on](https://github.com/xen-42/outer-wilds-real-solar-system).

Check the ship's log for how to use your warp drive to travel between star systems!

<!-- TOC -->

-   [Incompatible mods](#incompatible-mods)
-   [Supported mods](#supported-mods)
-   [Development](#development)
-   [Contact](#contact)
-   [Credits](#credits)

<!-- /TOC -->

## Incompatible mods

New Horizons conflicts with the mod Common Resources. This mod is a requirement for other mods such as OW Randomizer (not to be confused with Archipelago Randomizer) and Difficulty Mod.

Why do these two mods conflict? Common Resources is a mod which reimplements many of the game's features underneath the hood, for one reason or another. For instance, it completely overhauls how the orbits of planets work, as this is a requirement for it to support OW Randomizer. It does this even when you are only using Cheats Mod. In particular, having CR installed seems to, for whatever reason, break character dialogue introduced by New Horizons. As CR is no longer actively maintained, it is unlikely this issue will be resolved any time soon. 

## Supported Mods

New Horizons has optional support for a few other mods:

-   [Discord Rich Presence](https://outerwildsmods.com/mods/discordrichpresence/): Showcase what New Horizons worlds you're exploring in your Discord status!
-   [Voice Acting Mod](https://outerwildsmods.com/mods/voiceactingmod/): Characters in NH can be given voice lines which will work with this mod installed. Try it out by downloading NH Examples and talking to Ernesto!
-   [Achievements+](https://outerwildsmods.com/mods/achievements/): New Horizons and its addons have achievements you can unlock with this mod installed!

## Features

NH allows you to create planets:
- Load planet meshes or details from asset bundles made in Unity
- Create custom planets from heightmaps/texturemaps with support for triplanar mapping
- Add stock planet features: dialogue, Nomai text, geysers, cloaking fields, meteor-launching volcanoes, rafts, tornados, Dark Bramble seeds/nodes, black/white holes, funnels, lava/oceans/sand, signalscope signals/frequencies and others.
- Edit existing planets: Remove them, alter their orbits, add/delete objects from them.
- All these features work in both the main solar system and the Eye of the Universe scene

Even if the majority of your story mod is going to be done in Unity, NH still offers useful features which will improve mod-compatibility and stop you reinventing the wheel:
- NH allows you to separate your story mod into its own solar system scene accessible via wormhole OR via the ship's new warp drive feature found in the ship's log.
- NH allows you to create custom ship log entries.
- NH allows you to create Inhabitant slide reels with asynchronous loading optimization.
- NH has a localization system to translate ship logs, dialogue, Nomai text, and custom UI elements.
- NH allows you to add new characters to the cosmic-jam-session at the Eye of the Universe while ensuring mod compatibility

Use our [template Unity project](https://github.com/ow-mods/outer-wilds-unity-template) to create assets for use in NH, including all game scripts recovered using UtinyRipper

## Development

If you want to help (please dear god help us) then check out the [contact](#contact) info below or the [contributing](https://github.com/Outer-Wilds-New-Horizons/new-horizons/blob/master/CONTRIBUTING.md) page.

The Unity project we use to make asset bundles for this mod is [here](https://github.com/Outer-Wilds-New-Horizons/nh-unity).

## Contact

Join the [Outer Wilds Modding Discord](https://discord.gg/MvbCbBz6Q6) if you have any questions or just want to chat about modding! Theres a New Horizons category there dedicated to discussion of this mod.

## Credits

Main authors:

-   [xen](https://github.com/xen-42)
-   [Bwc9876](https://github.com/Bwc9876) (New Horizons v0.9.0 onwards)
-   [JohnCorby](https://github.com/JohnCorby)
-   [MegaPiggy](https://github.com/MegaPiggy)
  
New Horizons was made with help from:
-   [FreezeDriedMangos](https://github.com/FreezeDriedMangos)
-   [Trifid](https://github.com/TerrificTrifid)
-   [Hawkbar](https://github.com/Hawkbat)
-   And many others, see the [contributors](https://github.com/Outer-Wilds-New-Horizons/new-horizons/graphs/contributors) page.

Translation credits:

-   Russian: Tlya
-   German: Nolram
-   Spanish: Ciborgm9, Ink, GayCoffee
-   French: xen
-   Japanese: TRSasasusu
-   Portuguese: avengerx, loco-choco

New Horizons was based off [Marshmallow](https://github.com/misternebula/Marshmallow) made by:

-   [\_nebula](https://github.com/misternebula)

with help from:

-   TAImatem
-   AmazingAlek
-   Raicuparta
-   and the Outer Wilds discord server.
