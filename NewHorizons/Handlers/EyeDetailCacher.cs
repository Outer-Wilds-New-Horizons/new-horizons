using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Linq;

namespace NewHorizons.Handlers;

public static class EyeDetailCacher
{
    public static bool IsInitialized;

    public static void Init()
    {
        if (IsInitialized) return;

        SearchUtilities.ClearDontDestroyOnLoadCache();

        IsInitialized = true;

        foreach (var body in Main.BodyDict["EyeOfTheUniverse"])
        {
            NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: {body.Config.name}");
            if (body.Config?.EyeOfTheUniverse?.eyeTravelers != null)
            {
                foreach (var detail in body.Config.EyeOfTheUniverse.eyeTravelers)
                {
                    if (!string.IsNullOrEmpty(detail.assetBundle)) continue;

                    AddPathToCache(detail.path);
                }
            }

            if (body.Config?.EyeOfTheUniverse?.instrumentZones != null)
            {
                foreach (var detail in body.Config.EyeOfTheUniverse.instrumentZones)
                {
                    if (!string.IsNullOrEmpty(detail.assetBundle)) continue;

                    AddPathToCache(detail.path);
                }
            }

            if (body.Config?.EyeOfTheUniverse?.quantumInstruments != null)
            {
                foreach (var detail in body.Config.EyeOfTheUniverse.quantumInstruments)
                {
                    if (!string.IsNullOrEmpty(detail.assetBundle)) continue;

                    AddPathToCache(detail.path);
                }
            }

            if (body.Config?.Props?.details != null)
            {
                foreach (var detail in body.Config.Props.details)
                {
                    if (!string.IsNullOrEmpty(detail.assetBundle)) continue;

                    AddPathToCache(detail.path);
                }
            }

            if (body.Config?.Props?.scatter != null)
            {
                foreach (var scatter in body.Config.Props.scatter)
                {
                    if (!string.IsNullOrEmpty(scatter.assetBundle)) continue;

                    AddPathToCache(scatter.path);
                }
            }
        }
    }

    private static void AddPathToCache(string path)
    {
        NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: {path}");

        if (string.IsNullOrEmpty(path)) return;

        var planet = path.Contains('/') ? path.Split('/').First() : string.Empty;

        if (planet != "EyeOfTheUniverse_Body" && planet != "Vessel_Body")
        {
            NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: Looking for {path}");
            var obj = SearchUtilities.Find(path);
            if (obj != null)
            {
                NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: Added solar system asset to dont destroy on load cache for eye: {path}");
                SearchUtilities.AddToDontDestroyOnLoadCache(path, obj);
            }
        }
    }
}
