using System;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;

namespace PuffinDom.UI;

public class UIContext
{
    private static DeviceManager? _deviceManager;
    
    public static DeviceManager Device =>
        _deviceManager ??= new DeviceManagerContainerRegistration(CoreEnvironmentVariables.Device).Resolve().Value;

    private static Platform? _platform;

    public static Platform Platform
    {
        get => _platform ?? (CoreEnvironmentVariables.RunDroid ? Platform.Android : Platform.iOS);
        set => _platform = value;
    }

    public static bool Android => Platform == Platform.Android;

    public static bool AndroidTablet21 => Android && CoreEnvironmentVariables.Device == Emulator.AndroidTablet21;

    public static bool Android35 => Android && CoreEnvironmentVariables.Device == Emulator.Android35;

    // ReSharper disable once InconsistentNaming
    public static bool iOS => Platform == Platform.iOS;
    
    // Fix the static initialization order issue by using properties instead of readonly fields
    public static string DeviceApiVersion => Device.DeviceApiVersion;
    public static string PackageId => CoreEnvironmentVariables.PackageId;
}