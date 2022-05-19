---
Title: Dialogue
Description: Guide to making dialogue in New Horizons
Sort_Priority: 50
---

# Dialogue

This page goes over how to use dialogue in New Horizons.

# Understanding Dialogue

## Dialogue Tree

A dialogue tree is an entire conversation, it's made up of dialogue nodes.

## Dialogue Node

A node is a set of pages shown to the player followed by options the player can choose from to change the flow of the conversation.

## Condition

A condition is a yes/no value stored **for this loop and this loop only**.  It can be used to show new dialogue options, stop someone from talking to you (looking at you Slate), and more.

## Persistent Condition

A persistent condition is similar to a condition, except it *persists* through loops, and is saved on the player's save file.

## Remote Trigger

A remote trigger is used to have an NPC talk to you from a distance; ex: Slate stopping you for the umpteenth time to tell you information you already knew.

# Example XML

Here's an example dialogue XML:

```xml
<!-- Example Dialogue -->
<!-- All files must have `DialogueTree` as the root element, the xmlns:xsi=... and xsi:noNamespaceSchemaLocation=... is optional but provides improved error checking if your editor supports it -->
<DialogueTree xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xsi:noNamespaceSchemaLocation="https://raw.githubusercontent.com/xen-42/outer-wilds-new-horizons/main/NewHorizons/Schemas/dialogue_schema.xsd">
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

# Using the XML

To use the dialogue XML you have created, you simply need to reference it in the `dialogue` prop

```json
{
 "Props": {
  "dialogue": [
   {
    "position": {"x": 5, "y": 10, "z": 0},
    "xmlFile": "planets/path/to/your_file.xml"
   }
  ]
 }
}
```

# Dialogue Config

To view the options for the dialogue prop, check [the schema]({{ "Celestial Body Schema"|route }}#Props_dialogue)

# Controlling Conditions

You can set condition in dialogue with the `<SetCondition>` and `<SetPersistentCondition>` tags

```xml
<DialogueNode>
 <!-- ... -->
 <SetCondition>EXAMPLE_CONDITION</SetCondition>
 <SetPersistentCondition>EXAMPLE_P_CONDITION</SetPersistentCondition>
 <!-- ... -->
</DialogueNode>
```

# Dialogue Options

There are many control structures for dialogue options to hide/reveal them if conditions are met. Take a look at [the DialogueOption schema]({{ "Dialogue Schema"|route }}#DialogueTree-DialogueNode-DialogueOptionsList-DialogueOption-DialogueTarget) for more info.

# Controlling Flow

In addition to `<DialogueOptions>`, there are other ways to control the flow of the conversation.

## DialogueTarget

Defining `<DialogueTarget>` in the `<DialogueNode>` tag instead of a `<DialogueOption>` will make the conversation go directly to that target after the character is done talking.

## DialogueTargetShipLogCondition

Used in tandum with `DialogueTarget`, makes it so you must have a [ship log fact]({{ "Ship Log"|route }}#explore-facts) to go to the next node.
