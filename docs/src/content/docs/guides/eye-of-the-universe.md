---
title: Eye of the Universe
description: A guide to adding additional content to the Eye of the Universe with New Horizons
---

This guide covers some 'gotchas' and features unique to the Eye of the Universe.

## Extending the Eye of the Universe

### Star System

To define a Star System config for the Eye of the Universe, name your star system config file `EyeOfTheUniverse.json` or specify the `"name"` as `"EyeOfTheUniverse"`. Note that many of the star system features have no effect at the Eye compared to a regular custom star system.

```json
{
  "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/star_system_schema.json",
  "name": "EyeOfTheUniverse",
  // etc.
}
```

### Existing Planet

The existing areas of the Eye of the Universe, such as the "sixth planet" and funnel, the museum/observatory, the forest of galaxies, and the campfire, are all contained within one static "planet" (despite visually being distinct locations). To add to these areas, you'll need to specify a planet config file with a `"name"` of `"EyeOfTheUniverse"` and *also* a `"starSystem"` of `"EyeOfTheUniverse"`, as the star system and "planet" share the same name.

```json
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name": "EyeOfTheUniverse",
    "starSystem": "EyeOfTheUniverse",
    "Props": {
        "details": [
            // etc.
        ]
    },
    // etc.
}
```

### The Vessel

You can also add props to the Vessel at the Eye:

```json
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name": "Vessel",
    "starSystem": "EyeOfTheUniverse",
    "Props": {
        "details": [
            // etc.
        ]
    },
    // etc.
}
```

## Eye-Specific Features

### Eye Travelers

Eye Travelers are a special kind of detail prop (see [Detailing](/guides/details/)) that get added in as additional characters around the campfire at the end of the Eye sequence, similar to Solanum and the Prisoner in the vanilla game.

