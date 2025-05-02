using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PuffinDom.Infrastructure.Helpers.Device;
using PuffinDom.Infrastructure.Helpers.DeviceLog;
using PuffinDom.Tools;
using PuffinDom.Tools.Droid;
using PuffinDom.Tools.Droid.Enums;
using PuffinDom.Tools.IOS;
using PuffinDom.Tools.Logging;
using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Appium;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Infrastructure.Helpers.DeviceManagers;

public partial class DeviceManager
{
    public string? AndroidOpenedAppBundleId
    {
        get
        {
            string? bundleId = null;

            WaitCondition(
                () =>
                {
                    switch (Platform)
                    {
                        default:
                        case Platform.Android:
                            bundleId = Adb.GetOpenedAppBundleId(PuffinEnvironmentVariables.DroidEmulatorId);
                            break;
                        case Platform.iOS:
                            // TODO find out how to receive
                            return true;
                    }

                    return bundleId != null;
                },
                "Any app opened",
                true);

            Log.Write(
                bundleId is Adb.LauncherBundleId or Adb.LauncherBundleId2
                    ? $"Current app bundle ID: {bundleId} | Android Home Screen"
                    : $"Current app bundle ID: {bundleId}");

            return bundleId;
        }
    }

    public List<string> GetAndroidDevicePingLogs(string url)
    {
        _devicePingLogs.TryGetValue(url, out var value);

        return value?.LogQueue ?? [];
    }

    public DeviceManager StartAndroidDevicePingStream(List<string> proxyList)
    {
        if (Platform == Platform.iOS)
            return this;

        var pingUrls = PuffinConstants.PingUrls;
        pingUrls.AddRange(proxyList);

        foreach (var url in pingUrls)
        {
            var devicePingLogs = new AndroidPingLogCollector(Platform, url);
            devicePingLogs.StartLogStream(BootedDeviceUdid, url);

            _devicePingLogs.Add(url, devicePingLogs);
        }

        return this;
    }

    
    public DeviceManager EnableShowTaps()
    {
        using var logContext = Log.PushContext("Enable Show taps dev setting");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.SetShowTouches(PuffinEnvironmentVariables.DroidEmulatorId, true);
                break;
            case Platform.iOS:
                break;
        }

