---
Title: Understanding XML
Sort_Priority: 50
---

# Understanding XML

XML is the other language New Horizons uses for content.  
XML files are usually passed straight to the game's code instead of going through New Horizons.

## Syntax

XML is comprised of tags, a tag can represent a section or attribute

```xml
<Person>
	<Name>Jim</Name>
	<Age>32</Age>
	<IsMarried/>
</Person>
```

Notice how each tag is closed by an identical tag with a slash at the front (i.e `<Person>` is closed by `</Person>`).  

If the tag has no content you can use the self-closing tag shorthand (i.e. `<IsMarried/>` doesn't need a closing tag because of the `/` at the end).

This XML could be written in JSON as:

```json
{
	"name": "Jim",
	"age": 32,
	"isMarried": true
}
```

XML is a lot more descriptive, you can actually tell that the object is supposed to be a person by the name of the tag.

## Structure

All XML files must have **one** top-level tag, this varies depending on what you're using it for (like how ship logs use a `<AstroObjectEntry>` tag).  

## Schemas 

XML files can also have schemas, you specify them by adding attributes to the top-level tag:

```xml
<Person xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="Link goes here">
</Person>
```

In order to get schema validation and auto-fill you'll need the [Redhat XML VSCode extension](https://marketplace.visualstudio.com/items?itemName=redhat.vscode-xml){ target="_blank" }.

## Uses

XML is used for the following:

- [Shiplog Entries]({{ "Ship Log"|route }})
- [Dialogue](#)
- [Translatable Text](#)



