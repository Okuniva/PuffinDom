using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using PuffinDom.Infrastructure.Appium.Helpers;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Infrastructure.Helpers.DeviceLog;

namespace PuffinDom.Infrastructure.Helpers.DeviceManagers;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class DeviceManagerContainer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Emulator _emulator;
    private readonly Platform _platform;

    public DeviceManagerContainer(Emulator emulator)
    {
        _emulator = emulator;
        _platform = GetPlatformFromEmulator(emulator);
        
        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<ILogCollector, DeviceLogCollector>();
        services.AddSingleton<IAppiumServerWrapper, AppiumServerWrapper>();
        services.AddSingleton<IDeviceManager, DeviceManager>();
        services.AddSingleton<DeviceManager>();
        services.AddSingleton<IScreenshotService, ScreenshotService>();
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    private Platform GetPlatformFromEmulator(Emulator emulator) => emulator switch
    {
        Emulator.Android33 => Platform.Android,
        Emulator.Android35 => Platform.Android,
        Emulator.AndroidTablet21 => Platform.Android,
        Emulator.iOSLatest => Platform.iOS,
        _ => throw new ArgumentOutOfRangeException(nameof(emulator), emulator, null),
    };
    
    public DeviceManager GetDeviceManager() => _serviceProvider.GetRequiredService<DeviceManager>();
    
    public T GetService<T>() where T : class => _serviceProvider.GetRequiredService<T>();
    
    public Emulator GetEmulator() => _emulator;
    public Platform GetPlatform() => _platform;
}
