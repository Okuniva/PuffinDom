using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using PuffinDom.Infrastructure.Appium;
using PuffinDom.Infrastructure.Appium.Helpers;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Infrastructure.Helpers.DeviceLog;
using PuffinDom.Infrastructure.WaitingStrategy;
using PuffinDom.Tools;
using PuffinDom.Tools.Droid;
using PuffinDom.Tools.Droid.Enums;
using PuffinDom.Tools.IOS;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Infrastructure.Helpers.DeviceManagers;

[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Local")]
public partial class DeviceManager : IDeviceManager, IDisposable
{
    private const string IOSDeviceName = "booted";

    private static double? _density;
    private static Rect? _deviceRect;
    private readonly AppiumDriverWrapper _appiumClientDriverWrapper;
    private readonly IAppiumServerWrapper _appiumServerContext;
    private readonly ILogCollector _deviceLogs;
    private readonly Dictionary<string, AndroidPingLogCollector> _devicePingLogs;
    private readonly Emulator _emulator;
    private readonly MacOSPingLogCollector _macOsPingLogCollector;
    private readonly IScreenshotService _screenshotService;
    private string? _api;

    public DeviceManager(
        Platform platform,
        Emulator emulator,
        ILogCollector logCollector,
        IAppiumServerWrapper appiumServerWrapper,
        IScreenshotService screenshotService)
    {
        _emulator = emulator;
        Platform = platform;
        _deviceLogs = logCollector;
        _appiumServerContext = appiumServerWrapper;
        _screenshotService = screenshotService;
        _appiumClientDriverWrapper = new AppiumDriverWrapper(this);
        _devicePingLogs = new Dictionary<string, AndroidPingLogCollector>();
        _macOsPingLogCollector = new MacOSPingLogCollector(platform, "Mac os ping logs");
    }

    public bool IsKeyboardVisible =>
        _appiumClientDriverWrapper
            .IsKeyboardVisible();

    public string DeviceApiVersion => _api ??= Platform switch
    {
        Platform.Android => Adb.GetDeviceAndroidApi(PuffinEnvironmentVariables.DroidEmulatorId),
        Platform.iOS => XCodeCommandLine.GetIOSDeviceIdentifier(IOSDeviceName),
        _ => throw new ArgumentOutOfRangeException(),
    };

    public double Density
    {
        get
        {
            if (_density != null)
                return _density.Value;

            switch (Platform)
            {
                case Platform.Android:
                    _density = Adb.GetDensity(PuffinEnvironmentVariables.DroidEmulatorId);
                    break;
                case Platform.iOS:
                    var screenInfo = XCodeCommandLine.GetDeviceScreenInfo();
                    _deviceRect = new Rect(0, 0, screenInfo.Width, screenInfo.Height);
                    _density = screenInfo.Density;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return _density.Value;
        }
    }

    public Platform Platform { get; }

    
    public string BootedDeviceUdid
    {
        get
        {
            switch (Platform)
            {
                default:
                case Platform.Android:
                    return PuffinEnvironmentVariables.DroidEmulatorId;
                case Platform.iOS:
                    return XCodeCommandLine.GetBootedDeviceUdid();
            }
        }
    }

    public Rect DeviceRect
    {
        get
        {
            if (_deviceRect != null)
                return _deviceRect;

            switch (Platform)
            {
                default:
                case Platform.Android:
                {
                    var size = Adb.GetDeviceScreenSize(PuffinEnvironmentVariables.DroidEmulatorId);
                    _deviceRect = new Rect(0, 0, size.Width, size.Height);
                    break;
                }
                case Platform.iOS:
                {
                    var screenInfo = XCodeCommandLine.GetDeviceScreenInfo();
                    _deviceRect = new Rect(0, 0, screenInfo.Width, screenInfo.Height);
                    _density = screenInfo.Density;
                    break;
                }
            }

            return _deviceRect;
        }
    }

    public DeviceManager CloseApp(string bundleId, bool assert = true)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.CloseApp(PuffinEnvironmentVariables.DroidEmulatorId, bundleId, assert);
                break;
            case Platform.iOS:
                XCodeCommandLine.CloseApp(IOSDeviceName, bundleId, assert);
                break;
        }

        TakeScreenshot();

        return this;
    }

    public DeviceManager RemoveAllFilesFromSDCard()
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.RemoveAllFilesFromDeviceFolder(
                    PuffinEnvironmentVariables.DroidEmulatorId,
                    PuffinEnvironmentVariables.ScreenshotsDirectory);

                break;
            case Platform.iOS:
                if (Directory.Exists(PuffinConstants.IOSScreenshotsFolderName))
                    Directory.Delete(PuffinConstants.IOSScreenshotsFolderName, true);

                break;
        }

        return this;
    }

    public DeviceManager CollectAllScreenshots(string destinationFolder)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.DownloadFolder(PuffinEnvironmentVariables.DroidEmulatorId, PuffinEnvironmentVariables.ScreenshotsDirectory, destinationFolder);
                RemoveAllUnnecessaryDirectories(destinationFolder);
                break;

                static void RemoveAllUnnecessaryDirectories(string destinationFolder)
                {
                    foreach (var directory in Directory.GetDirectories(destinationFolder))
                        Directory.Delete(directory, true);
                }
            case Platform.iOS:
                Directory.Move(PuffinConstants.IOSScreenshotsFolderName, destinationFolder);

                break;
        }

        return this;
    }

    public DeviceManager PressHomeButton()
    {
        using var logContext = Log.PushContext("Pressing Home Button");

        switch (Platform)
        {
            default:
            case Platform.Android:

                var bundleId = AndroidOpenedAppBundleId;

                if (AndroidOpenedAppBundleId is Adb.LauncherBundleId or Adb.LauncherBundleId2)
                {
                    Log.Write("No need to press Home button. Already on home screen");
                    return this;
                }

                Adb.SendKey(PuffinEnvironmentVariables.DroidEmulatorId, AndroidKeyCodes.HOME);

                WaitCondition(
                    () => bundleId != AndroidOpenedAppBundleId,
                    "Home screen is opened",
                    true);

                break;
            case Platform.iOS:
                _appiumClientDriverWrapper.PressHomeButton();
                break;
        }

        InvalidateCachedPageSource();
        return this;
    }

    
    public DeviceManager RemoveApp(string bundleId, bool removeCalabashService = true, bool log = true)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.RemoveApp(PuffinEnvironmentVariables.DroidEmulatorId, bundleId, log);
                break;
            case Platform.iOS:
            {
                XCodeCommandLine.UninstallApp(PuffinConstants.iOSSimulatorName, bundleId, false, log);
                ThreadSleep.For(4.Seconds(), "Delay after uninstalling app");
                if (removeCalabashService)
                {
                    // ReSharper disable once StringLiteralTypo
                    XCodeCommandLine.UninstallApp(PuffinConstants.iOSSimulatorName, "sh.calaba.DeviceAgent.xctrunner", false, log);
                    ThreadSleep.For(4.Seconds(), "Delay after uninstalling app");
                }

                break;
            }
        }

        InvalidateCachedPageSource();

        return this;
    }

    public void TakeScreenshotAndSaveLocally()
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.TakeScreenshotAndSaveLocally(PuffinEnvironmentVariables.DroidEmulatorId, PuffinEnvironmentVariables.ScreenshotsDirectory);
                break;
            case Platform.iOS:
                XCodeCommandLine.TakeScreenshot(IOSDeviceName, PuffinConstants.IOSScreenshotsFolderName);
                break;
        }
    }

    
    public DeviceManager ShutdownDevice()
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.ShutdownVirtualDevice(PuffinEnvironmentVariables.DroidEmulatorId);
                break;
            case Platform.iOS:
                XCodeCommandLine.ShutdownDevice(IOSDeviceName, false);
                ThreadSleep.For(10.Seconds(), "Delay after device shutdown");
                break;
        }

        return this;
    }

    public DeviceManager TakeScreenshot(string? fileNamePostfix = null, bool force = false, bool assert = true)
    {
        _screenshotService.TakeScreenshot(Platform, fileNamePostfix, force, assert);
        return this;
    }

    public DeviceManager OpenDeepLink(string deepLink, string scheme, string deepLinkHost)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.OpenDeepLink(PuffinEnvironmentVariables.DroidEmulatorId, deepLink, scheme, deepLinkHost);
                break;
            case Platform.iOS:
                XCodeCommandLine.OpenDeepLink(IOSDeviceName, deepLink, scheme, deepLinkHost);
                break;
        }

        InvalidateCachedPageSource();
        return this;
    }

    public DeviceManager StopAppiumClientDriver()
    {
        _appiumClientDriverWrapper.Stop();
        return this;
    }

    public DeviceManager InvalidateCachedPageSource()
    {
        _appiumClientDriverWrapper.InvalidateCachedPageSource();
        return this;
    }

    public DeviceManager OpenApp(string bundleId, bool log = true)
    {
        using var logContext = Log.PushContext($"Launching app {bundleId}");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.StartApp(PuffinEnvironmentVariables.DroidEmulatorId, bundleId, log);
                break;
            case Platform.iOS:
                XCodeCommandLine.StartApp(IOSDeviceName, bundleId, log);
                break;
        }

        return this;
    }

    public IDisposable TurnOffScreenshots()
    {
        return _screenshotService.TurnOffScreenshots();
    }

    public DeviceManager TapCoordinates(int x, int y, int times = 1)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:

                for (var i = 0; i < times; i++)
                    Adb.TapCoordinates(PuffinEnvironmentVariables.DroidEmulatorId, x, y);

                break;

            case Platform.iOS:
                _appiumClientDriverWrapper
                    .TapCoordinates(x, y, times);

                break;
        }

        InvalidateCachedPageSource();

        ThreadSleep.For(
            PuffinConstants.DefaultDelayAfterAnyAction,
            "Waiting after tap");

        return this;
    }

    public DeviceManager ClearText(int lettersCount)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                PressBackSpaceButton(lettersCount);
                break;
            case Platform.iOS:
                _appiumClientDriverWrapper
                    .ClearText();

                break;
        }

        InvalidateCachedPageSource();
        return this;

        void PressBackSpaceButton(int times = 1)
        {
            for (var i = 0; i < times; i++)
                Adb.SendKey(
                    PuffinEnvironmentVariables.DroidEmulatorId,
                    AndroidKeyCodes.FORWARD_DEL,
                    false);
        }
    }

    public DeviceManager EnterText(string text)
    {
        Log.Write("Typing text: " + text);

        if (_emulator == Emulator.AndroidTablet21)
            text = text.Replace(" ", "%s");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.InputText(PuffinEnvironmentVariables.DroidEmulatorId, text, false);
                InvalidateCachedPageSource();
                break;
            case Platform.iOS:
                _appiumClientDriverWrapper
                    .EnterText(text);

                break;
        }

        InvalidateCachedPageSource();
        return this;
    }

    public DeviceManager EnterTextBySymbols(string text)
    {
        Log.Write("Typing text by symbols: " + text);

        foreach (var symbol in text)
            EnterText(symbol.ToString());

        return this;
    }

    public string GetScreenAsXml(string contextName, bool full = false)
    {
        return _appiumClientDriverWrapper
            .PageSource(contextName, full);
    }

    public DeviceManager DragCoordinates(
        int fromXPx,
        int fromYPx,
        int toXPx,
        int toYPx,
        bool preventInertia = true,
        TimeSpan? duration = null)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.Swipe(
                    PuffinEnvironmentVariables.DroidEmulatorId,
                    fromXPx,
                    fromYPx,
                    toXPx,
                    toYPx,
                    duration ?? PuffinConstants.DefaultDragDuration);

                break;
            case Platform.iOS:
                _appiumClientDriverWrapper
                    .DragCoordinates(fromXPx, fromYPx, toXPx, toYPx, preventInertia, duration);

                break;
        }

        InvalidateCachedPageSource();
        return this;
    }

    public DeviceManager TouchAndHoldRect(Rect viewRect)
    {
        return DragCoordinates(
            viewRect.CenterX,
            viewRect.CenterY,
            viewRect.CenterX,
            viewRect.CenterY,
            true,
            PuffinConstants.TapAndHoldDefaultHoldDuration);
    }

    
    public DeviceManager InstallApp(string pathToBundleFile, bool log = true)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.InstallApp(PuffinEnvironmentVariables.DroidEmulatorId, pathToBundleFile);
                break;
            case Platform.iOS:
                XCodeCommandLine.InstallApp(IOSDeviceName, pathToBundleFile, log);
                ThreadSleep.For(10.Seconds(), "Delay after app installation");
                break;
        }

        return this;
    }

    public DeviceManager RestartAppiumFully(bool startTouchApp = true)
    {
        using var logContext = Log.PushContext("Restarting Appium Fully");

        var attempt = 0;

        while (true)
            try
            {
                RecreateAppiumServer();
                _appiumClientDriverWrapper.RecreateDriver(startTouchApp);
                break;
            }
            catch (Exception e)
            {
                attempt++;

                Log.Write(e, $"Failed to start Appium fully after {attempt} attempts");

                if (attempt == 3)
                    throw new TechnicalCrashFailTestException($"Failed to start Appium fully after {attempt} attempts", e);

                ThreadSleep.For(PuffinConstants.AppiumServerStartRetriesDelay, "Delay between appium restarts");
            }

        return this;
    }

    public DeviceManager StartAppiumClientDriver()
    {
        _appiumClientDriverWrapper.RecreateDriver();
        return this;
    }

    private DeviceManager RecreateAppiumServer()
    {
        _appiumServerContext.RestartAppiumServer();
        return this;
    }

    public DeviceManager StopAppiumFully()
    {
        StopAppiumClientDriver();
        _appiumServerContext.Dispose();
        return this;
    }

    public DeviceManager StartAppiumIfNeeded()
    {
        using var logContext = Log.PushContext("Starting Appium Server & Driver");

        try
        {
            _appiumServerContext.RestartAppiumServer(false);
            _appiumClientDriverWrapper.RestartDriverIfNeeded();
            InvalidateCachedPageSource();
            _appiumClientDriverWrapper.PageSource("Test Appium Started");
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception e)
        {
            Log.Write(e, "Failed to start Appium server");

            if (Platform == Platform.iOS)
            {
                ShutdownAllIOSDevices();
                EraseAllIOSDevices();
            }

            ThreadSleep.For(1.Minute(), "Delay between appium restarts");
            _appiumServerContext.RestartAppiumServer(false);
            _appiumClientDriverWrapper.RestartDriverIfNeeded();
        }

        return this;
    }

    private bool WaitCondition(
        Func<bool> waitCondition,
        string conditionName,
        bool assert = false,
        TimeSpan? timeout = null,
        Func<string>? customErrorMessage = null)
    {
        return ConditionWaitingStrategy.WaitCondition(
            this,
            waitCondition,
            conditionName,
            assert,
            timeout,
            customErrorMessage);
    }

    public string GetAppProcessId(string bundleId)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                return Adb.GetAppProcessId(PuffinEnvironmentVariables.DroidEmulatorId, bundleId);
            case Platform.iOS:
                return XCodeCommandLine.GetAppProcessId(bundleId);
        }
    }

    public DateTime GetDeviceTime()
    {
        return Platform == Platform.iOS
            ? DateTime.Now
            : Adb.GetAndroidDeviceTime(PuffinEnvironmentVariables.DroidEmulatorId);
    }

    public void Dispose()
    {
        StopDeviceLogStream();
        StopAppiumFully();
    }
}