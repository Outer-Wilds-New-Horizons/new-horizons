![new horizons thumbnail 2](https://user-images.githubusercontent.com/22628069/154112130-b777f618-245f-44c9-9408-e11141fc5fde.png)


[![Support me on Patreon](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dxen_42%26type%3Dpatrons&style=flat)](https://patreon.com/xen_42)
![Current version](https://img.shields.io/github/manifest-json/v/xen-42/outer-wilds-new-horizons?color=gree&filename=NewHorizons%2Fmanifest.json)
![Downloads](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/total)
![Downloads of newest version](https://img.shields.io/github/downloads/xen-42/outer-wilds-new-horizons/latest/total)
[![Build](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml/badge.svg)](https://github.com/xen-42/outer-wilds-new-horizons/actions/workflows/build.yaml)

A custom world creation tool for Outer Wilds.

You can view the addons creators have made (or upload one yourself) [here](https://outerwildsmods.com/custom-worlds)!

Check the ship's log for how to use your warp drive to travel between star systems!

*Do you want to create planets using New Horizons?* Then check out our [website](https://nh.outerwildsmods.com/) for all our documentation!

<!-- TOC -->

- [Incompatible mods](#incompatible-mods)
- [Roadmap](#roadmap)
- [Contact](#contact)
- [Credits](#credits)

<!-- /TOC -->

## Incompatible mods
- Autoresume.
- Multiplayer mods like QSB and OWO. QSB actually kinda works decently as long as you don't warp.

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
- Custom ship log entries and fact reveals, for rumour mode and map mode (done)
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
- Russian: GrayFix and Tyla
- German: Nolram
- Spanish: Ciborgm9 and Ink

Marshmallow was made with help from:
- TAImatem
- AmazingAlek
- Raicuparta
- and the Outer Wilds discord server.
