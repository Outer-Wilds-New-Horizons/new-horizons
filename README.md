![new horizons thumbnail 2](https://user-images.githubusercontent.com/22628069/154112130-b777f618-245f-44c9-9408-e11141fc5fde.png)


[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Downh%26type%3Dpatrons&style=flat)](https://patreon.com/ownh)
[![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg)](https://www.paypal.com/paypalme/xen42)
![Current version](https://img.shields.io/github/manifest-json/v/xen-42/outer-wilds-new-horizons?color=gree&filename=NewHorizons%2Fmanifest.json)
![Downloads](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/total)
![Downloads of newest version](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/latest/total)
![Latest release date](https://img.shields.io/github/release-date/xen-42/outer-wilds-new-horizons)
[![Build](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml/badge.svg)](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml)

*Do you want to create planets using New Horizons?* Then check out our [website](https://nh.outerwildsmods.com/) for all our documentation!

A custom world creation tool for Outer Wilds.

You can view the addons creators have made (or upload one yourself) [here](https://outerwildsmods.com/custom-worlds)!

If you want to see examples of what NH can do check out the [examples add-on](https://github.com/xen-42/ow-new-horizons-examples) or [real solar system add-on](https://github.com/xen-42/outer-wilds-real-solar-system).

Check the ship's log for how to use your warp drive to travel between star systems!

<!-- TOC -->

- [Incompatible mods](#incompatible-mods)
- [Roadmap](#roadmap)
- [Development](#development)
- [Contact](#contact)
- [Credits](#credits)

<!-- /TOC -->

## Incompatible mods
- Autoresume.
- Multiplayer mods like QSB and OWO. QSB actually kinda works decently as long as you don't warp.

## Features
- Load planet meshes or details from asset bundles 
- Use our [template Unity project](https://github.com/xen-42/outer-wilds-unity-template) to create assets for use in NH, including all game scripts recovered using UtinyRipper
- Separate solar system scenes accessible via wormhole OR via the ship's new warp drive feature accessible via the ship's log
- Remove existing planets
- Create planets from heightmaps/texturemaps
- Create stars, comets, asteroid belts, satellites, geysers, cloak fields
- Binary orbits
- Signalscope signals and custom frequencies
- Surface scatter: rocks, trees, etc, using in-game models, or custom ones 
- Black hole / white hole pairs 
- Custom dialogue and custom ship log entries for rumour mode and map mode
- Funnels and variable surface height (can be made of sand/water/lava/star)

## Roadmap
- Procedural terrain generation (started)
- "Quantum" planet parameters
- Better terrain and water LOD
- Edit existing planet orbits (started)
- Implement all planet features:
	- Tornados + floating islands
	- Let any star go supernova
	- Meteors
	- Pocket dimensions
	- Timed position/velocity changes
- Implement custom Nomai scrolls
- Implement custom translatable writing

## Development
If you want to help (please dear god help us) then check out the [contact](#contact) info below.
The Unity project we use to make asset bundles for this mod is [here](https://github.com/xen-42/new-horizons-unity).

## Contact
Join the [Outer Wilds Modding Discord](https://discord.gg/MvbCbBz6Q6) if you have any questions or just want to chat about modding! Theres a New Horizons category there dedicated to discussion of this mod.

## Credits
Main authors:
- xen (New Horizons v0.1.0 onwards)
- [Bwc9876](https://github.com/Bwc9876) (New Horizons v0.9.0 onwards)
- [Mister_Nebula](https://github.com/misternebula) ([Marshmallow](https://github.com/misternebula/Marshmallow) v0.1 to v1.1.0)

New Horizons was made with help from:
- [Nageld](https://github.com/Nageld): Set up xml reading for custom dialogue in v0.8.0
- [jtsalomo](https://github.com/jtsalomo): Implemented [OW_CommonResources](https://github.com/PacificEngine/OW_CommonResources) support introduced in v0.5.0
- [Raicuparta](https://github.com/Raicuparta): Integrated the [New Horizons Template](https://github.com/xen-42/ow-new-horizons-config-template) into the Outer Wilds Mods website

Translation credits:
- Russian: GrayFix and Tlya
- German: Nolram
- Spanish: Ciborgm9 and Ink

Marshmallow was made with help from:
- TAImatem
- AmazingAlek
- Raicuparta
- and the Outer Wilds discord server.
