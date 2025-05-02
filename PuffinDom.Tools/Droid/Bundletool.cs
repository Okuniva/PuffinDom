using System.IO;
using System.Text.RegularExpressions;
using PuffinDom.Tools.ExternalApplicationsTools;

namespace PuffinDom.Tools.Droid;

public class Bundletool
{
    private const string AabTargetPath = "./BundleToolApks/app.apks";

    private static string ConvertAabToApksArchive(string aabPath, string? outputPath = null, bool overwrite = true)
    {
        outputPath ??= AabTargetPath;

        if (Directory.Exists(outputPath) && !overwrite)
            return outputPath;

        ExternalProgramRunner.Run(
            "bundletool",
            $"build-apks --bundle={aabPath} --output={outputPath} --overwrite",
            message: "Extracting apks from aab bundle file");

        return outputPath;
    }

    public static (int Min, int Max) GetSize(string aabPath)
    {
        var outputFolder = ConvertAabToApksArchive(aabPath, overwrite: false);

        var output = ExternalProgramRunner.Run(
                "bundletool",
                $"get-size total --apks={outputFolder}",
                message: $"Getting PlayStore app size from aab bundle: {aabPath}")
            .Output.Replace("MIN,MAX", "")
            .Trim();

        var matches = Regex.Matches(output, "(.*),(.*)");
        var min = matches[0].Groups[1].Value;
        var max = matches[0].Groups[2].Value;
        return (int.Parse(min), int.Parse(max));
    }

    public static void InstallFromAab(string path)
    {
        var outputFolder = ConvertAabToApksArchive(path, overwrite: true);

        ExternalProgramRunner.Run(
            "bundletool",
            $"install-apks --apks={outputFolder}",
            message: $"Installing app from aab bundle: {path}");
    }
}