using NewHorizons.Utility;
using NewHorizons.Utility.OWML;
using System.Linq;

namespace NewHorizons.Handlers;

public static class EyeDetailCacher
{
    public static void Init()
    {
        foreach (var body in Main.BodyDict["EyeOfTheUniverse"])
        {
            NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: {body.Config.name}");
            if (body.Config?.Props?.details == null) continue;

            foreach (var detail in body.Config.Props.details)
            {
                NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: {detail.path}");


                if (string.IsNullOrEmpty(detail.path)) continue;

                var planet = detail.path.Contains('/') ? detail.path.Split('/').First() : string.Empty;

                // TODO: what other root paths can we ignore
                if (planet != "EyeOfTheUniverse_Body" && planet != "Vessel_Body")
                {
                    NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: Looking for {detail.path}");
                    var obj = SearchUtilities.Find(detail.path);
                    if (obj != null)
                    {
                        NHLogger.LogVerbose($"{nameof(EyeDetailCacher)}: Added solar system asset to dont destroy on load cache for eye: {detail.path}");
                        SearchUtilities.AddToDontDestroyOnLoadCache(detail.path, obj);
                    }
                }
            }
        }
    }
}
