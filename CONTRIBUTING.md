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

## Update the Schema!

If you make any changes that edit basically anything in the [External](NewHorizons/External) folder, please update the relevant schemas.

## Contributing to Documentation

If you wish to contribute to the documentation, take a look at [Setup.md](docs/Setup.md) in the docs folder.
