using System.Diagnostics.CodeAnalysis;
using StrongInject;
using PuffinDom.Infrastructure.Appium.Helpers;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Infrastructure.Helpers.DeviceLog;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;

namespace PuffinDom.Infrastructure.Helpers;

[Register(typeof(DeviceLogCollector), Scope.SingleInstance, typeof(ILogCollector))]
[Register(typeof(AppiumServerWrapper), Scope.SingleInstance, typeof(IAppiumServerWrapper))]
[Register(typeof(DeviceManager), Scope.SingleInstance, typeof(IDeviceManager), typeof(DeviceManager))]
[Register(typeof(ScreenshotService), Scope.SingleInstance, typeof(IScreenshotService))]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public partial class DeviceManagerContainerRegistration : IContainer<DeviceManager>
{
    public DeviceManagerContainerRegistration(Emulator emulator)
    {
        Emulator = emulator;
    }

    [Instance]
    public Emulator Emulator { get; }

    [Instance]
    public Platform Platform => Emulator switch
    {
        Emulator.Android33 => Platform.Android,
        Emulator.Android35 => Platform.Android,
        Emulator.AndroidTablet21 => Platform.Android,
        Emulator.iOSLatest => Platform.iOS,
        _ => throw new ArgumentOutOfRangeException(nameof(Emulator), Emulator, null),
    };
}