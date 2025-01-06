---
title: Helpful Resources
description: Helpful resources for developing New Horizons addons
---

## Schemas Are Your Friend

The [schemas](/schemas/body-schema) are the best place to go for up-to-date info on what you can do with New Horizons.

They're automatically generated directly from the code, so they're always up-to-date.

You can also use them to get autocomplete and tooltips in your code editor.

### JSON Schemas

JSON schemas are automatically supported by VSCode, so you can get autocomplete and tooltips for the schemas, just specify it using `$schema` in your JSON file.

You can get the URL to the schema by clicking the "View Raw" button on the schema page. Copy the link and paste it into your `$schema` field.

### XML Schemas

If you're using VSCode, you can use the [XML addon](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml) to get autocomplete and tooltips for the schemas.

You can get the URL to the schema by clicking the "View Raw" button on the schema page. Copy the link and paste it into your `xsi:noNamespaceSchemaLocation` field.

## Reloading Configs In-Game

It can get annoying when you have to keep closing and opening the game over and over again to test changes, that's why New Horizons has a "Reload Configs" feature.
To enable it, head to your Mods menu and select New Horizons and check the box that says Debug, this will cause a "Reload Configs" option to appear in your pause menu which will reload changes from your filesystem.
You may also notice blue and yellow logs start appearing in your console, this is New Horizons providing additional info on what it's currently doing, it can be helpful when you're trying to track down an issue.

## Templates

There are two templates for New Horizons addons.
The [New Horizons Addon Template](https://github.com/xen-42/ow-new-horizons-config-template) is used for addons that **don't use custom code**,
this is ideal for simple projects and people just starting out.

The [Outer Wilds Mod Template](https://github.com/ow-mods/ow-mod-template) is used for mods that use custom code,
**you must enable "Use New Horizons" in order for it to work with New Horizons**.
This is ideal for people that want to expand on New Horizons and add custom behaviour.

## Texturing

The texturemap/heightmap feature was inspired by the Kerbal Space Program mod Kopernicus. A lot of the same techniques that apply to
planet creation there apply to New Horizons. If you need help with planetary texturing, check out [The KSP texturing guide](https://forum.kerbalspaceprogram.com/index.php?/topic/165285-planetary-texturing-guide-repository/).

[Photopea](https://www.photopea.com/) is a free browser-based photo editor which has useful features like
rectangular-to-polar coordinate transformation, useful for fixing abnormalities at the poles of your planets.

## Helpful Mods

These mods are useful when developing your addon

- [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) - Used to find the paths of game objects for copying and can be used to manually position props, ship log entries, and more.
- [Collider Visualizer](https://outerwildsmods.com/mods/collidervisualizer) - Useful when creating dialogue triggers or reveal volumes.
- [Save Editor](https://outerwildsmods.com/mods/saveeditor) - Useful when creating a custom [ship log](/ship-log), can be used to reveal all custom facts so you can see them in the ship's computer.
- [Time Saver](https://outerwildsmods.com/mods/timesaver/) - Lets you skip some repeated cutscenes and get into the game faster.
- [The Examples Mod](https://github.com/Outer-Wilds-New-Horizons/nh-examples) - A mod that contains examples of how to use New Horizons features.

## Helpful Tools

These tools/references are highly recommended

- [VSCode](https://code.visualstudio.com/)
- [VSCode XML Addon](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml)
- [XML Basics Tutorial](https://www.w3schools.com/xml/xml_whatis.asp)
- [JSON Basics Tutorial](https://www.tutorialspoint.com/json/index.htm)
- [OWML Docs](https://owml.outerwildsmods.com/)
