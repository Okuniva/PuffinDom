using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Tools.Droid;

public class AndroidAaptPathProvider
{
    private static readonly string _userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    private static readonly string _androidSdkBuildTools = Path.Combine(_userHome, "Library", "Android", "sdk", "build-tools");
    private static readonly Lazy<string[]> _aaptPaths = new(FindAaptPaths);

    public static string[] GetPaths() => _aaptPaths.Value;

    private static string[] FindAaptPaths()
    {
        if (!Directory.Exists(_androidSdkBuildTools))
            throw new Exception("Could not find Android SDK build tools");

        var paths = new List<string>();

        try
        {
            var buildToolVersions = GetBuildToolVersions();

            if (buildToolVersions.Count > 0)
                paths.AddRange(ValidateAaptInPaths(buildToolVersions));
        }
        catch (Exception ex)
        {
            Log.Write($"Error finding aapt paths: {ex.Message}");
        }

        paths.Add("aapt");
        return paths.ToArray();
    }

    private static List<string> GetBuildToolVersions()
    {
        var buildToolsDirectories = Directory.GetDirectories(_androidSdkBuildTools);
        var buildToolVersions = new List<string>();

        foreach (var dir in buildToolsDirectories)
        {
            var versionName = Path.GetFileName(dir);
            var isValidVersionFormat = Regex.IsMatch(versionName, @"^[\d.]+$");
            if (isValidVersionFormat)
                buildToolVersions.Add(versionName);
        }

        return buildToolVersions;
    }

    private static List<string> ValidateAaptInPaths(List<string> versions)
        => versions
            .Select(version => Path.Combine(_androidSdkBuildTools, version, "aapt"))
            .Where(File.Exists)
            .OrderByDescending(v => v, StringComparer.Ordinal)
            .ToList();
}