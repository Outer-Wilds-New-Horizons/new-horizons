Title: Detailing
Sort-Priority: 90

## Details/Scatterer

For physical objects there are currently two ways of setting them up: specify an asset bundle and path to load a custom asset you created, or specify the path to the item you want to copy from the game in the scene hierarchy. Use the [Unity Explorer](https://outerwildsmods.com/mods/unityexplorer) mod to find an object you want to copy onto your new body. Some objects work better than others for this. Good luck. Some pointers:
- Use "Object Explorer" to search
- Do not use the search functionality on Scene Explorer, it is really really slow. Use the "Object Search" tab instead.
- Generally you can find planets by writing their name with no spaces/punctuation followed by "_Body".

## Asset Bundles

Here is a template project: [Outer Wilds Unity Template](https://github.com/xen-42/outer-wilds-unity-template)

The template project contains ripped versions of all the game scripts, meaning you can put things like DirectionalForceVolumes in your Unity project to have artificial gravity volumes loaded right into the game.

If for whatever reason you want to set up a Unity project manually instead of using the template, follow these instructions:

1. Start up a Unity 2017 project (I use Unity 2017.4.40f1 (64-bit), so if you use something else I can't guarantee it will work). The DLC updated Outer Wilds to 2019.4.27 so that probably works but I personally haven't tried it.
2. In the "Assets" folder in Unity, create a new folder called "Editor". In it create a file called "CreateAssetBundle.cs" with the following code in it:

```cs
using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
```

3. Create your object in the Unity scene and save it as a prefab.
4. Add all files used (models, prefabs, textures, materials, etc) to an asset bundle by selecting them and using the drop down in the bottom right. Here I am adding a rover model to my "rss" asset bundle for the Real Solar System add-on.
![setting asset bundle]({{ 'images/detailing/asset_bundle.png'|static }})

5. In the top left click the "Assets" drop-down and select "Build AssetBundles". This should create your asset bundle in a folder in the root directory called "StreamingAssets".
6. Copy the asset bundle and asset bundle .manifest files from StreamingAssets into your mod's "planets" folder. If you did everything properly they should work in game. To double check everything is included, open the .manifest file in a text editor to see the files included and their paths.
