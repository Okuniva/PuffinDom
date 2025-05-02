using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Helpers;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;

namespace PuffinDom.UI;

public class UIContext
{
    private static DeviceManager? _deviceManager;
    
    public static DeviceManager Device =>
        _deviceManager ??= new DeviceManagerContainer(PuffinEnvironmentVariables.Device).GetDeviceManager();

    private static Platform? _platform;

    public static Platform Platform
    {
        get => _platform ?? (PuffinEnvironmentVariables.RunDroid ? Platform.Android : Platform.iOS);
        set => _platform = value;
    }

    public static bool Android => Platform == Platform.Android;

    public static bool AndroidTablet21 => Android && PuffinEnvironmentVariables.Device == Emulator.AndroidTablet21;

    public static bool Android35 => Android && PuffinEnvironmentVariables.Device == Emulator.Android35;

    // ReSharper disable once InconsistentNaming
    public static bool iOS => Platform == Platform.iOS;
    public static readonly string DeviceApiVersion = _deviceManager!.DeviceApiVersion;
    public static readonly string PackageId = PuffinEnvironmentVariables.PackageId;
}