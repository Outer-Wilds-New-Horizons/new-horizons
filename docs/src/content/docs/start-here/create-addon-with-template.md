---
title: Creating an Addon Using the New Horizons Template
description: A guide for creating a New Horizons mod project from the template
---

> [!IMPORTANT]
> This guide is designed for making an addon project that will be using only New Horizons without custom scripts.
> If you want to have support for custom scripts, you should follow the [OWML Getting Started Guide](https://owml.outerwildsmods.com/guides/getting_started.html) and tick "Use New Horizons" when it shows up.

## Making a GitHub Repository

To get started, you'll need to click the green "Use This Template" button on [the New Horizons addon template](https://github.com/xen-42/ow-new-horizons-config-template) GitHub repository.

- Set the Name to your username followed by a dot (`.`), followed by your mod's name in PascalCase (no spaces, new words have capital letters). So for example if my username was "Test" and my mod's name was "Really Cool Addon", I would name the repo `Test.ReallyCoolAddon`.
- The description is what will appear in the mod manager under the mod's name, you can always edit this later
- You can set the visibility to what you want; But when you go to publish your mod, it will need to be public

## Open The Project

Now clone the repository to your local computer and open it in your favorite editor (we recommend [VSCode](https://code.visualstudio.com/)).

## Project Layout

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
