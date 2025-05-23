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

### Conditions

In dialogue, the available conversation topics can be limited by what the player knows, defined using dialogue conditions, persistent conditions, and ship log facts. Dialogue can also set conditions to true or false, and reveal ship log facts to the player. This is covered in detail later on this page.

### Remote Trigger

A remote trigger is used to have an NPC talk to you from a distance; ex: Slate stopping you for the umpteenth time to tell you information you already knew.

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

### EntryCondition

The first dialogue node that opens when a player starts talking to a character is chosen using this property. To mark a DialogueNode as beginning the dialogue by default, use the condition DEFAULT (a DialogueTree should always have a node with the DEFAULT entry condition to ensure there is a way to start dialogue).

The entry condition can be either a condition or a persistent condition.

### Condition

A condition is a yes/no value stored **for this loop and this loop only**. It can be used to show new dialogue options, stop someone from talking to you (looking at you Slate), and more.

Conditions can be set in dialogue using `<SetCondition>CONDITION_NAME</SetCondition>`. This can go in a DialogueNode in which case it will set the condition to true when that node is read. There is a similar version of this for DialogueOptions called `<ConditionToSet>CONDITION_NAME</ConditionToSet>` which will set it to true when that option is selected. Conditions can be disabled using `<ConditionToCancel>CONDITION_NAME</<ConditionToCancel>` in a DialogueOption, but cannot be disabled just by entering a DialogueNode. 

You can lock a DialogueOption behind a condition using `<RequiredCondition>CONDITION_NAME</RequiredCondition>`, or remove a DialogueOption after the condition is set to true using `<CancelledCondition>CONDITION_NAME</CancelledCondition>`. 

Dialogue conditions can also be set in code with `DialogueConditionManager.SharedInstance.SetConditionState("CONDITION_NAME", true/false)` or read with `DialogueConditionManager.SharedInstance.GetConditionState("CONDITION_NAME")`. 

Note that `CONDITION_NAME` is a placeholder that you would replace with whatever you want to call your condition. Consider appending conditions with the name of your mod to make for better compatibility between mods, for example a condition name like `SPOKEN_TO` is very generic and might conflict with other mods whereas `NH_EXAMPLES_SPOKEN_TO_ERNESTO` is much less likely to conflict with another mod.

### Persistent Condition

A persistent condition is similar to a condition, except it _persists_ through loops, and is saved on the players save file.

Persistent conditions shared many similar traits with regular dialogue conditions. You can use `<SetPersistentCondition>`, `<DisablePersistentCondition>`. On dialogue options you can use `<RequiredPersistentCondition>`, `<CancelledPersistentCondition>`

Persistent conditions can also be set in code with `PlayerData.SetPersistentCondition("PERSISTENT_CONDITION_NAME", true/false)` and read using `PlayerData.GetPersistentCondition("PERSISTENT_CONDITION_NAME")`.

### Ship Logs

Dialogue can interact with ship logs, either granting them to the player (`<RevealFacts>` on a DialogueNode) or locking dialogue behind ship log completion (`<RequiredLogCondition>` on a DialogueOption). 

You can also use `<DialogueTargetShipLogCondition>` in tandem with `DialogueTarget` to make it so you must have a [ship log fact](/guides/ship-log#explore-facts) to go to the next node.

## Adding to existing dialogue

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

Note: If you're loading dialogue in code, 2 frames must pass before entering the conversation in order for ReuseDialogueOptionsListFrom to take effect.


## Dialogue FAQ

### How do I easily position my dialogue relative to a speaking character

Use `pathToAnimController` to specify the path to the speaking character (if they are a Nomai or Hearthian make sure this goes directly to whatever script controls their animations), then set `isRelativeToParent` to true (this is setting available on all NH props for easier positioning). Now when you set their `position`, it will be relative to the speaker. Since this position is normally where the character is standing, set the `y` position to match how tall the character is. Instead of `pathToAnimController` you can also use `parentPath`.

### How do I have the dialogue prompt say "Read" or "Play recording"

`<NameField>` sets the name of the character, which will then show in the prompt to start dialogue. You can alternatively use `<NameField>SIGN</NameField>` to have the prompt say "Read", and `<NameField>RECORDING</NameField>` to have it say "Play recording".