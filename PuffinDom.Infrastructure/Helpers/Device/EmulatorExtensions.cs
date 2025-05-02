using System;

namespace PuffinDom.Infrastructure.Helpers.Device;

public static class EmulatorExtensions
{
    
    public static string ToLocalizedName(this Emulator emulator)
    {
        return emulator switch
        {
            Emulator.Android35 => "Regular Phone API 35",
            Emulator.Android33 => "Regular Phone API 33",
            Emulator.AndroidTablet21 => "Old Tablet API 21",
            Emulator.iOSLatest => "Regular Phone Latest iOS",
            _ => throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null),
        };
    }

    public static bool IsRunDroid(this Emulator emulator) => emulator.ToString().Contains("Android");

    public static bool IsRunIOS(this Emulator emulator) => emulator.ToString().Contains("iOS");
}