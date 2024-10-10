---
title: Dialogue
description: Guide to making dialogue in New Horizons
---

This page goes over how to use dialogue in New Horizons.

## Understanding Dialogue

### Dialogue Tree

A dialogue tree is an entire conversation, it's made up of dialogue nodes.

### Dialogue Node

A node is a set of pages shown to the player followed by options the player can choose from to change the flow of the conversation.

### Condition

A condition is a yes/no value stored **for this loop and this loop only**. It can be used to show new dialogue options, stop someone from talking to you (looking at you Slate), and more.

### Persistent Condition

A persistent condition is similar to a condition, except it _persists_ through loops, and is saved on the players save file.

### Remote Trigger

A remote trigger is used to have an NPC talk to you from a distance; ex: Slate stopping you for the umpteenth time to tell you information you already knew.

### ReuseDialogueOptionsListFrom

This is a custom XML node introduced by New Horizons. Use it when adding new dialogue to existing characters, to repeat the dialogue options list from another node.

For example, Slate's first dialogue with options is named `Scientist5`. To make a custom DialogueNode using these dialogue options (meaning new dialogue said by Slate, but reusing the possible player responses) you can write:

```xml
<DialogueNode>
    <Name>...</Name>
    <Dialogue>
        <Page>NEW DIALOGUE FOR SLATE HERE.</Page>
    </Dialogue>
    <DialogueOptionsList>
        <ReuseDialogueOptionsListFrom>Scientist5</ReuseDialogueOptionsListFrom>
    </DialogueOptionsList>
</DialogueNode>
```

## Example XML

Here's an example dialogue XML:

```xml title="ExampleDialogue.xml"
<!-- Example Dialogue -->
<!-- All files must have `DialogueTree` as the root element, the xmlns:xsi=... and xsi:noNamespaceSchemaLocation=... is optional but provides improved error checking if your editor supports it -->
<DialogueTree xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xsi:noNamespaceSchemaLocation="https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/dialogue_schema.xsd">
 <NameField>EXAMPLE NPC</NameField> <!-- The name of this character -->

 <DialogueNode> <!-- A dialogue node is a set of pages displayed to the player optionally followed by options -->
  <Name>Start</Name> <!-- The name of this node, used to go to this node from another node -->
  <EntryCondition>DEFAULT</EntryCondition> <!-- The condition that must be met for this node to be reached; A file should always have a node with "DEFAULT" -->
  <Dialogue> <!-- The actual dialogue we want to show the player -->
   <Page>Start</Page> <!-- A single page of the dialogue -->
        <Page>Start Part 2</Page> <!-- Another page -->
  </Dialogue>

  <DialogueOptionsList> <!-- Show options the player can choose from when the character is done talking -->
   <DialogueOption> <!-- A single option the player can pick -->
    <Text>Goto 1</Text> <!-- The text to display for the option -->
    <DialogueTarget>1</DialogueTarget> <!-- The name of the node to jump to -->
   </DialogueOption>
   <!-- A few more options... -->
   <DialogueOption>
    <Text>Goto 2</Text>
    <DialogueTarget>2</DialogueTarget>
   </DialogueOption>
   <DialogueOption>
    <Text>Goto End</Text>
    <DialogueTarget>End</DialogueTarget>
   </DialogueOption>
  </DialogueOptionsList>
 </DialogueNode>

 <DialogueNode> <!-- Another node -->
  <Name>1</Name> <!-- Name of the node -->
  <!-- (Note the lack of an EntryCondition) -->
  <Dialogue>
   <Page>This is 1</Page>
  </Dialogue>

  <DialogueOptionsList>
   <DialogueOption>
    <Text>Goto 2</Text>
    <DialogueTarget>2</DialogueTarget>
   </DialogueOption>
   <DialogueOption>
    <Text>Goto End</Text>
    <DialogueTarget>End</DialogueTarget>
   </DialogueOption>
  </DialogueOptionsList>
 </DialogueNode>

 <DialogueNode> <!-- Another node why not -->
  <Name>2</Name>
  <Dialogue>
   <Page>This is 2</Page>
  </Dialogue>

  <DialogueOptionsList>
   <DialogueOption>
    <Text>Goto 1</Text>
    <DialogueTarget>1</DialogueTarget>
   </DialogueOption>
   <DialogueOption>
    <Text>Goto End</Text>
    <DialogueTarget>End</DialogueTarget>
   </DialogueOption>
  </DialogueOptionsList>
 </DialogueNode>

 <DialogueNode> <!-- The end node -->
  <Name>End</Name>
  <Dialogue>
   <Page>This is the end</Page>
  </Dialogue>
  <!-- When a node doesn't have any options defined the dialogue box will close once the pages have been read -->
 </DialogueNode>
</DialogueTree>
```

