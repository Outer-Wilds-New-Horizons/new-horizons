---
title: Star Systems
description: A guide to creating a custom star system in New Horizons
---

Welcome! This page outlines how to edit a custom star system.

## Getting Started

Star Systems are placed in a folder called systems within your mod folder.

The name of your star system config must be the same as the unique id used in the `starSystem` field of your planet configs. So if you used `xen.RealSolarSystem` as the `starSystem` in your planet, the star system's JSON file would have to be named `xen.RealSolarSystem.json`.

A star system config file will look something like this:

```json title="my_star_system.json"
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/star_system_schema.json",
    "travelAudio": "assets/Travel.mp3",
    "Vessel": {
        "coords": {
            "x": [4, 0, 3, 1],
            "y": [0, 5, 4],
            "z": [5, 4, 0, 3, 1]
        },
        "vesselSpawn": {
            "position": {
                "x": 0,
                "y": 0,
                "z": 8000
            }
        }
    }
}
```

To see all the different things you can put into a config file check out the [Star System Schema](/schemas/star-system-schema).

## Vessel Coordinates

You can warp to custom star systems via the Nomai vessel. Each coordinate has to be 2-6 points long.
These are the points for each coordinate node. When making your unique coordinate you should only use each point once.

![nomaiCoordinateIndexes](@/assets/docs-images/star_systems/nomai_coordinate_indexes.webp)

### Hearthian Solar System Vessel Coordinates

You can use these coordinates to warp back to the hearthian solar system.

![hearthianSolarSystemCoordinates](@/assets/docs-images/star_systems/hearthian_solar_system_coordinates.webp)
