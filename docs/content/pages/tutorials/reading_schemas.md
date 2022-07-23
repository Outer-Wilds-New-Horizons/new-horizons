---
Title: Reading Schemas
Sort_Priority: 90
---

# Reading Schema Pages

Reading and understanding the schema pages are key to knowing how to create planets.  While these tutorials may be helpful, they won't cover everything, and new features may be added before the tutorial on them can be written.

## Celestial Body Schema

The [celestial body schema]({{ "Celestial Body Schema"|route }}) is the schema for making planets, there are other schemas which will be explained later but for now let's focus on this one.

![The Celestial Body Schema Page]({{ "images/reading_schemas/body_schema_1.webp"|static }})

As you can see the type of this is `object`, which we talked about in the previous section.
We can also observe a blue badge that says "No Additional Properties", this signifies that you can't add keys to the object that aren't in the schema, for example:

```json
{
    "name": "Wetrock",
    "coolKey": "Look at my cool key!"
}
```

Will result in a warning in VSCode. Now, this will *not* prevent the planet from being loaded, however you should still avoid doing it.

## Simple Properties

![The name property on the celestial body schema]({{ "images/reading_schemas/body_schema_2.webp"|static }})

Next up let's look at `name`, this field is required, meaning you *have* to have it for a planet to load.
When we click on name we first see a breadcrumb, this is essentially a guide of where you are in the schema, right now we're in the name property of the root (topmost) object.
We can also see it's description, its type is `string`, and that it requires at least one character (so you can't just put `""`).

Badges can also show stuff such as the default value, the minimum and maximum values, and more.

## Object Properties

![The Base object on the celestial body schema]({{ "images/reading_schemas/body_schema_3.webp"|static }})

Next let's look at an `object` within our root `object`, let's use `Base` as the example.

Here we can see it's similar to our root object, in that it doesn't allow additional properties.
We can also see all of its properties listed out.

## Array Properties

Now let's take a look over at [removeChildren]({{ "Celestial Body Schema"|route }}#removeChildren) to see how arrays work (if you're wondering how you can get the page to scroll to a specific property, simply click on the property and copy the URL in your URL bar)

![The curve property on a star in the celestial body schema]({{ "images/reading_schemas/body_schema_4.webp"|static }})

Here we can see that the type is an `array`, and each item in this array must be a `string`

## Some Vocabulary

- GameObject: Essentially just any object in, well, the game. You can view these object in a tree-like structure with the [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) mod. Every GameObject has a path, which is sort of like a file path in that it's a list of parent GameObjects seperated by forward slashes followed by the GameObject's name.
- Component: By themselves, a GameObject doesn't actually *do* anything, components provide stuff like collision, rendering, and logistics to GameObjects
- Config: Just another name for a JSON file "planet config" simply means a json file that describes a planet
- Module: A specific section of the config (e.g. Base, Atmosphere, etc), these usually start with capital letters

## Note About File Paths

Whenever a description refers to the "relative path" of a file, it means relative to the mod's directory, this means you **must** include the `planets` folder in the path:

```json
"planets/assets/images/MyCoolImage.png"
```

## Other Schemas

There are other schemas available, some are for JSON, and some are for XML.

## Moving Forward

Now that you know how to read the schema pages, you can understand the rest of this site.  A lot of the other tutorials here will often tell you to take a look at schemas to explain what certain properties do.

**Next Up: [Planet Generation]({{ "Planet Generation"|route }})**
