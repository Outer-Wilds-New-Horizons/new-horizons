---
title:Creating an Addon Project
description:A guide for creating a project with New Horizons support
---

If you want to create a New Horizons project, you have a few options, depending on your use case.

## Using the New Horizons Template

Use this method if you **are not  planning** to use any custom scripts in your mod. This is helpful if you'd prefer to avoid installing Visual Studio, but also limits what you can do in your mod.

### Making a GitHub Repository

To get started, you'll need to click the green "Use This Template" button on [the New Horizons addon template](https://github.com/xen-42/ow-new-horizons-config-template) GitHub repository.

- Set the Name to your username followed by a dot (`.`), followed by your mod's name in PascalCase (no spaces, new words have capital letters). So for example if my username was "Test" and my mod's name was "Really Cool Addon", I would name the repo `Test.ReallyCoolAddon`.
- The description is what will appear in the mod manager under the mod's name, you can always edit this later
- You can set the visibility to what you want; But when you go to publish your mod, it will need to be public

### Open The Project

Now clone the repository to your local computer and open it in your favorite editor (we recommend [VSCode](https://code.visualstudio.com/)).

### Project Layout

- .github: This folder contains special files for use on GitHub, they aren't useful right now but will be when we go to publish the mod
- planets: This folder contains a single example config file that destroys the Quantum Moon, we'll keep it for now so we can test our addon later.
- .gitattributes: This is another file that will be useful when publishing
- default-config.json: This file is used in C#-based mods to allow a custom options menu, New Horizons doesn't support a custom options menu, but we still need the file here in order for the addon to work.
- manifest.json: This is the first file we're going to edit, we need to fill it out with information about our mod
    - First you're going to set `author` to your author name, this should be the same name that you used when creating the GitHub repo.
    - Next, set `name` to the name you want to appear in the mod manager and website.
    - Now set `uniqueName` to the name of your GitHub Repo.
    - You can leave `version`, `owmlVersion`, and `dependencies` alone
- NewHorizonsConfig.dll: This is the heart of your addon, make sure to never move or rename it.

### Adding support for custom scripts later

If you later decide you want to use custom scripts, follow the guide below for using the standard OWML template. Then, copy all your `.json` and `.xml` files into your new project. You may wish to move all your existing files in your GitHub repository into a new branch, then force push your existing project into your repository, or alternatively make a brand new repository.

## Using the standard OWML Template

If you'd like the ability to use custom scripts, then you'll want to make a typical Outer Wilds mod and make sure to include the support for New Horizons. This will require Visual Studio and is a little more complicated, but will give you far more power over your mod.

Follow the [OWML Getting Started Guide](https://owml.outerwildsmods.com/guides/getting_started.html). However, when following the project creation wizard, make sure to tick the "Use New Horizons" checkbox when it shows up.

Next, create a `planets`, `systems`, `dialogue` and `translations` folder inside your mod project, in the **root** folder of your mod. You'll also want to make a few edits to your .csproj file in order to automatically copy those to your build. Open the .csproj file with a text editor or click the entry at the top of the Solution Explorer with the name of your mod. In the same `<ItemGroup>` that includes instructions to copy your manifest files, add these:

```xml
    <None Include="planets\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="systems\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="dialogue\**\*.*">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Include="translations\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
```

Now, whenever you run a build action, any files in these folders will be copied over, keeping whichever file is more recent in case of conflicts.

## Adding New Horizons to an existing mod

If you have an existing mod that was made without New Horizons, adding New Horizons compatibility is very easy.

First, create a `planets`, `systems`, `dialogue` and `translations` folder inside your mod project, in the **root** folder of your mod. You'll also want to make a few edits to your .csproj file in order to automatically copy those to your build. Open the .csproj file with a text editor or click the entry at the top of the Solution Explorer with the name of your mod. In the same `<ItemGroup>` that includes instructions to copy your manifest files, add these:

```xml
    <None Include="planets\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="systems\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Include="dialogue\**\*.*">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
    <None Include="translations\**\*.*">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
```

Then, you'll need to add some code for actually loading your planets and systems into your mod.

1. Download [INewHorizons.cs](/NewHorizons/INewHorizons.cs) and save it into your mod project somewhere.
2. Open your main mod file (ModName.cs) and add `public INewHorizons NewHorizons;` to your field definitions.
3. Add the following code to your `Start()` method:

```cs
// Get the New Horizons API and load configs
NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
NewHorizons.LoadConfigs(this);
```

This will make New Horizons automatically load any planets and systems you have set up, though if you're making a more complicated mod you may prefer to load config files manually. 
