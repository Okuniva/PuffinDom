using System;
using System.Collections.Generic;
using System.IO;
using PuffinDom.Tools;
using PuffinDom.Infrastructure;

namespace PuffinDom.Infrastructure.Helpers.Device;

public class DroidEmulatorConfigurationHelper
{
    public const string EmulatorsNamesPostfix = "uitests";

    public static readonly Dictionary<Emulator, Tuple<string, string>> Emulators = new()
    {
        {
            Emulator.Android35,
            new Tuple<string, string>(
                $"35_{EmulatorsNamesPostfix}",
                GetSystemImageForDroidEmulator(AndroidSystemName.Android35))
        },
        {
            Emulator.Android33,
            new Tuple<string, string>(
                $"33_{EmulatorsNamesPostfix}",
                GetSystemImageForDroidEmulator(AndroidSystemName.Android33))
        },
        {
            Emulator.AndroidTablet21,
            new Tuple<string, string>(
                $"21_Tablet_{EmulatorsNamesPostfix}",
                GetSystemImageForDroidEmulator(AndroidSystemName.Android21))
        },
    };

    public static int SDCardSizeInMb => 128;

    public static string GetProcessorType()
    {
        if (RunningOSTools.IsMacOS)
            return "arm64-v8a";

        if (RunningOSTools.IsWindows && Environment.Is64BitOperatingSystem)
            return "x86_64";

        return "x86";
    }

    public static string GetProcessorTypeForCpuArchitecture()
    {
        if (RunningOSTools.IsMacOS)
            return "arm64";

        if (RunningOSTools.IsWindows && Environment.Is64BitOperatingSystem)
            return "x86_64";

        return "x86";
    }

    public static int Density(AndroidSystemName androidSystemName)
    {
        return androidSystemName switch
        {
            AndroidSystemName.Android33 or AndroidSystemName.Android35 => 144,
            AndroidSystemName.Android21 => 160,
            _ => throw new ArgumentOutOfRangeException(nameof(androidSystemName), androidSystemName, null),
        };
    }

    public static int Height(AndroidSystemName androidSystemName)
    {
        return androidSystemName switch
        {
            AndroidSystemName.Android33 or AndroidSystemName.Android35 => 720,
            AndroidSystemName.Android21 => 800,
            _ => throw new ArgumentOutOfRangeException(nameof(androidSystemName), androidSystemName, null),
        };
    }

    public static int Width(AndroidSystemName androidSystemName)
    {
        return androidSystemName switch
        {
            AndroidSystemName.Android33 or AndroidSystemName.Android35 => 352,
            AndroidSystemName.Android21 => 1280,
            _ => throw new ArgumentOutOfRangeException(nameof(androidSystemName), androidSystemName, null),
        };
    }

    public static string GetImageSystemDirectoryForEmulatorConfig(AndroidSystemName androidSystemVersion) =>
        GetSystemImage(androidSystemVersion, Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

    private static string GetSystemImageForDroidEmulator(AndroidSystemName androidSystemVersion) =>
        GetSystemImage(androidSystemVersion, ';');

    private static string GetSystemImage(AndroidSystemName androidSystemVersion, char separator) =>
        $"system-images{separator}" +
        $"{GetDroidSystemVersion(androidSystemVersion)}{separator}" +
        $"{GetDroidSystemImageGoogleServices(androidSystemVersion)}{separator}" +
        $"{GetProcessorType()}";

    private static string GetDroidSystemVersion(AndroidSystemName androidSystemName)
    {
        return androidSystemName switch
        {
            AndroidSystemName.Android33 => "android-33",
            AndroidSystemName.Android35 => "android-35",
            AndroidSystemName.Android21 => "android-21",
            _ => throw new ArgumentOutOfRangeException(nameof(androidSystemName), androidSystemName, null),
        };
    }

    private static string GetDroidSystemImageGoogleServices(AndroidSystemName androidSystemVersion)
    {
        var googleApis = "google_apis";

        googleApis += androidSystemVersion == AndroidSystemName.Android21 ? "" : "_playstore";

        return PuffinEnvironmentVariables.IsGoogleServicesEnabled ? googleApis : "default";
    }
}