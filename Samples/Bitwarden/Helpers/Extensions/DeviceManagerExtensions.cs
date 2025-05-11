using PuffinDom.Infrastructure.Helpers.DeviceManagers;

namespace Bitwarden.Helpers.Extensions;

public static class DeviceManagerExtensions_
{
    public static DeviceManager CloseBitwarden(this DeviceManager deviceManager)
    {
        deviceManager.CloseApp(BitwardenConstants.BundleId);

        return deviceManager;
    }

    public static DeviceManager OpenBitwarden(this DeviceManager deviceManager)
    {
        deviceManager.OpenApp(BitwardenConstants.BundleId);

        return deviceManager;
    }

    public static DeviceManager ReopenBitwarden(this DeviceManager deviceManager)
    {
        CloseBitwarden(deviceManager);
        OpenBitwarden(deviceManager);

        return deviceManager;
    }
}