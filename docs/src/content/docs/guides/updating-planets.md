---
title: Update Existing Planets
description: A guide for updating base-game planets in New Horizons
---

Similar to above, make a config where "Name" is the name of the planet. The name should be able to just match their in-game english names, however if you encounter any issues with that here are the in-code names for planets that are guaranteed to work (case sensitive!):

- `SUN`
- `CAVE_TWIN` (Ember Twin)
- `TOWER_TWIN` (Ash Twin)
- `TIMBER_HEARTH`
- `BRITTLE_HOLLOW`
- `GIANTS_DEEP`
- `DARK_BRAMBLE`
- `COMET` (Interloper)
- `WHITE_HOLE`
- `WhiteholeStation`
- `QUANTUM_MOON`
- `ORBITAL_PROBE_CANNON`
- `TIMBER_MOON` (Attlerock)
- `VOLCANIC_MOON` (Hollow's Lantern)
- `DREAMWORLD`
- `MapSatellite`
- `RINGWORLD` (The Stranger)

Some features will not work if you try to add them to a base planet config. These include:

- FocalPoints (just makes no sense really, a focal point is meant to be a intangible point between two binary bodies).
- Gravity (including the strength, fall-off, and the size of the gravitational sphere of influence)
- Reference frames (the volume used for targetting a planet with your ships navigation systems)

You can also delete parts of an existing planet. Here's part of an example config which would delete the rising sand from Ember Twin:

```json title="EmberTwin.json"
{
    "name": "Ember Twin",
    "removeChildren": ["SandSphere_Rising"]
}
```

In `removeChildren` you list the relative paths for the children of the planet's gameObject that you want to delete. Relative path meaning it does not include the root planet game object (in this case it would be `EmberTwin_Body`).

## Destroy Existing Planets

You do this (but with the appropriate name) as its own config.

```json title="EmberTwin.json"
{
    "name": "Ember Twin",
    "destroy": true
}
```

Note that destroying a planet will destroy anything orbiting it. For instance, destroying Giants Deep will automatically destroy the Orbital Probe Cannon. If you destroy the sun, it will destroy the entire solar system. This is not recommended, since if you want to start a new solar system from scratch you should use the `starSystem` field to create your own system.

Remember that if you destroy Timber Hearth you must put a `Spawn` module on another planet, since you just destroyed the default spawn location.
