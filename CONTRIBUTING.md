# Contributing

Thank you for choosing to contribute to New Horizons!

## Getting Started

To get started, [fork this repository](https://github.com/xen-42/outer-wilds-new-horizons/fork).  
Once you have a fork created, create a new branch off of `dev` where you will make your changes.  
Then, clone your fork and checkout your new branch.

## Building

To build a development release of New Horizons, use the `Debug` build target.  
This will automatically build to your mods directory in OWML (so long as it's in `%APPDATA%/OuterWildsModManager/OWML`).

### Getting Line Numbers

To save yourself the pain of decoding where in a function an error occured, you can [download this dll file](https://cdn.discordapp.com/attachments/929787137895854100/936860223983976448/mono-2.0-bdwgc.dll) and place it in `MonoBleedingEdge/EmbedRuntime` of the game's folder.  
Then (so long as you build targeting `Debug`), line numbers will be shown in any error that comes from New Horizons

## Provide examples

When adding a new feature, include a complete set of planet config files that will sufficiently demonstrate the functionality of the feature/bug fix/improvement. This way reviewers can just copy paste these files into the New Horizons planets folder.

## Updating The Schema

When you add fields to config classes, please document them using XML documentation so that our action can generate a proper schema.

```cs
/// <summary>
/// This is my new field!
/// </summary>
public string myField;
```

You can also use `Range` (from `System.ComponentModel.DataAnnotations` NOT Unity), and `DefaultValue`:

```cs
/// <summary>
/// This is my new field!
/// </summary>
[Range(0, 100)]
[DefaultValue(50)]
public int myField;
```

### Enums

You can also setup enums in the config classes:

```cs
[JsonConverter(typeof(StringEnumConverter))]
public enum MyCoolEnum {
    [EnumMember(Value = @"value1")]
    Value1 = 0,
    [EnumMember(Value = @"value2")]
    Value2 = 1,
}

/// <summary>
/// My enum field
/// </summary>
public MyCoolEnum enumField;
```

These will automatically be converted from strings to the proper enum type.

## Contributing to Documentation

If you wish to contribute to the documentation, take a look at [CONTRIBUTING.md](docs/CONTRIBUTING.md) in the docs folder.

## Disclaimer

This should go without saying, but we will not accept PRs that are obviously AI generated, nor will we accept PRs from people who have not actually played the game or any mods.

Any potential bug bounties for New Horizons are only eligible to be claimed by those who have created mods for Outer Wilds in the past.