At minimum, you will need a character object to act as the traveler, an audio file for the looping part of the new instrument track, an audio file to layer over the finale, a dialogue XML file with certain dialogue conditions set up, and a [quantum instrument](#quantum-instruments).

The traveler will only appear once their quantum instrument is gathered. After that, they will appear in the circle around the campfire, and they can be interacted with through dialogue to start playing their instrument. The instrument audio is handled via a signal on the traveler that only becomes audible after talking to them.

Custom travelers will automatically be included in the inflation animation that pushes everyone away from the campfire at the end of the sequence.

[Eye Travelers](#eye-travelers), [Quantum Instruments](#quantum-instruments), and [Instrument Zones](#instrument-zones) are all linked by their `"id"` properties. Ensure that your ID matches between those details and is unique enough to not conflict with other mods.

Here's an example config:
```json
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name": "EyeOfTheUniverse",
    "starSystem": "EyeOfTheUniverse",
    "EyeOfTheUniverse": {
        "eyeTravelers": [
            {
                "id": "Slate",
                "signal": {
                    "name": "Slate",
                    "audio": "planets/MetronomeLoop.wav",
                    "detectionRadius": 0,
                    "identificationRadius": 10,
                    "onlyAudibleToScope": false,
                    "position": {"x": 0, "y": 0.75, "z": 0},
                    "isRelativeToParent": true
                },
                "finaleAudio": "planets/MetronomeFinale.wav",
                "startPlayingCondition": "EyeSlatePlaying",
                "participatingCondition": "EyeSlateParticipating",
                "dialogue": {
                    "xmlFile": "planets/EyeSlate.xml",
                    "position": {"x": 0.0, "y": 1.5, "z": 0.0},
                    "isRelativeToParent": true
                },
                "path": "TimberHearth_Body/Sector_TH/Sector_Village/Sector_StartingCamp/Characters_StartingCamp/Villager_HEA_Slate"
            }
        ]
    }
}
```

To see the full list of eye traveler properties and descriptions of what each property does, check [the EyeTravelerInfo schema](/schemas/body-schema/defs/eyetravelerinfo/).

On compatibility:
> New Horizons changes the campfire sequence in minor ways to make it easier for mods which add additional travelers to be compatible with each other. The audio handling is changed so that instrument audio will be synchronized and layered with each other automatically. All travelers will be automatically repositioned in a circle around the campfire based on the total number of travelers, both vanilla and modded. As long as other details (such as quantum instruments and instrument zones) are not placed in the same locations as other existing mods, the campfire sequence changes will be compatible.

#### Audio

The looping audio clip is used for the signals emitted by the traveler and their quantum instrument, and is the audio that gets played when they play their instrument. It should be a WAV file with 16 measures of music at 92 BPM (exactly 2,003,478 samples at 48,000 Hz, or approximately 42 seconds long). It is highly recommended that you use Audacity or another audio editor to trim your audio clip to exactly the same length as one of the vanilla traveler audio clips, or else it will fall gradually out of sync with the other instrument loops.

The finale audio clip is only played once, after all travelers have started playing. It should be 8 measures of the main loop at 92 BPM followed by 2 measures of fade-out (approximately 26 seconds long in total). Unlike the looping audio clip, it does not need to precisely match the length of the vanilla finale clip; it can end early or continue playing after the other ends.

The game plays all of the looping audio clips (including your custom one) simultaneously once you tell the first traveler to start playing, and then fades them in one by one as you talk to the others. After all travelers are playing, the game selects a finale audio clip that contains all Hearthian and Nomai/Owlk instruments mixed into one file, and then your custom finale audio clip will be layered over whichever vanilla clip plays. Only include your own instrument in the clip, and ensure it sounds okay alongside Solanum, the Prisoner, and both/neither.

#### Dialogue

The dialogue XML for your traveler works like other dialogue (see the [Dialogue](/guides/dialogue/) guide) but there are specially named conditions you will need to use for the functionality to work as expected:
- Use `<EntryCondition>AnyTravelersGathered</EntryCondition>` to check if any traveler has been gathered yet. This includes Riebeck and Esker, so it should always be true, unless you forcibly enable your traveler to be enabled early.
- Use `<EntryCondition>AllTravelersGathered</EntryCondition>` to check if all of the travelers have been gathered and are ready to start playing.
- Use `<EntryCondition>JamSessionIsOver</EntryCondition>` to check if the travelers have stopped playing the song and the sphere of possibilities has appeared.
- Use a `<SetCondition></SetCondition>` with the condition defined in your eye traveler config's `"startPlayingCondition"` on the node or dialogue option that should make your traveler start playing their instrument. This condition name must be unique and not conflict with other mods.
- If you want your traveler to be present but have an option to not participate in the campfire song (like the Prisoner), use a `<SetCondition></SetCondition>` with the condition defined in your eye traveler config's `"participatingCondition"` on the node or dialogue option where your traveler agrees to join in. This condition name must be unique and not conflict with other mods.

```xml
<DialogueTree xsi:noNamespaceSchemaLocation="https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/dialogue_schema.xsd" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <NameField>Slate, Probably</NameField>
  <DialogueNode>
    <Name>WAITING_FOR_OTHERS</Name>
    <EntryCondition>DEFAULT</EntryCondition>
    <Dialogue>
      <Page>It's me, definitely Slate and not a dreamstalker in disguise.</Page>
    </Dialogue>
  </DialogueNode>
  <DialogueNode>
    <Name>ANY_GATHERED</Name>
    <EntryCondition>AnyTravelersGathered</EntryCondition>
    <Dialogue>
      <Page>You still have other travelers to gather.</Page>
    </Dialogue>
    <DialogueOptionsList>
        <DialogueOption>
            <Text>You're going to join in, right?</Text>
            <DialogueTarget>PARTICIPATING</DialogueTarget>
        </DialogueOption>
        <DialogueOption>
            <Text>Okay then...</Text>
        </DialogueOption>
    </DialogueOptionsList>
  </DialogueNode>
  <DialogueNode>
    <Name>PARTICIPATING</Name>
    <Dialogue>
        <Page>Sure.</Page>
    </Dialogue>
    <SetCondition>EyeSlateParticipating</SetCondition>
  </DialogueNode>
  <DialogueNode>
    <Name>READY_TO_PLAY</Name>
    <EntryCondition>AllTravelersGathered</EntryCondition>
    <Dialogue>
      <Page>We're all here. Time to start the music.</Page>
    </Dialogue>
    <DialogueOptionsList>
      <DialogueOption>
        <Text>Ready to go.</Text>
        <DialogueTarget>START_PLAYING</DialogueTarget>
      </DialogueOption>
      <DialogueOption>
        <Text>Not yet.</Text>
        <DialogueTarget>NOT_YET</DialogueTarget>
      </DialogueOption>
    </DialogueOptionsList>
  </DialogueNode>
  <DialogueNode>
    <Name>START_PLAYING</Name>
    <Dialogue>
        <Page>Let's begin.</Page>
    </Dialogue>
    <SetCondition>EyeSlatePlaying</SetCondition>
  </DialogueNode>
  <DialogueNode>
    <Name>NOT_YET</Name>
    <Dialogue>
      <Page>Whenever you're ready.</Page>
    </Dialogue>
  </DialogueNode>
  <DialogueNode>
    <Name>FAREWELL</Name>
    <EntryCondition>JamSessionIsOver</EntryCondition>
    <Dialogue>
      <Page>It's rewind time.</Page>
    </Dialogue>
  </DialogueNode>
</DialogueTree>
```

#### Custom Animation

To add custom animations to your Eye Traveler, there is some setup work that has to be done in Unity. You will need to set up your character in Unity and load them via asset bundle, like you would any other detail:

```json
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name": "EyeOfTheUniverse",
    "starSystem": "EyeOfTheUniverse",
    "EyeOfTheUniverse": {
        "eyeTravelers": [
            {
                "id": "MyCoolEyeGuy",
                "assetBundle": "planets/bundles/eyeoftheuniverse",
                "path": "Assets/EyeOfTheUniverse/Traveler/MyCoolEyeGuy.prefab"
            }
        ]
    }
}
```

Next, create an Animator Controller asset in Unity with at least two states, named "Idle" and "PlayingInstrument". You can assign whatever animation clip you like to these states, but the names of the states must match exactly. The default Idle state will play when the traveler is first spawned in, and will transition to the PlayingInstrument state when the right conditions are met to start playing the instrument. Ensure that both animation clips are set to loop in their import settings.

Add a boolean Parameter in the left panel named "Playing". This will be set to true when the traveler starts playing their instrument.

Add a transition in both directions between the Idle and PlayingInstrument states. Uncheck "Has Exit Time" for both transitions and adjust the other timing settings as desired.

Add a Condition on the `Idle -> PlayingInstrument` transition to check for `Playing` = `true`, and the inverse for `PlayingInstrument -> Idle`.

![animController](@/assets/docs-images/eye_of_the_universe/animController.webp)

In your character object, find the `Animator` component and set its `Controller` property to the Animator Controller asset you created. If you have a `TravelerEyeController` component in your object, set its `_animator` property to your Animator component.

If everything was set up correctly, your character should play their animations in-game.

### Quantum Instruments

Quantum instruments are the interactible instruments, typically hidden by a short 'puzzle', that cause their corresponding traveler to appear around the campfire. They are just like any other detail prop (see [Detailing](/guides/details/)) but they have additional handling to only activate after gathering and speaking to Riebeck, like the other instrument 'puzzles' in the Eye sequence.

If not specified, the quantum instrument will inherit some of the properties for its `"signal"` from the corresponding eye traveler config.

If you want other objects besides the traveler to appear or disappear in response to gathering the instrument, specify a custom dialogue condition name for `"gatherCondition"` and use that same condition as the `"activationCondition"` or `"deactivationCondition"` for the details you want to toggle.

Quantum instruments will automatically be included in the inflation animation that pushes everyone away from the campfire at the end of the sequence.

[Eye Travelers](#eye-travelers), [Quantum Instruments](#quantum-instruments), and [Instrument Zones](#instrument-zones) are all linked by their `"id"` properties. Ensure that your ID matches between those details and is unique enough to not conflict with other mods.

```json
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name": "EyeOfTheUniverse",
    "starSystem": "EyeOfTheUniverse",
    "EyeOfTheUniverse": {
        "eyeTravelers": [
            {
                "id": "Slate",
                "signal": {
                    "name": "Slate",
                    "audio": "planets/MetronomeLoop.wav"
                    // etc.
                },
                // etc.
            }
        ],
        "quantumInstruments": [
            {
                "id": "Slate",
                "gatherWithScope": false,
                "gatherCondition": "EyeSlateGather",
                "path": "TimberHearth_Body/Sector_TH/Sector_Village/Sector_StartingCamp/Props_StartingCamp/OtherComponentsGroup/Props_HEA_CampsiteLogAssets/Props_HEA_MarshmallowCanOpened",
                "position": {"x": -43.94369, "y": 0, "z": 7506.436},
                "signal": {
                    "detectionRadius": 0,
                    "identificationRadius": 10,
                    "position": {"x": 0, "y": 0.1, "z": 0},
                    "isRelativeToParent": true
                }
            }
        ]
    }
}
```

To see the full list of quantum instrument properties and descriptions of what each property does, check [the QuantumInstrumentInfo schema](/schemas/body-schema/defs/quantuminstrumentinfo/).

### Instrument Zones

Instrument zones are just like any other detail prop (see [Detailing](/guides/details/)) but they have additional handling to only activate after gathering and speaking to Riebeck, like the other instrument 'puzzles' in the Eye sequence.

Custom instrument zones will automatically be included in the inflation animation that pushes everyone away from the campfire at the end of the sequence.

[Eye Travelers](#eye-travelers), [Quantum Instruments](#quantum-instruments), and [Instrument Zones](#instrument-zones) are all linked by their `"id"` properties. Ensure that your ID matches between those details and is unique enough to not conflict with other mods.

```json
{
    "$schema": "https://raw.githubusercontent.com/Outer-Wilds-New-Horizons/new-horizons/main/NewHorizons/Schemas/body_schema.json",
    "name": "EyeOfTheUniverse",
    "starSystem": "EyeOfTheUniverse",
    "EyeOfTheUniverse": {
        "eyeTravelers": [
            {
                "id": "Slate",
                // etc.
            }
        ],
        "instrumentZones": [
            {
                "id": "Slate",
                "deactivationCondition": "EyeSlateGather",
                "path": "TimberHearth_Body/Sector_TH/Sector_Village/Sector_StartingCamp/Props_StartingCamp/OtherComponentsGroup/Props_HEA_CampsiteLogAssets",
                "removeChildren": [
                    "Props_HEA_MarshmallowCanOpened"
                ],
                "position": {"x": -43.30302, "y": 0, "z": 7507.822}
            }
        ]
    }
}
```

To see the full list of instrument zone properties and descriptions of what each property does, check [the InstrumentZoneInfo schema](/schemas/body-schema/defs/instrumentzoneinfo/).

## Eye-Specific Considerations

### Cross-System Details

Specifying details with a `"path"` pointing to an object in the regular solar system normally wouldn't work, as the Eye of the Universe lives in a completely separate scene from the rest of the game and those objects don't exist at the Eye. New Horizons works around this by force-loading the regular solar system, grabbing any objects referenced in Eye of the Universe config files, and then attempting to preserve these objects when loading the Eye of the Universe scene. This can cause issues with many different kinds of props, especially interactive ones that depend on some other part of the solar system existing.

Because the objects are not available outside of this workaround, objects from the regular solar system cannot be spawned in via the New Horizons API.

### Custom Planets

While you *can* define completely custom planets the same as you would in a regular custom solar system, they may exhibit weird orbital behaviors or pass through the existing static Eye of the Universe objects. Prefer adding onto the existing bodies or setting a `"staticPosition"` on your planet configs to lock them in place.

### The Player Ship and Ship Logs

The player's ship does not exist in the Eye of the Universe scene. In addition to the obvious issues this causes (no access to the ship's warp functionality, ship spawn points being non-functional, etc.), the ship log computer not existing causes some methods of checking and learning ship log facts to not function at all while at the Eye. If you need to track whether the player has met certain conditions elsewhere in the game (for example, if they've previously met a character that you now want to appear at the campfire), consider using a Persistent Dialogue Condition, which does not have these issues.

### Mod Compatibility

Other existing and future story mods will want to add additional content to the Eye of the Universe, and unlike entirely custom planets and star systems, there is a high probability that objects placed at the Eye may overlap with those placed by other mods. When testing, try installing as many of these other mods as possible and seeing if the objects they add overlap with yours. If so, consider moving your objects to a different position. When possible, use New Horizons features that preserve compatibility between mods.