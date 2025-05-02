using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using PuffinDom.Tools.Droid.Enums;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.ExternalApplicationsTools.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Tools.Droid;

public class Adb
{
    public const string LauncherBundleId = "com.google.android.apps.nexuslauncher";
    public const string LauncherBundleId2 = "com.android.launcher3";

    
    public static ConnectedDevice? GetVirtualDeviceWithId(string avdId)
    {
        var devices = GetDevices();

        foreach (var device in devices)
            try
            {
                var deviceAvdId = GetDeviceName(device.Serial);
                if (deviceAvdId.Equals(avdId, StringComparison.OrdinalIgnoreCase))
                    return device;
            }
            catch (Exception e)
            {
                Log.Write($"Failed to get virtual device ID for device '{device.Serial}': {e.Message} during search for '{avdId}'");
            }

        return null;
    }

    
    public static IEnumerable<ConnectedDevice> GetVirtualDevicesWithId(string avdId)
    {
        var devices = GetDevices();

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var device in devices)
        {
            var deviceAvdId = GetDeviceName(device.Serial);
            if (deviceAvdId.Equals(avdId, StringComparison.OrdinalIgnoreCase))
                yield return device;
        }
    }

    public static void SaveApplicationTombstone(string bugreportFileName)
    {
        ExternalProgramRunner.Run("adb", $"bugreport {bugreportFileName}");
    }

    public static List<ConnectedDevice> GetDevices()
    {
        return GetDevicesNoLogging();
    }

    
    public static ConnectedDevice? GetDevice(string serial)
    {
        Log.Write($"Searching for connected device '{serial}'");

        var devices = GetDevicesNoLogging();

        return devices.FirstOrDefault(d => d.Serial.Equals(serial, StringComparison.OrdinalIgnoreCase));
    }

    public static void ClearLogcat(string serial)
    {
        Log.Write($"Clearing logcat for '{serial}'");

        EnsureDeviceVisible(serial);

        ExternalProgramRunner.Run(
            "adb",
            $"-s {serial} logcat -c",
            message: "Clearing logcat");
    }

    public static void InstallApp(string serial, string appPath, bool assert = true, bool log = true)
    {
        Log.Write($"Installing '{appPath}' on virtual device '{serial}'");

        EnsureDeviceVisible(serial);

        ExternalProgramRunner.Run(
            "adb",
            $"-s \"{serial}\" install \"{appPath}\"",
            assert: assert,
            log: log,
            message: $"Installing '{appPath}' bundle");
    }

    public static void DisableAnimations(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global window_animation_scale 0.0",
            message: "Disabling window animation scale");

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global transition_animation_scale 0.0",
            message: "Disabling transition animation scale");

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global animator_duration_scale 0.0",
            message: "Disabling animator duration scale");
    }

    public static void ShutdownVirtualDevice(string serial)
    {
        Log.Write($"Shutting down virtual device with serial '{serial}'...");

        EnsureDeviceVisible(serial);

        ExternalProgramRunner.Run(
            "adb",
            $"-s \"{serial}\" emu kill",
            message: $"Shutting down virtual device with serial {serial}");

        EnsureShutdown(serial);
    }

    
    public static string GetDataDirectory(string serial, string packageName)
    {
        Log.Write($"Retrieving data path for app '{packageName}' on device '{serial}'");

        var result = RunCommandAsApp(serial, packageName, "pwd");
        var packageDataRoot = result.Output.Trim();

        return $"{packageDataRoot}/files";
    }

    private static ProcessResult RunCommandAsApp(string serial, string packageName, string command)
    {
        Log.Write($"Running command '{command}' as app '{packageName}' on device '{serial}'...");

        command = $"run-as \"{packageName}\" {command}";
        return RunCommand(serial, command);
    }

    public static ProcessResult RunCommand(string serial, string command, bool assert = true)
    {
        Log.Write($"Running command '{command}' on device '{serial}'...");

        EnsureDeviceVisible(serial);

        return ExternalProgramRunner.Run(
            "adb",
            $"-s \"{serial}\" shell {command}",
            assert: assert);
    }

    public static DateTime GetAndroidDeviceTime(string serial)
    {
        return DateTime.Parse(
            RunCommand(serial, "date +'%Y-%m-%d_%H:%M:%S.%3N'")
                .Output.Replace('_', ' '));
    }

    private static void EnsureShutdown(string serial)
    {
        while (IsDeviceVisible(serial))
            ThreadSleep.For(1.Second(), "Delay between validation if device shutdown");
    }

    public static bool IsDeviceStableConnectedToInternet(string deviceId)
    {
        using var logContext = Log.PushContext("Check device internet connection");
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell ping -c 2 8.8.8.8",
            message: "Ping 8.8.8.8",
            assert: false);

        if (!result.IsSuccess)
            foreach (var line in result.GetErrorOutput())
                if (line.Equals("connect: Network is unreachable"))
                {
                    Log.Write("Android Device Failed to ping 8.8.8.8");
                    return false;
                }

        if (result.IsSuccess)
        {
            Log.WriteInDebug("Ping result:");
            foreach (var line in result.GetOutput())
            {
                Log.WriteInDebug(line);
                if (!line.Contains("2 packets transmitted, 2 received, 0% packet loss"))
                    continue;

                Log.Write("Android Device Successfully pinged 8.8.8.8");
                return true;
            }
        }

        Log.Write("Android Device Failed to ping 8.8.8.8");
        return false;
    }

    private static void EnsureDeviceVisible(string serial)
    {
        var foundDevice = IsDeviceVisible(serial);
        if (!foundDevice)
            throw new Exception($"Unable to find virtual device '{serial}'.");
    }

    private static bool IsDeviceVisible(string serial)
    {
        var devices = GetDevicesNoLogging();
        return devices.Any(d => d.Serial.Equals(serial, StringComparison.OrdinalIgnoreCase));
    }

    private static List<ConnectedDevice> GetDevicesNoLogging()
    {
        using var logContext = Log.PushContext("Searching for connected devices");

        var result = ExternalProgramRunner.Run(
            "adb",
            "devices",
            message: "Getting connected devices");

        var devices = new List<ConnectedDevice>();

        foreach (var line in result.GetOutput())
        {
            if (!line.Contains('\t'))
                continue;

            var splitChar = new[] { '\t' };
            var parts = line.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                continue;

            var serial = parts[0].Trim();
            var state = parts[1].Trim().ToLowerInvariant() switch
            {
                "device" => ConnectedDeviceState.Connected,
                "offline" => ConnectedDeviceState.Disconnected,
                "no device" => ConnectedDeviceState.Unknown,
                _ => ConnectedDeviceState.Unknown,
            };

            devices.Add(new ConnectedDevice(serial, state));
        }

        return devices;
    }

    public static void ClearAppData(string deviceId, string packageName)
    {
        using var logContext = Log.PushContext($"Clearing app data for '{packageName}'");

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell pm clear {packageName}",
            message: "Clearing app data");
    }

    
    public static void SetMaxLogBufferSize(string deviceId)
    {
        var increasingSizesArray = new[] { "5M", "8M", "10M" };
        foreach (var memorySize in increasingSizesArray)
            ExternalProgramRunner.Run(
                "adb",
                $"-s {deviceId} logcat -G {memorySize}",
                message: $"Setting log buffer size to {memorySize}");
    }

    public static void SetKeepActivities(string deviceId, bool turnOn = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId}" +
            $" shell settings put global always_finish_activities {(turnOn ? "1" : "0")}",
            message: "Setting keep activities " + (turnOn ? "on" : "off"));
    }

    
    public static void SetShowAllANR(string deviceId, bool turnOn = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} " +
            $"shell settings put global show_all_anrs {(turnOn ? "1" : "0")}",
            message: "Setting show all anrs");
    }

    
    public static void SetDeveloperSettingsShowAllBackgroundCrashes(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global show_background_crashes 1",
            message: "Setting show background crashes");
    }

    
    public static void SetDeveloperSettingsHideAllBackgroundCrashes(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global show_background_crashes 0",
            message: "Setting hide background crashes");
    }

    
    public static void SetDeveloperSettingsShowAllCrashes(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global show_crashes 1",
            message: "Setting show all crashes");
    }

    
    public static void SetDeveloperSettingsHideAllCrashes(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global show_crashes 0",
            message: "Setting hide all crashes");
    }

    
    public static void SetDeveloperSettingsShowAllStrictModeViolations(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global show_strict_mode_violations 1",
            message: "Setting show all strict mode violations");
    }

    
    public static void SetDeveloperSettingsHideAllStrictModeViolations(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global show_strict_mode_violations 0",
            message: "Setting hide all strict mode violations");
    }

    public static void SetOemUnlock(string deviceId, bool turnOn = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} " +
            $"shell settings put global oem_unlock_allowed {(turnOn ? "1" : "0")}",
            message: "Setting oem unlock " + (turnOn ? "on" : "off"));
    }

    public static void SwitchDeveloperMode(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global development_settings_enabled 1");
    }

    
    public static void SwitchAirplaneMode(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global airplane_mode_on 1",
            message: "Setting airplane mode on");
    }

    
    public static void SwitchAirplaneModeOff(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} settings put global airplane_mode_on 0",
            message: "Setting airplane mode off");
    }

    
    public static void SetNFC(string deviceId, bool turnOn = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global nfc_on {(turnOn ? "1" : "0")}");
    }

    public static void SetShowTouches(string deviceId, bool turnOn)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put system show_touches {(turnOn ? "1" : "0")}");
    }

    
    public static void SwitchStayAwake()
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell settings put global stay_on_while_plugged_in 1",
            message: "Setting stay_awake on");
    }

    
    public static void SwitchStayAwakeOff()
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell settings put global stay_on_while_plugged_in 0",
            message: "Setting stay_awake off");
    }

    
    public static void SwitchSound()
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell settings put global sound_effects_enabled 1");
    }

    
    public static void SwitchSoundOff()
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell settings put global sound_effects_enabled 0");
    }

    
    public static void SwitchLockScreen()
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell settings put global lockscreen.disabled 1",
            message: "Setting lock screen On");
    }

    
    public static void SwitchLockScreenOff()
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell settings put global lockscreen.disabled 0",
            message: "Setting lock screen Off");
    }

    public static void SetScreenTimeoutOffToMaximum(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} " +
            "shell settings put global screen_off_timeout 2147483647",
            message: "Setting screen timeout to too long");
    }

    public static void SetScreenAutoBrightness(string deviceId, bool turnOn)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} " +
            $"shell settings put global screen_brightness_mode {(turnOn ? "1" : "0")}",
            message: "Setting screen auto brightness " + (turnOn ? "on" : "off"));
    }

    public static void SendKey(string deviceId, AndroidKeyCodes code, bool waitAfter = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell input keyevent {(int)code}",
            message: $"Sending key '{code}' with code {(int)code}");

        if (waitAfter)
            ThreadSleep.For(1.Second(), "Sleep after keyevent");
    }

    public static void InputText(string deviceId, string text, bool waitAfter = true)
    {
        var safeText = text
            .Replace(" ", "\\ ")
            .Replace("!", "\\!")
            .Replace("@", "\\@")
            .Replace("#", "\\#")
            .Replace("'", "\\'")
            .Replace("\"", "\\\"")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("%", "\\%");

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell input text \"{safeText}\"",
            message: $"Typing text '{text}'");

        if (waitAfter)
            ThreadSleep.For(1.Second(), "Sleep after text input");
    }

    
    private static void KillAppBackgroundProcesses(string deviceId, string bundleId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell am kill {bundleId}");
    }

    public static void RemoveApp(string deviceId, string bundleId, bool log = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} " +
            $"shell pm uninstall {bundleId}",
            assert: false,
            log: log,
            message: $"Removing app {bundleId}");

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} " +
            $"shell pm uninstall {bundleId}.test",
            assert: false,
            log: log,
            message: $"Removing app {bundleId}.test");
    }

    
    //TODO try to use
    public static TimeSpan OpenAppAndMeasureStart(string deviceId, string bundleId, string activityName)
    {
        var output = ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell am start -W -n {bundleId}/{activityName}");

        var totalTimeLineFromOutput = Regex.Match(output.Output, "TotalTime: (\\d+)").Value;
        var totalTimeText = int.Parse(Regex.Match(totalTimeLineFromOutput, "\\d+").Value);
        return totalTimeText.Milliseconds();
    }

    
    public static void OpenApp(string deviceId, string bundleId, string activity, bool waitAppOpened = true)
    {
        var waitAppOpenedParameter = waitAppOpened
            ? "-W"
            : "";

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell am start {waitAppOpenedParameter} " +
            $"-n {bundleId}/{activity}",
            message: "Opening app " + bundleId + " with activity " + activity);
    }

    public static void CloseApp(string deviceId, string appName, bool assert = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell am force-stop {appName}",
            assert: assert,
            message: $"Closing app {appName}");
    }

    
    public static void RebootDevice(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} reboot",
            message: "Rebooting device");
    }

    
    public static void StartVideoRecording(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell screenrecord --bit-rate 1000000 /sdcard/video.mp4",
            message: "Starting video recording");
    }

    public static void RemoveAllFilesFromDeviceFolder(string deviceId, string android21ScreenshotFolder)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell \"rm -rf {android21ScreenshotFolder}*\"",
            assert: false,
            message: "Removing all files from sdcard");
    }

    public static string TakeScreenshotAndSaveLocally(string deviceId, string? filePath = null)
    {
        const string screenshotName = "screenshot.png";

        filePath ??= "/sdcard/";

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell screencap -p \"{filePath}{screenshotName}\"",
            message: $"Taking screenshot and save to {filePath}");

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} pull \"{filePath}{screenshotName}\"",
            message: $"Pulling screenshot from {filePath}");

        return screenshotName;
    }

    public static void TakeScreenshot(string deviceId, string? fileName = null, bool assert = true, string? filePath = null)
    {
        fileName ??= $"{DateTime.Now:HH-mm-ss-fff}";

        filePath ??= "/sdcard/";

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell screencap -p \"{filePath}{fileName}.png\"",
            message: $"Take screenshot and save to {filePath}",
            timeout: 10.Seconds(),
            assert: assert);
    }

    public static string CollectLogs(string deviceId)
    {
        var parameters = $"-s {deviceId} logcat -d -v time";

        var collectLogs = ExternalProgramRunner.Run(
                "adb",
                parameters,
                log: false,
                collectOutput: true,
                message: "Collecting logcat")
            .Output;

        Log.Write($"Collected {collectLogs.Length} symbols");

        return collectLogs;
    }

    public static string DumpSysMemInfoPlain(string deviceId, string bundleId, bool log = true)
    {
        return ExternalProgramRunner.Run(
                "adb",
                $"-s {deviceId} shell dumpsys meminfo {bundleId}",
                log: log,
                message: "Collecting device memory info")
            .Output;
    }

    public static void DownloadFolder(string deviceId, string sourceFolder, string destinationFolder)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} pull {sourceFolder} {destinationFolder}",
            message: $"Downloading folder {sourceFolder} to {destinationFolder}");
    }

    public static void SetLogBufferSize(string deviceId, string memorySize)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} logcat -G {memorySize}",
            message: $"Setting log buffer size to {memorySize}");
    }

    public static double GetDensity(string deviceId)
    {
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell wm density",
            message: "Getting device density");

        var parsedDensity = Regex.Match(
                result.Output,
                "Physical density: (\\d+)")
            .Groups[1].Value;

        return int.Parse(parsedDensity) / 100.0;
    }

    private static bool IsEmulatorApkHashEqualLocalApkHash(string emulator, string path, string bundleId)
    {
        var localApkHash = CalculateLocalApkHash(path);
        Log.Write($"Local APK hash: {localApkHash}");

        var emulatorApkHash = GetEmulatorApkHash(emulator, bundleId);
        Log.Write($"Emulator APK hash: {emulatorApkHash}");

        return !string.IsNullOrEmpty(emulatorApkHash) &&
               !string.IsNullOrEmpty(localApkHash) &&
               emulatorApkHash.Equals(localApkHash, StringComparison.OrdinalIgnoreCase);
    }

    public static bool InstallApkIfNotInstalled(
        string emulator,
        string path,
        string bundleId)
    {
        using var logContext = Log.PushContext($"Installing Apk: {bundleId}, from: {path}");

        var isLatestVersionOfApplicationWasInstalled = IsAppInstalledWithSameBundleId(emulator, bundleId)
                                    && IsEmulatorApkHashEqualLocalApkHash(emulator, path, bundleId);

        if (!isLatestVersionOfApplicationWasInstalled)
        {
            RemoveApp(emulator, bundleId);
            InstallApp(emulator, path);
        }

        ValidateAppBundleFileHasProperBundleId(path, bundleId);
        if (!IsEmulatorApkHashEqualLocalApkHash(emulator, path, bundleId))
            throw new Exception("Hash mismatch after installation");

        return !isLatestVersionOfApplicationWasInstalled;
    }

    private static string CalculateLocalApkHash(string apkPath)
    {
        using var fileStream = new FileStream(apkPath, FileMode.Open, FileAccess.Read);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(fileStream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private static string GetEmulatorApkHash(string emulator, string bundleId)
    {
        using var logContext = Log.PushContext($"Getting APK hash from emulator for {bundleId}");
        try
        {
            var result = ExternalProgramRunner.Run(
                "adb",
                $"-s {emulator} shell pm path {bundleId}",
                message: $"Getting APK path for {bundleId}",
                assert: false);

            if (!result.IsSuccess || string.IsNullOrEmpty(result.Output))
            {
                Log.Write("Failed to get APK path on emulator");
                return string.Empty;
            }

            var apkPath = result.Output.Trim().Replace("package:", "").Split('\n')[0].Trim();
            if (string.IsNullOrEmpty(apkPath))
            {
                Log.Write("Invalid APK path format returned from emulator");
                return string.Empty;
            }

            result = ExternalProgramRunner.Run(
                "adb",
                $"-s {emulator} shell sha256sum {apkPath}",
                message: "Calculating APK hash on emulator",
                assert: false);

            if (!result.IsSuccess || string.IsNullOrEmpty(result.Output))
            {
                result = ExternalProgramRunner.Run(
                    "adb",
                    $"-s {emulator} shell md5sum {apkPath}",
                    message: "Calculating APK MD5 hash on emulator",
                    assert: false);

                if (!result.IsSuccess || string.IsNullOrEmpty(result.Output))
                {
                    Log.Write("Failed to calculate hash on emulator");
                    return string.Empty;
                }
            }

            var hashOutput = result.Output.Trim().Split(' ')[0].Trim();
            return hashOutput;
        }
        catch (Exception ex)
        {
            Log.Write($"Error getting emulator APK hash: {ex.Message}");
            return string.Empty;
        }
    }

    public static bool InstallAabIfNotInstalled(
        string emulator,
        string path,
        string bundleId)
    {
        if (!IsAppInstalledWithSameBundleId(emulator, bundleId))
        {
            Bundletool.InstallFromAab(path);
            return true;
        }

        Log.Write("App is already installed");

        return false;
    }

    private static void ValidateAppBundleFileHasProperBundleId(string path, string bundleId)
    {
        using var logContext = Log.PushContext($"Validating bundle {path} has bundle id '{bundleId}'");

        var fileBundleId = Aapt.GetAndroidManifest(path).PackageName;

        Log.Write($"Bundle ID in the .apk file: {fileBundleId}");

        if (fileBundleId != bundleId)
            throw new Exception("Bundle ID in the .apk file does not match the expected one");
    }

    public static void TurnOffAllInternet(string emulator)
    {
        TurnOffWiFiInternet(emulator);
        ThreadSleep.For(2.Seconds(), "Wait for WiFi internet to switch off");
        TurnOffCellularInternet(emulator);
        ThreadSleep.For(2.Seconds(), "Wait for Cellular internet to switch off");
    }

    
    public static void TurnOnWiFiInternet(string emulator)
    {
        if (IsWiFiOn(emulator))
            return;

        ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell svc wifi enable");
    }

    private static bool IsWiFiOn(string emulator)
    {
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell dumpsys wifi",
            log: false,
            message: "Checking if WiFi is on");

        return result.Output.Contains("Wi-Fi is enabled");
    }

    
    public static void TurnOnCellularInternet(string emulator)
    {
        if (IsCellularInternetOn(emulator))
            return;

        ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell svc data enable");
    }

    private static void TurnOffCellularInternet(string emulator)
    {
        if (!IsCellularInternetOn(emulator))
            return;

        ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell svc data disable",
            message: "Turning off cellular internet");
    }

    private static bool IsCellularInternetOn(string emulator)
    {
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell dumpsys telephony.registry",
            log: false,
            message: "Checking if cellular internet is on");

        return result.Output.Contains("mDataConnectionState=2");
    }

    private static void TurnOffWiFiInternet(string emulator)
    {
        if (!IsWiFiOn(emulator))
            return;

        ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell svc wifi disable",
            message: "Turning off WiFi internet");
    }

    private static bool IsAppInstalledWithSameBundleId(string emulator, string bundleId)
    {
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell pm list packages",
            log: false,
            message: "Getting list of installed apps");

        var appInstalled = result.Output.Contains(bundleId);

        Log.Write(
            appInstalled
                ? $"App {bundleId} is installed"
                : $"App {bundleId} is not installed");

        return appInstalled;
    }

    public static void OpenDeepLink(
        string emulator,
        string deepLink,
        string scheme,
        string deepLinkHost)
    {
        var fullDeepLink = $"{scheme}://{deepLinkHost}/{deepLink}";

        ExternalProgramRunner.Run(
            "adb",
            $"-s {emulator} shell am start -a android.intent.action.VIEW -d \"{fullDeepLink}\"",
            message: $"Opening deep link {fullDeepLink}");
    }

    public static string? GetOpenedAppBundleId(string androidId)
    {
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {androidId} shell dumpsys activity activities",
            log: false,
            message: "Getting opened app bundle ID");

        var resumedActivityLine = result.Output
            .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(l => l.Contains("ResumedActivity:"));

        var bundleId = resumedActivityLine?.Split(' ')
            .FirstOrDefault(l => l.Contains('/'))
            ?.Split('/')
            .FirstOrDefault();

        return bundleId;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static string GetDeviceName(string droidEmulatorId)
    {
        using var logContext = Log.PushContext(
            $"Reading virtual device ID for '{droidEmulatorId}'");

        EnsureDeviceVisible(droidEmulatorId);

        return ExternalProgramRunner
            .Run(
                "adb",
                $"-s {droidEmulatorId} shell getprop ro.boot.qemu.avd_name")
            .Output;
    }

    public static string GetDeviceAndroidApi(string emulatorId)
    {
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {emulatorId} shell getprop ro.build.version.sdk",
            message: "Getting device Android API");

        return result.Output;
    }

    public static void StartApp(string droidEmulatorId, string bundleId, bool log = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {droidEmulatorId} shell monkey -p {bundleId} -c android.intent.category.LAUNCHER 1",
            log: log,
            message: $"Launching app {bundleId}");
    }

    public static void TapCoordinates(string droidEmulatorId, int x, int y)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {droidEmulatorId} shell input tap {x} {y}",
            message: $"Tapping coordinates {x} {y}");
    }

    public static void SwitchDeviceUIModeToDark(bool waitAfter = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell cmd uimode night yes",
            message: "Toggle dark mode on device to 'Dark'");

        if (waitAfter)
            ThreadSleep.For(2.Second(), "Wait after switching device UI mode to dark");
    }

    public static void SwitchDeviceUIModeToLight(bool waitAfter = true)
    {
        ExternalProgramRunner.Run(
            "adb",
            "shell cmd uimode night no",
            message: "Toggle dark mode on device to 'Light'");

        if (waitAfter)
            ThreadSleep.For(2.Second(), "Wait after switching device UI mode to light");
    }

    public static void TurnOnAllInternet(string droidEmulatorId)
    {
        TurnOnWiFiInternet(droidEmulatorId);
        ThreadSleep.For(2.Seconds(), "Waiting for WiFi internet is off");
        TurnOnCellularInternet(droidEmulatorId);
        ThreadSleep.For(2.Seconds(), "Waiting for Cellular internet is off");
    }

    public static void Swipe(
        string droidEmulatorId,
        int fromXPx,
        int fromYPx,
        int toXPx,
        int toYPx,
        TimeSpan? duration = null)
    {
        if (fromXPx == toXPx && fromYPx == toYPx && duration != null)
            ExternalProgramRunner.Run(
                "adb",
                $"-s {droidEmulatorId} shell input swipe {fromXPx} {fromYPx} {toXPx} {toYPx} {duration.Value.TotalMilliseconds}",
                message: $"Tapping and Holding on {fromXPx} {fromYPx} with duration {duration.Value.TotalMilliseconds}");
        else if (duration == null)
            ExternalProgramRunner.Run(
                "adb",
                $"-s {droidEmulatorId} shell input swipe {fromXPx} {fromYPx} {toXPx} {toYPx}",
                message: $"Swiping from {fromXPx} {fromYPx} to {toXPx} {toYPx}");
        else
            ExternalProgramRunner.Run(
                "adb",
                $"-s {droidEmulatorId} shell input swipe {fromXPx} {fromYPx} {toXPx} {toYPx} {duration.Value.TotalMilliseconds}",
                message: $"Swiping from {fromXPx} {fromYPx} to {toXPx} {toYPx} with duration {duration.Value.TotalMilliseconds}");
    }

    public static (int Width, int Height) GetDeviceScreenSize(string deviceId)
    {
        var result = ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell wm size",
            message: "Getting device screen size");

        var match = Regex.Match(result.Output, @"Physical size: (\d+)x(\d+)")
                    ?? throw new InvalidOperationException("Failed to parse device screen size from output.");

        return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
    }

    public static string GetAppProcessId(string deviceId, string bundleId)
    {
        var processId = ExternalProgramRunner.Run(
                "adb",
                $"-s {deviceId} shell pidof {bundleId}",
                message: "Getting app logcat process ID")
            .Output;

        if (processId.IsNullOrEmpty())
            throw new Exception("Could not find process id for app with bundle id " + bundleId);

        return processId;
    }

    public static void DisableSpellChecker(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put secure spell_checker_enabled 0",
            message: "Disabling Spell Checker");
    }

    public static void SetOnScreenKeyboardShouldBeShown(string deviceId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put secure show_ime_with_hard_keyboard 1",
            message: "Setting on screen keyboard should be shown");
    }

    public static void SetHiddenApiPolicyPrePApps(string deviceId, int value)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global hidden_api_policy_pre_p_apps {value}",
            message: $"Setting hidden_api_policy_pre_p_apps to {value}");
    }

    public static void SetHiddenApiPolicyPApps(string deviceId, int value)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global hidden_api_policy_p_apps {value}",
            message: $"Setting hidden_api_policy_p_apps to {value}");
    }

    public static void SetHiddenApiPolicy(string deviceId, int value)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell settings put global hidden_api_policy {value}",
            message: $"Setting hidden_api_policy to {value}");
    }

    
    public static void DisablePackage(string deviceId, string packageBundleId)
    {
        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell pm disable-user {packageBundleId}",
            message: $"Disabling package {packageBundleId}");
    }

    public static void SetDebugApp(string deviceId, string bundleId)
    {
        RunCommand(deviceId, $"am set-debug-app --persistent {bundleId}");
    }
}