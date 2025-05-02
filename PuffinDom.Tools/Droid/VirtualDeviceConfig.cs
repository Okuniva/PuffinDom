using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PuffinDom.Tools.Droid.Enums;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Tools.Droid;

public class VirtualDeviceConfig
{
    private static readonly Regex _androidApiRegex = new(@"android-(\d+)");
    private readonly string? _avdPath;

    private readonly string _configPath;

    private Dictionary<string, string>? _properties;

    public VirtualDeviceConfig(string avdPath)
    {
        _avdPath = avdPath ?? throw new ArgumentNullException(nameof(avdPath));

        _configPath = Path.Combine(avdPath, "config.ini");
    }

    
    public IReadOnlyDictionary<string, string> GetProperties()
    {
        if (_properties != null)
            return _properties;

        Log.Write($"Loading config.ini {_configPath}...");

        var contents = File.ReadAllText(_configPath);
        _properties = ParseConfig(contents);

        return _properties;
    }

    
    public string? GetStringValue(string key)
    {
        var props = GetProperties();

        props.TryGetValue(key, out var value);

        return value;
    }

    public VirtualDevice CreateVirtualDevice()
    {
        var props = GetProperties();

        if (!props.TryGetValue("avdid", out var id))
            id = Path.GetFileNameWithoutExtension(_avdPath);

        if (string.IsNullOrEmpty(id))
            throw new Exception("Invalid config.ini. Unable to find the virtual device ID.");

        var name = props.GetValueOrDefault("avd.ini.displayname", id);

        var package = props.GetValueOrDefault("image.sysdir.1", "");

        var separators = new[] { '\\', '/', ';' };
        var packageParts = package.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        package = string.Join(";", packageParts);

        var apiLevel = 0;
        if (packageParts.Length == 4)
        {
            var apiMatch = _androidApiRegex.Match(packageParts[1]);
            if (apiMatch.Success)
                apiLevel = int.Parse(apiMatch.Groups[1].Value);
        }

        if (!TryGetType(props, out var type))
            type = VirtualDeviceType.Unknown;

        return new VirtualDevice(id, name, package, type, apiLevel, _avdPath);
    }

    private static bool TryGetType(IReadOnlyDictionary<string, string> props, out VirtualDeviceType value)
    {
        if (props.TryGetValue("tag.id", out var type))
            switch (type.Trim().ToLowerInvariant())
            {
                case "default":
                case "google_apis":
                case "google_apis_playstore":
                    value =
                        TryGetDimensions(props, out var width, out var height, out var density)
                        && Math.Min(width, height) / (density / 160) >= 600
                            ? VirtualDeviceType.Tablet
                            : VirtualDeviceType.Phone;

                    return true;
            }

        value = VirtualDeviceType.Unknown;
        return false;
    }

    private static bool TryGetDimensions(IReadOnlyDictionary<string, string> props, out int width, out int height, out double density)
    {
        width = 0;
        height = 0;
        density = 160;

        if (!props.TryGetValue("hw.lcd.width", out var widthString) || !int.TryParse(widthString, out width))
            return false;

        if (!props.TryGetValue("hw.lcd.height", out var heightString) || !int.TryParse(heightString, out height))
            return false;

        if (!props.TryGetValue("hw.lcd.density", out var densityString) || !double.TryParse(densityString, out density))
            density = 160;

        return true;
    }

    private static Dictionary<string, string> ParseConfig(string contents)
    {
        char[] separators = ['\r', '\n'];
        var lines = contents.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        var props = new Dictionary<string, string>();

        foreach (var line in lines)
        {
            var separator = new[] { '=' };
            var pair = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (pair.Length == 2)
                props[pair[0].ToLowerInvariant()] = pair[1];
        }

        return props;
    }
}