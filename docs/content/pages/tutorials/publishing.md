---
Title: Publishing Addons
Sort_Priority: 1
---

# Publishing Your Addon

This page goes over how to publish a release for your mod and submit your mod to the [outer wilds mod database](https://github.com/ow-mods/ow-mod-db) for review.  

This guide assumes you've created your addon by following [the addon creation guide]({{ "Creating An Addon"|route }}).

## Housekeeping

Before you release anything, you'll want to make sure:

- Your mod has a descriptive `README.md`. (This will be shown on the website)
- Your repo has the description field (click the cog in the right column on the "Code" tab) set. (this will be shown in the manager)
- There's no `config.json` in your addon. (Not super important, but good practice)
- Your manifest has a valid name, author, and unique name. 
<!-- Can't think of anything else rn, might update later -->

## Releasing

First things first we're going to create a release on GitHub. To do this, first make sure all your changes are committed and pushed in GitHub desktop.  

Then, edit your `manifest.json` and set the version number to `0.1.0` (or any version number that's higher than `0.0.0`).  

Finally, push your changes to GitHub, head to the "Actions" tab of your repository and you should see an action running.  

Once the action finishes head back to the "Code" tab and you should see a Version 0.1.0 (or whatever version you put) in the column on the right.  

Double check the release, it should have a zip file in the assets with your mod's unique name.

## Submitting

The hard part is over now, all that's left is to submit your mod to the database.  

[Head to the mod database and make a new "Add/Update Existing Mod" issue](#) <!-- TODO: Paste link for issue here, vim is dumb -->

Fill this out with your mod's info and make sure to put `xen.NewHorizons` in the parent mod field.  

Once you're done filling out the form, an admin will review your mod, make sure it works, and approve it into the database.  

Congrats! You just published your addon!

## Updating

If you want to update your mod, you can simply bump the version number in `manifest.json` again.  

To edit the release notes displayed in discord, enter them in the "Description" field before you commit in GitHub desktop.  The most recent commit's description is used for the relase notes.  

**You don't need to create a new issue on the database to update your mod, it will be updated automatically after a few minutes**




