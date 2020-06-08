# Marshmallow
A planet creator for Outer Wilds.

Planets are created using a JSON file format structure, and placed in the `planets` folder.

Moon support is coming soon!
Support for mods to create custom planets / moons is also coming soon!

<!-- TOC -->

- [Creating a planet file](#creating-a-planet-file-)
- [Submit bugs / Chat about life](#submit-bugs--chat-about-life-)

<!-- /TOC -->

## Creating a planet file :
This list will update as more options are added. Structure JSON file like so :
```
{
  "settings": {
    	"name" : "Test Planet",
      "position" : [0, 0, 20000],
      "orbitAngle" : 0,
      "hasFog" : true,
      "fogTint" : [0, 75, 15, 128],
      "fogDensity" : 0.5
  }
}
```
Everything in "Required" is always needed, and so is every tabbed line in an option.
### Required :
- name - The name of the planet.
- position - The Vector3 positon of the planet in world space. Write as \[x, y, z].
- orbitAngle - The angle of the orbit.

### Optional :
- hasClouds - Set to "true" if you want Giant's Deep-type clouds.
  - topCloudSize - The size of the outer sphere of the clouds.
  - bottomCloudSize - The size of the bumpy clouds underneath the top. *(Check that the bottom clouds are not poking through the top!)*
  - cloudTint - The color of the clouds. Write as \[r, g, b, a] in byte form. (0-255)
- hasWater - Set to "true" if you want water.
  - waterSize - Size of the water sphere.
- hasRain - Set to "true" if you want it to be raining.
- hasGravity - Set to "true" if you want gravity.
  - surfaceAcceleration - Strength of gravity.
- hasMapMarker - Set to "true" if you want the planet name on the map.
- hasFog - Set to "true" if you want fog.
  - fogTint - The color of the fog. Write as \[r, g, b, a] in byte form. (0-255)
  - fogDensity - The thickness of the fog. \[0-1]
  
## Submit bugs / Chat about life :
Did you know we have a nice [Outer Wilds discord](https://discord.gg/Sftcc9Z)? Drop by to submit bugs, ask questions, and chat about whatever.
Join us modding folk in the `#modding` channel!