        return this;
    }

    public DeviceManager SetDebugApp(string bundleId)
    {
        using var logContext = Log.PushContext($"Set {bundleId} as debug app");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.SetDebugApp(PuffinEnvironmentVariables.DroidEmulatorId, bundleId);
                break;
            case Platform.iOS:
                break;
        }

        return this;
    }

    public bool InstallApkIfNotInstalled(string droidEmulatorId, string apkPath, string bundleId)
    {
        using var logContext = Log.PushContext($"Installing apk {bundleId}");

        switch (Platform)
        {
            default:
            case Platform.Android:
                return Adb.InstallApkIfNotInstalled(droidEmulatorId, apkPath, bundleId);
            case Platform.iOS:
                return false;
        }
    }

    
    [Obsolete("Not stable")]
    public bool InstallAabIfNotInstalled(string droidEmulatorId, string aabPath, string bundleId)
    {
        using var logContext = Log.PushContext($"Installing aab {bundleId}");

        switch (Platform)
        {
            default:
            case Platform.Android:
                return Adb.InstallAabIfNotInstalled(droidEmulatorId, aabPath, bundleId);
            case Platform.iOS:
                return false;
        }
    }

    public DeviceManager AssertBrowserOpened()
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                WaitCondition(
                    () => AndroidOpenedAppBundleId is PuffinConstants.AndroidBrowserBundleId or PuffinConstants.AndroidChromiumBrowserBundleId
                        or PuffinConstants.Android21BrowserBundleId,
                    "Browser is opened",
                    true);

                break;
            case Platform.iOS:
                //TODO
                break;
        }

        return this;
    }

    
    public DeviceManager SendKey(AndroidKeyCodes keycodePaste, bool waitAfter = true)
    {
        Adb.SendKey(PuffinEnvironmentVariables.DroidEmulatorId, keycodePaste, waitAfter);
        InvalidateCachedPageSource();

        return this;
    }

    public string DumpSysMemInfoPlainOnAndroid(string bundleId)
    {
        if (Platform == Platform.iOS)
            throw new NotSupportedException();

        return Adb.DumpSysMemInfoPlain(PuffinEnvironmentVariables.DroidEmulatorId, bundleId, false);
    }

    public DeviceManager ChangeUIThemeTo(AppTheme appTheme)
    {
        // ToDo add optional assert by comparing bitmap
        switch (appTheme)
        {
            default:
            case AppTheme.Light when Platform == Platform.Android:
                Adb.SwitchDeviceUIModeToLight();
                break;
            case AppTheme.Light when Platform == Platform.iOS:
                XCodeCommandLine.SwitchDeviceUIModeToLight();
                break;
            case AppTheme.Dark when Platform == Platform.Android:
                Adb.SwitchDeviceUIModeToDark();
                break;
            case AppTheme.Dark when Platform == Platform.iOS:
                XCodeCommandLine.SwitchDeviceUIModeToDark();
                break;
        }

        return this;
    }

    
    public DroidMemInfo? DumpSysMemInfo(string bundleId)
    {
        var result = Adb.DumpSysMemInfoPlain(PuffinEnvironmentVariables.DroidEmulatorId, bundleId, false);
        result = result.Substring(result.IndexOf("App Summary", StringComparison.Ordinal));

        var totalPssMatch = Regex.Match(result, @"(TOTAL PSS:.*\d )|(TOTAL:.*\d )");
        if (!totalPssMatch.Success)
            return null;

        var split = totalPssMatch.Value.Split(" ");
        var totalPss = int.Parse(split[^2]);

        var viewsCountMatch = Regex.Match(result, "Views:.*\\d ");

        if (!viewsCountMatch.Success)
            return null;

        var splitViewsCount = viewsCountMatch.Value.Split(" ");
        var viewsCount = int.Parse(splitViewsCount[^2]);

        var assetsCountMatch = Regex.Match(result, "Assets:.*\\d ");

        if (!assetsCountMatch.Success)
            return null;

        var assetsCountSplit = assetsCountMatch.Value.Split(" ");
        var assetsCount = int.Parse(assetsCountSplit[^2]);

        return new DroidMemInfo(totalPss, viewsCount, assetsCount);
    }

    
    public DeviceManager SetOemUnlock()
    {
        Adb.SetOemUnlock(PuffinEnvironmentVariables.DroidEmulatorId);

        return this;
    }

    public DeviceManager TrySetMaxLogBufferSize()
    {
        var increasingSizesArray = new[]
            { "1M", "2M", "3M", "4M", "5M", "6M", "7M", "8M", "9M", "10M", "11M", "12M", "13M", "14M", "15M", "16M", "17M", "18M", "19M", "20M" };

        foreach (var memorySize in increasingSizesArray)
            Adb.SetLogBufferSize(PuffinEnvironmentVariables.DroidEmulatorId, memorySize);

        return this;
    }

    
    public DeviceManager SetScreenAutoBrightness(bool turnOn)
    {
        Adb.SetScreenAutoBrightness(PuffinEnvironmentVariables.DroidEmulatorId, turnOn);
        return this;
    }

    public bool IsDeviceStableConnectedToInternet()
    {
        return Platform switch
        {
            Platform.Android => Adb.IsDeviceStableConnectedToInternet(PuffinEnvironmentVariables.DroidEmulatorId),
            Platform.iOS => true,
            _ => throw new PlatformNotSupportedException(),
        };
    }

    
    public DeviceManager SetKeepActivities()
    {
        if (Platform == Platform.Android)
            Adb.SetKeepActivities(PuffinEnvironmentVariables.DroidEmulatorId);

        return this;
    }

    
    public DeviceManager PressAndroidPhysicalBackSpaceButton()
    {
        if (Platform == Platform.Android)
            SendKey(AndroidKeyCodes.BACK);

        InvalidateCachedPageSource();

        return this;
    }

    public DeviceManager BootDeviceIfNeeded()
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                if (Adb.GetDevices().Count == 0)
                {
                    Log.Write("No devices found. Booting emulator");
                    EmulatorManager.BootEmulator(DroidEmulatorConfigurationHelper.Emulators[PuffinEnvironmentVariables.Device].Item1);
                }
                else
                    Log.Write("Emulator is already booted");

                break;
            case Platform.iOS:
                //XCodeCommandLine.BootDevice(IOSDeviceName);
                //ThreadSleep.For(10.Seconds());
                break;
        }

        return this;
    }

    public DeviceManager DisableAnimations()
    {
        using var logContext = Log.PushContext("Disabling Android animations");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.DisableAnimations(PuffinEnvironmentVariables.DroidEmulatorId);
                break;
            case Platform.iOS:
                break;
        }

        return this;
    }

    public DeviceManager ClearAppData(string bundleId)
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.ClearAppData(PuffinEnvironmentVariables.DroidEmulatorId, bundleId);
                ThreadSleep.For(200.Milliseconds(), "Waiting app data cleared");
                break;
            case Platform.iOS:
                break;
        }

        return this;
    }

    public DeviceManager EnableDeveloperMode()
    {
        using var logContext = Log.PushContext("Enable developer mode");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.SwitchDeveloperMode(PuffinEnvironmentVariables.DroidEmulatorId);
                break;
            case Platform.iOS:
                break;
        }

        return this;
    }

    public DeviceManager TurnOffInternet()
    {
        using var logContext = Log.PushContext("Turning off internet");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.TurnOffAllInternet(PuffinEnvironmentVariables.DroidEmulatorId);
                break;
            case Platform.iOS:
                break;
        }

        return this;
    }

    public DeviceManager TurnOnInternet()
    {
        using var logContext = Log.PushContext("Turning on internet");

        switch (Platform)
        {
            default:
            case Platform.Android:
                Adb.TurnOnAllInternet(PuffinEnvironmentVariables.DroidEmulatorId);
                break;
            case Platform.iOS:
                break;
        }

        return this;
    }

    public DeviceManager DismissKeyboard(bool assert = true)
    {
        using var logContext = Log.PushContext("Dismissing keyboard");

        InvalidateCachedPageSource();

        switch (Platform)
        {
            default:
            case Platform.Android:
            {
                if (_emulator == Emulator.AndroidTablet21)
                    return this;

                if (IsKeyboardVisible)
                    SendKey(AndroidKeyCodes.BACK, false);
                else if (assert)
                    throw new Exception("Keyboard is not visible");

                break;
            }
            case Platform.iOS:
            {
                throw new NotSupportedException("Close iOS with tapping on keyboard");
            }
        }

        ThreadSleep.For(PuffinConstants.DefaultDelayAfterAnyAction, "Delay after dismissing keyboard");

        WaitCondition(
            () => !IsKeyboardVisible,
            "Keyboard is dismissed",
            true);

        return this;
    }

    public DeviceManager DisableAndroidSpellChecker()
    {
        if (Platform == Platform.iOS)
            return this;

        Adb.DisableSpellChecker(PuffinEnvironmentVariables.DroidEmulatorId);

        return this;
    }

    public DeviceManager AndroidOnScreenKeyboardShouldBeShown()
    {
        if (Platform == Platform.iOS)
            return this;

        Adb.SetOnScreenKeyboardShouldBeShown(PuffinEnvironmentVariables.DroidEmulatorId);

        return this;
    }

    public DeviceManager SetAndroidScreenTimeoutOffToMaximum()
    {
        if (Platform == Platform.iOS)
            return this;

        Adb.SetScreenTimeoutOffToMaximum(PuffinEnvironmentVariables.DroidEmulatorId);

        return this;
    }

    public DeviceManager SetAndroidPoliciesToDefaults()
    {
        if (Platform == Platform.iOS)
            return this;

        Adb.SetHiddenApiPolicyPrePApps(PuffinEnvironmentVariables.DroidEmulatorId, 1);
        Adb.SetHiddenApiPolicyPApps(PuffinEnvironmentVariables.DroidEmulatorId, 1);
        Adb.SetHiddenApiPolicy(PuffinEnvironmentVariables.DroidEmulatorId, 1);

        return this;
    }

    public DeviceManager SetAndroidDeviceTime()
    {
        if (Platform == Platform.iOS)
            return this;

        Adb.RunCommand(
            PuffinEnvironmentVariables.DroidEmulatorId,
            $"date -s @{DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000}");

        return this;
    }
}