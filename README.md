![new horizons thumbnail 2](https://user-images.githubusercontent.com/22628069/154112130-b777f618-245f-44c9-9408-e11141fc5fde.png)


[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dxen_42%26type%3Dpatrons&style=flat)](https://patreon.com/xen_42)
![Current version](https://img.shields.io/github/manifest-json/v/xen-42/outer-wilds-new-horizons?color=gree&filename=NewHorizons%2Fmanifest.json)
![Downloads](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/total)
![Downloads of newest version](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/latest/total)
![Latest release date](https://img.shields.io/github/release-date/xen-42/outer-wilds-new-horizons)
[![Build](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml/badge.svg)](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml)

*Do you want to create planets using New Horizons?* Then check out our [website](https://nh.outerwildsmods.com/) for all our documentation!

A custom world creation tool for Outer Wilds.

You can view the addons creators have made (or upload one yourself) [here](https://outerwildsmods.com/custom-worlds)!

Check the ship's log for how to use your warp drive to travel between star systems!

<!-- TOC -->

- [Incompatible mods](#incompatible-mods)
- [Roadmap](#roadmap)
- [Contact](#contact)
- [Credits](#credits)

<!-- /TOC -->

## Incompatible mods
- Autoresume.
- Multiplayer mods like QSB and OWO. QSB actually kinda works decently as long as you don't warp.

## Features
- Heightmaps/texturemaps
- Remove existing planets
- Stars
- Binary orbits
- Comets
- Signalscope signals
- Asteroid belts 
- Support satellites 
- Surface scatter: rocks, trees, etc, using in-game models, or custom ones 
- Load planet meshes from asset bundle 
- Black hole / white hole pairs 
- Separate solar system scenes accessible via wormhole
- Warp drive with target set in ship's log
- Implement custom dialogue 
- Make a [template Unity project](https://github.com/xen-42/outer-wilds-unity-template) to use with NH, including all game scripts recovered using UtinyRipper to make AssetBundle creation easier.
- Custom ship log entries and fact reveals, for rumour mode and map mode
- Funnels (sand/water/lava/star)
- Variable surface height (sand/water/lava/star)

## Roadmap
- Procedural terrain generation (started)
- "Quantum" planet parameters
- Better terrain and water LOD
- Edit existing planet orbits
- Implement all planet features:
	- Tornados + floating islands
	- Let any star go supernova
	- Geysers
	- Meteors
	- Pocket dimensions
	- Timed position/velocity changes
- Implement custom Nomai scrolls
- Implement custom translatable writing
- Destroy planets that fall into a star

## Contact
Join the [Outer Wilds Modding Discord](https://discord.gg/MvbCbBz6Q6) if you have any questions or just want to chat about modding! Theres a New Horizons category there dedicated to discussion of this mod.

## Credits
Authors:
- xen (New Horizons v0.1.0 onwards)
- [Mister_Nebula](https://github.com/misternebula) ([Marshmallow](https://github.com/misternebula/Marshmallow) v0.1 to v1.1.0)

New Horizons was made with help from:
- [jtsalomo](https://github.com/jtsalomo): Implemented [OW_CommonResources](https://github.com/PacificEngine/OW_CommonResources) support introduced in v0.5.0
- [Raicuparta](https://github.com/Raicuparta): Integrated the [New Horizons Template](https://github.com/xen-42/ow-new-horizons-config-template) into the Outer Wilds Mods website
- [Nageld](https://github.com/Nageld): Set up xml reading for custom dialogue in v0.8.0
- [Bwc9876](https://github.com/Bwc9876): Set up ship log entries and QOL debug options in v0.9.x. Set up the website.

Translation credits:
- Russian: GrayFix and Tlya
- German: Nolram
- Spanish: Ciborgm9 and Ink

Marshmallow was made with help from:
- TAImatem
- AmazingAlek
- Raicuparta
- and the Outer Wilds discord server.
