---
title: Creating An Addon
description: A guide to creating addons for New Horizons
---

Up until now, you've been using the sandbox feature of New Horizons (simply placing your files in the `xen.NewHorizons` folder).
While this is the easiest way to get started, you won't be able to publish your work like this. In this tutorial we will:

- Create a new GitHub repository from a template
- Use GitHub Desktop to clone this repository to our computer
- Edit the files in this repository to make our addon

## Making a GitHub Repository

To get started, we need a place to store our code. GitHub is one of the most popular websites to store source code, and it's also what the mod database uses to let people access our mod.
First you're going to want to [create a GitHub account](https://github.com/signup), and then head to [this repository](https://github.com/xen-42/ow-new-horizons-config-template).
Now, click the green "Use This Template" button.

- Set the Name to your username followed by a dot (`.`), followed by your mod's name in PascalCase (no spaces, new words have capital letters). So for example if my username was "Test" and my mod's name was "Really Cool Addon", I would name the repo `Test.ReallyCoolAddon`.
- The description is what will appear in the mod manager under the mod's name, you can always edit it later
- You can set the visibility to what you want; But when you go to publish your mod, it will need to be public

## Cloning the Repository

Now that we've created our GitHub repository (or "repo"), we need to clone (or download) it onto our computer.
To do this we recommend using the [GitHub Desktop App](https://desktop.github.com/), as it's much easier to use than having to fight with the command line.

Once we open GitHub desktop we're going to log in, select File -> Options -> Accounts and sign in to your newly created GitHub account.
Now we're ready to clone the repo, select File -> Clone Repository. Your repository should appear in the list.
Before you click "Clone", we need to select where to store the repo, open up the mod manager and go to "Settings", then copy the value located in the "OWML path" field and paste it in the "Local path" field on GitHub desktop.
This *will* show an error, and this is going to sound extremely stupid, but just click the "Choose..." button, and press "Select Folder" and it will be fixed.

Our repository is now cloned to our computer!

## Editing Files

Now that our repo is cloned, we're going to need to edit the files in it.
To get started editing the files, simply click "Open in Visual Studio Code" in GitHub Desktop.

### Files Explanation

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
- README.md: This file is displayed on the mod website when you go to a specific mod's page, you can delete the current contents.
  - This file is a [markdown](https://www.markdowntutorial.com/) file, if you're not comfortable writing an entire README right now, just write a small description of your mod.

### Committing The Changes

Now that we have our files set up, switch back to GitHub desktop, you'll notice that the files you've changed have appeared in a list on the left.
What GitHub Desktop does is keep track of changes you make to your files over time.
Then, once you're ready, you commit these changes to your repo by filling out the "Summary" field with a small description of your changes, and then pressing the blue button that says "commit to main".

Think of committing like taking a snapshot of your project at this moment in time. If you ever mess up your project, you can always revert to another commit to get back to a working version. It is highly recommended to commit often, there is no downside to committing too much.

### Pushing The Changes

OK, so we've committed our new changes, but these commits still only exist on our computer, to get these changes onto GitHub we can click the "Push Origin" button on the top right.

## Testing The Addon

Now that we have our manifest filled out, go take a look at the "Mods" tab in the manager and scroll to the bottom of the "Enabled Mods" list.

You should see your mod there with the downloads counter set as a dash and the version set to "0.0.0".

### Checking In-Game

Now when you click "Start Game" and load into the solar system, you should be able to notice that the quantum moon is gone entirely, this means that your addon and its configs were successfully loaded.

## Note About File Paths

Whenever something refers to the "relative path" of a file, it means relative to your mod's directory, this means you **must** include the `planets` folder in the path:

```json
"planets/assets/images/MyCoolImage.png"
```

## Going Forward

Now instead of using the New Horizons mod folder, you can use your own mod's folder instead.
