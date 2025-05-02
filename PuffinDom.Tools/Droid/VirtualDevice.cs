using System;
using PuffinDom.Tools.Droid.Enums;

namespace PuffinDom.Tools.Droid;


public class VirtualDevice
{
    public VirtualDevice(string id, string name, string package, VirtualDeviceType type, int apiLevel, string? avdPath)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Package = package ?? throw new ArgumentNullException(nameof(package));
        Type = type;
        ApiLevel = apiLevel;
        AvdPath = avdPath;
    }

    public string Id { get; }

    public string Name { get; }

    public string Package { get; }

    public VirtualDeviceType Type { get; }

    public int ApiLevel { get; }

    public string? AvdPath { get; }

    public Version Version =>
        ApiLevel switch
        {
            19 => new Version(4, 4),
            20 => new Version(4, 4), // 4.4W (wear)
            21 => new Version(5, 0),
            22 => new Version(5, 1),
            23 => new Version(6, 0),
            24 => new Version(7, 0),
            25 => new Version(7, 1),
            26 => new Version(8, 0),
            27 => new Version(8, 1),
            28 => new Version(9, 0),
            29 => new Version(10, 0),
            30 => new Version(11, 0),
            31 => new Version(12, 0),
            32 => new Version(12, 1),
            33 => new Version(13, 0),
            _ => new Version(),
        };

    public override string ToString() =>
        $"{Name} (API {ApiLevel})";
}