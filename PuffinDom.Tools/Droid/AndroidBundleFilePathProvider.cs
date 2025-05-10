using PuffinDom.Tools.Droid.Enums;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Tools.Droid;

public class AndroidBundleFilePathProvider
{
    public static List<string> GetFiles(string directory, AndroidBundleType androidBundleType = AndroidBundleType.Apk)
    {
        var pattern = androidBundleType switch
        {
            AndroidBundleType.Apk => "*.apk",
            AndroidBundleType.Aab => "*.aab",
            _ => throw new ArgumentOutOfRangeException(nameof(androidBundleType), androidBundleType, null),
        };

        return Directory.EnumerateFiles(directory, pattern).ToList();
    }

    private static string GetSingleMatchingFile(string directory, AndroidBundleType androidBundleType = AndroidBundleType.Apk)
    {
        var androidPackages = GetFiles(directory, androidBundleType);

        if (androidPackages.IsEmpty())
            throw new Exception($"NOT FOUND! No android {androidBundleType} was found in '{directory}'");

        if (androidPackages.Count > 1)
            throw new Exception($"More than one file android {androidBundleType} package were found in '{directory}'");

        return androidPackages.Single();
    }

    public static string Get(string pathToDirectory, AndroidBundleType androidBundleType = AndroidBundleType.Apk)
    {
        return Path.GetFullPath(
            GetSingleMatchingFile(
                pathToDirectory,
                androidBundleType));
    }
}