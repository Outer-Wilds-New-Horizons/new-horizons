---
title: Volumes
description: Guide to making volumes in New Horizons
---

Volumes are invisible 3D "zones" or "triggers" that cause various effects when objects enter or leave them. For example, `oxygenVolumes` refill the player's oxygen when they enter (used for the various oxygen-generating trees in the game), `forces.directionalVolumes` push players and other physics objects in a specific direction (used by both Nomai artificial gravity surfaces and tractor beams), `revealVolumes` unlock ship log facts when the player enters or observes them (used everywhere in the game), and more.

New Horizons makes adding volumes to your planets easy; just specify them like you would [for a prop](/guides/details/) but under `Volumes` instead of `Props`. For example, to add an oxygen volume at certain location:

```json title="planets/My Cool Planet.json"
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name" : "My Cool Planet",
    "Volumes": {
        "oxygenVolumes": [
            {
                "position": {"x": 399.4909, "y": -1.562098, "z": 20.11444},
                "radius": 30,
                "treeVolume": true,
                "playRefillAudio": true
            }
        ]
    }
}
```

Listing out every type of volume is outside the scope of this guide, but you can see every supported type of volume and the properties they need in [the VolumesModule schema](/schemas/body-schema/defs/volumesmodule/).

## Volume Shapes

By default, volumes are spherical, and you can specify the radius of that sphere with the `radius` property. If you want to use a different shape for your volume, such as a box or capsule, you can specify your volume's `shape` like so:

```json title="planets/My Cool Planet.json"
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name" : "My Cool Planet",
    "Volumes": {
        "forces": {
            "directionalVolumes": [
                {
                    "rename": "ArtificialGravitySurface",
                    "force": 8,
                    "playGravityCrystalAudio": true,
                    "shape": {
                        "type": "box",
                        "size": {
                            "x": 15.0,
                            "y": 10.0,
                            "z": 5.0
                        },
                        "offset": {
                            "x": 0,
                            "y": 5.0,
                            "z": 0
                        }
                    },
                    "position": { "x": 0, "y": -110, "z": 0 },
                    "rotation": { "x": 180, "y": 0, "z": 0 }
                }
            ]
        }
    }
}
```

The supported shape types are: `sphere`, `box`, `capsule`, `cylinder`, `cone`, `hemisphere`, `hemicapsule`, and `ring`. See [the ShapeInfo schema](/schemas/body-schema/defs/shapeinfo/) for the full list of properties available to define each shape.

Note that `sphere`, `box`, and `capsule` shapes are more reliable and efficient than other shapes, so prefer using them whenever possible.

### Debugging

To visualize the shapes of your volumes in-game, use the [Collider Visualizer mod](https://outerwildsmods.com/mods/collidervisualizer/). It will display a wireframe of the shapes around you so you can see precisely where they are and reposition or resize them as needed.
