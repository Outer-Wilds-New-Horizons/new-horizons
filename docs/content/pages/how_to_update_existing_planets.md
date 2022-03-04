Title: Update Existing Planets
Sort-Priority: 100

### How to update existing planets

Similar to above, make a config where "Name" is the name of the planet. The name should be able to just match their in-game english names, however if you encounter any issues with that here are the in-code names for planets that are guaranteed to work: `SUN`, `CAVE_TWIN` (Ember Twin), `TOWER_TWIN` (Ash Twin), `TIMBER_HEARTH`, `BRITTLE_HOLLOW`, `GIANTS_DEEP`, `DARK_BRAMBLE`, `COMET` (Interloper), `WHITE_HOLE`, `WHITE_HOLE_TARGET` (Whitehole station I believe), `QUANTUM_MOON`, `ORBITAL_PROBE_CANNON`, `TIMBER_MOON` (Attlerock), `VOLCANIC_MOON` (Hollow's Lantern), `DREAMWORLD`, `MapSatellite`, `RINGWORLD` (the Stranger).

Only some of the above modules are supported (currently) for existing planets. Things you cannot modify for existing planets include: heightmaps, procedural generation, gravity, or their orbits. You also can't make them into stars or binary focal points (but why would you want to, just delete them and replace them entirely). However this still means there are many things you can do: completely change their atmospheres, give them rings, asteroid belts, comet tails, lava, water, prop details, or signals. 

You can also delete parts of an existing planet. Here's part of an example config which would delete the rising sand from Ember Twin:
```json
"name" : "Ember Twin",
"childrenToDestroy" : ["SandSphere_Rising"],
```

In `childrenToDestroy` you list the relative paths for the children of the planet's gameObject that you want to delete.

### How to destroy existing planets

You do this (but with the appropriate name) as it's own config.
```json
{
	"name" : "Ember Twin",
	"destroy" : true,
}
```

Remember that if you destroy Timber Hearth you better put a [Spawn](#spawn) module on another planet. If you want to entirely replace the solar system you can restroy everything, including the sun. Also, deleting a planet destroys anything orbiting it, so if you want to replace the solar system you can just destroy the sun. If you're making a brand new star system, you don't have to worry about deleting any existing planets; they won't be there.