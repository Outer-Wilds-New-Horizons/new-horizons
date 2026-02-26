---
title: Adding New Horizons to an Existing Outer Wilds Mod
description: A guide on how to integrate New Horizons into a mod that did not already have it
---

If you have an existing mod that was made without New Horizons, adding New Horizons compatibility is very easy.

1. Download [INewHorizons.cs](/NewHorizons/INewHorizons.cs) and save it into your mod project somewhere.
2. Open your main mod file (ModName.cs) and add `public INewHorizons NewHorizons;` to your field definitions.
3. Add the following code to your `Start()` method:

```cs
// Get the New Horizons API and load configs
NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
NewHorizons.LoadConfigs(this);
```

This will make New Horizons automatically load any planets and systems you have set up, though if you're making a more complicated mod you may prefer to load config files manually.
