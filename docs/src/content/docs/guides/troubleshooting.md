---
title: Troubleshooting
description: A guide to troubleshooting common issues with mods
---

## My slide reels aren't updating when I change them

Certain images (such as slide reels) get modified by New Horizons before usage, to save on resources NH will cache
the modified version of these images on the file system to be recalled later for easier access. If you are changing
an image you'll need to clear the cache located in the `SlideReelsCache` folder of your mod's directory to see changes. To do this simply delete the folder and restart the game.

## My planet is flying away at light speed and also I have anglerfish

Be sure to disable `hasFluidDetector` (previous had to enable `invulnerableToSun`). The anglerfish have fluid volumes in their mouths for killing you
which interact poorly with the fluid detector and can mess up the movement of the planet.

## My Nomai text isn't updating

Either clear the .nhcache files or enable Debug mode to always regenerate the text cache.

## Prop placer is gone!
This is not a bug, actually. We removed prop placer because it was inconsistent and buggy, and no one in years cared enough to fix it.
Use the debug raycast button and Unity Explorer to place your props, or otherwise work in unity editor.
