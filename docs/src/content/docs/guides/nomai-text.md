---
title: Nomai Text
description: Guide to making Nomai Text in New Horizons
---

This page goes over how to use Nomai text in New Horizons.

## Understanding Nomai Text

Nomai text is the backbone of many story mods. There are two parts to setting up Nomai text: The XML file and the planet config.

### XML

In your XML, you define the actual raw text which will be displayed, the ship logs it unlocks, and the way it branches. See [the Nomai text XML schema](/schemas/text-schema/) for more info.

Nomai text contains a root `<NomaiObject>` node, followed by `<TextBlock>` nodes and optionally a `<ShipLogConditions>` node.

Nomai text is made up of `TextBlock`s. Each text block has an `ID` which must be unique (you can just number them for simplicity). After the first defined text block, each must have a `ParentID`. For scrolls and regular wall text, the text block only gets revealed after its parent block. Multiple text blocks can have the same parent, allowing for branching paths. In recorders and computers, each text block must procede in order (the second parented to the first, the third to the second, etc). In cairns, there is only one text block.

To unlock ship logs after reading each text block, add a `<ShipLogConditions>` node. This can contains multiple `<RevealFact>` nodes, each one defining a `<FactID>`, `<Condition>` (which contains a comma delimited list). The ship log conditions node can either have `<LocationA/>` or `<LocationB/>`, which means the logs will unlock only if you are at that location. The `<Condition>` list links a fact to a specific text block.

### Json

In your planet config, you must define where the Nomai text is positioned. See [the translator text json schema](/schemas/body-schema/defs/translatortextinfo/) for more info.

You can input a `seed` for a wall of text which will randomly generate the position of each arc. To test out different combinations, just keep incrementing the number and then hit "Reload Configs" from the pause menu with debug mode on. This seed ensures the same positioning each time the mod is played. Alternatively, you can use `arcInfo` to set the position and rotation of all text arcs, as well as determining their types (adult, teenager, child, or Stranger). The various age stages make the text look messier, while Stranger allows you to make a translatable version of the DLC text.