## Using the XML

To use the dialogue XML you have created, you simply need to reference it in the `dialogue` prop

```json
{
    "Props": {
        "dialogue": [
            {
                "position": { "x": 5, "y": 10, "z": 0 },
                "xmlFile": "planets/path/to/your_file.xml"
            }
        ]
    }
}
```

## Dialogue Config

To view the options for the dialogue prop, check [the schema](/schemas/body-schema/defs/propmodule#dialogue)

## Controlling Conditions

You can set condition in dialogue with the `<SetCondition>` and `<SetPersistentCondition>` tags

```xml {3-4}
<DialogueNode>
 <!-- ... -->
 <SetCondition>EXAMPLE_CONDITION</SetCondition>
 <SetPersistentCondition>EXAMPLE_P_CONDITION</SetPersistentCondition>
 <!-- ... -->
</DialogueNode>
```

## Dialogue Options

There are many control structures for dialogue options to hide/reveal them if conditions are met. Take a look at [the DialogueOption schema](/schemas/dialogue-schema/defs/dialogueoption#DialogueOption-DialogueTarget) for more info.

## Controlling Flow

In addition to `<DialogueOptions>`, there are other ways to control the flow of the conversation.

### DialogueTarget

Defining `<DialogueTarget>` in the `<DialogueNode>` tag instead of a `<DialogueOption>` will make the conversation go directly to that target after the character is done talking.

### DialogueTargetShipLogCondition

Used in tandem with `DialogueTarget`, makes it so you must have a [ship log fact](/guides/ship-log#explore-facts) to go to the next node.

### Adding to existing dialogue

Here's an example of how to add new dialogue to Slate, without overwriting their existing dialogue. This will also allow multiple mods to all add new dialogue to the same character.

```xml {3-4}
<DialogueTree>
    <DialogueNode>
        <Name>Scientist5</Name>
        <DialogueOptionsList>
            <DialogueOption>
                <Text>Hi how are you?</Text>
                <DialogueTarget>example_new_slate_Text</DialogueTarget>
            </DialogueOption>
        </DialogueOptionsList>
    </DialogueNode>
    <DialogueNode>
        <Name>example_new_slate_Text</Name>
        <Dialogue>
            <Page>I'm good!</Page>
        </Dialogue>
    </DialogueNode>
</DialogueTree>
```

NH will merge together `<DialogueNode>` nodes that have the same `<Name>` field, adding their `<DialogueOptionsList>` together. No other changes will be merged.

NH can also add new `<DialogueNode>` nodes into the text, however you have to add `<DialogueOption>`s that link to them for them to ever be read by the player.

Be careful to use unique names to ensure optimal compatibility between mods. Consider prefixing the names of your nodes with the name of your mod.

To use this additional dialogue you need to reference it in a planet config file:

```json
"dialogue": [
    {
        "pathToExistingDialogue": "Sector_TH/Sector_Village/Sector_StartingCamp/Characters_StartingCamp/Villager_HEA_Slate/ConversationZone_RSci",
        "xmlFile": "planets/text/Slate.xml"
    }
]
```
