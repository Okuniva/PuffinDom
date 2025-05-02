using PuffinDom.Tools;
using PuffinDom.Tools.IOS;

namespace PuffinDom.Infrastructure.Helpers.DeviceManagers;

public partial class DeviceManager
{
    public DeviceManager EraseAllIOSDevices(bool assert = true)
    {
        if (RunningOSTools.IsMacOS)
            XCodeCommandLine.EraseAllDevices(assert);

        return this;
    }

    public DeviceManager ShutdownAllIOSDevices(bool assert = true)
    {
        if (RunningOSTools.IsMacOS)
            XCodeCommandLine.ShutdownAllDevices(assert);

        return this;
    }

    
    public DeviceManager EraseDevice(string? iOSDeviceName = null, bool assert = true)
    {
        iOSDeviceName ??= IOSDeviceName;

        switch (Platform)
        {
            default:
            case Platform.Android:
                break;
            case Platform.iOS:
                XCodeCommandLine.EraseDevice(iOSDeviceName, assert: assert);
                break;
        }

        return this;
    }
}