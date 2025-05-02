using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.IOS.Types;
using PuffinDom.Tools.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Tools.IOS;

public static class XCodeCommandLine
{
    
    public static SimulatorDeviceList ListDevices(bool log = true)
    {
        var processResult = ExternalProgramRunner.Run(
            "xcrun",
            "simctl list -j devices",
            log: log,
            message: "Listing iOS devices");

        return JsonConvert.DeserializeObject<SimulatorDeviceList>(
            processResult.Output,
            new StringEnumConverter())!;
    }

    
    public static SimulatorRuntimesList ListRuntimes(bool log = true)
    {
        var processResult = ExternalProgramRunner.Run(
            "xcrun",
            "simctl list -j runtimes",
            log: log,
            message: "Listing iOS runtimes");

        return JsonConvert.DeserializeObject<SimulatorRuntimesList>(processResult.Output)!;
    }

    
    public static SimulatorDeviceTypesList ListDeviceTypes()
    {
        var processResult = ExternalProgramRunner.Run("xcrun", "simctl list -j devicetypes");

        return JsonConvert.DeserializeObject<SimulatorDeviceTypesList>(processResult.Output)!;
    }

    
    public static void BootDevice(string? deviceName)
    {
        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl boot {deviceName}",
            message: $"Booting iOS device {deviceName}");

        ExternalProgramRunner.Run(
            "open",
            "-a Simulator",
            message: "Opening Simulator application");

        if (deviceName != null)
            WaitForBoot(deviceName);
    }

    public static void EraseDevice(string deviceName, bool log = true, bool assert = true)
    {
        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl erase {deviceName}",
            log: log,
            assert: assert,
            message: $"Erasing iOS device {deviceName}");
    }

    public static void ShutdownAllDevices(bool log = true, bool assert = true)
    {
        ExternalProgramRunner.Run(
            "xcrun",
            "simctl shutdown all",
            log: log,
            assert: assert,
            message: "Shutting down all iOS devices");
    }

    public static void EraseAllDevices(bool log = true, bool assert = true)
    {
        ExternalProgramRunner.Run(
            "xcrun",
            "simctl erase all",
            log: log,
            assert: assert,
            message: "Erasing all iOS devices");
    }

    public static void ShutdownDevice(string? deviceName, bool log = true)
    {
        deviceName ??= "booted";

        if (IsDeviceBooted(deviceName, log))
            ExternalProgramRunner.Run(
                "xcrun",
                $"simctl shutdown {deviceName}",
                log: log,
                message: $"Shutting down {deviceName} device");
    }

    public static string GetBootedDeviceUdid()
    {
        var bootedDevice = ListDevices(false)
            .AllDevices
            .FirstOrDefault(x => x.State == "Booted");

        if (bootedDevice == null)
            throw new Exception("Could not find booted device");

        var bootedId = bootedDevice
            .UDID;

        Log.Write($"GetBootedDeviceUDID={bootedId}");
        return bootedId;
    }

    private static bool IsDeviceBooted(string deviceName, bool log = true)
    {
        if (deviceName == "booted")
            return ListDevices(log)
                .AllDevices
                .Any(x => x.State == "Booted");

        return ListDevices(log)
            .AllDevices
            .Any(
                x =>
                    x.Name == deviceName
                    && x.State == "Booted");
    }

    
    public static void UpgradeDevice(string deviceName, string runtimeIdentifier)
    {
        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl upgrade {deviceName} {runtimeIdentifier}",
            message: $"Upgrading device {deviceName} to {runtimeIdentifier}");
    }

    public static void StartApp(string? deviceName, string bundleId, bool log = true)
    {
        deviceName ??= "booted";

        if (IsDeviceBooted(deviceName, log))
            ExternalProgramRunner.Run(
                "xcrun",
                $"simctl launch {deviceName} {bundleId}",
                log: log,
                message: $"Starting application {bundleId} on device {deviceName}");
    }

    public static void InstallApp(string? deviceName, string pathToApp, bool log = true)
    {
        if (deviceName != null && !IsDeviceBooted(deviceName, log))
            BootDevice(deviceName);

        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl install {deviceName} \"{pathToApp}\"",
            log: log,
            message: $"Installing .app file '{pathToApp}' to device {deviceName}");
    }

    public static void UninstallApp(string deviceName, string bundleId, bool assert = true, bool log = true)
    {
        if (IsDeviceBooted(deviceName, log))
            ExternalProgramRunner.Run(
                "xcrun",
                $"simctl uninstall {deviceName} {bundleId}",
                assert: assert,
                log: log,
                message: $"Uninstalling application {bundleId} from device {deviceName}");
    }

    
    public static string CreateDevice(string deviceName, string deviceTypeIdentifier)
    {
        var processResult = ExternalProgramRunner.Run(
            "xcrun",
            $"simctl create {deviceName} {deviceTypeIdentifier}",
            message: $"Creating iOS device {deviceName} with type {deviceTypeIdentifier}");

        return processResult.Output;
    }

    public static string TakeScreenshot(
        string deviceName,
        string folder,
        string? fileName = null)
    {
        if (!IsDeviceBooted(deviceName, false))
            throw new InvalidOperationException("Device not booted");

        fileName ??= $"{DateTime.Now:HH-mm-ss-fff}";

        if (!Directory.Exists(folder))
        {
            Log.Write("Creating folder for screenshots: " + folder);
            Directory.CreateDirectory(folder);
        }

        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl io {deviceName} screenshot {folder}/{fileName}.png",
            message: $"Taking screenshot on device {deviceName}");

        return Path.Combine(folder, fileName);
    }

    
    public static void DeleteVirtualDevice(string deviceName)
    {
        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl delete {deviceName}",
            message: $"Deleting virtual device {deviceName}");
    }

    
    public static void WaitForBoot(string deviceName)
    {
        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl bootstatus {deviceName}",
            message: $"Waiting for iOS device {deviceName} to boot");
    }

    public static void CloseApp(string? id, string bundleId, bool assert = true)
    {
        id ??= "booted";

        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl terminate {id} {bundleId}",
            assert: assert,
            message: $"Closing application {bundleId} on device {id}");
    }

    public static void OpenDeepLink(string? id, string deepLink, string scheme, string deepLinkHost)
    {
        id ??= "booted";

        var fullDeepLink = $"{scheme}://{deepLinkHost}/{deepLink}";

        ExternalProgramRunner.Run(
            "xcrun",
            $"simctl openurl {id} \"{fullDeepLink}\"",
            message: $"Opening deep link {fullDeepLink} on device {id}");
    }

    public static string GetIOSDeviceIdentifier(string iOSSimulatorName, bool log = true)
    {
        foreach (var device in ListDevices(log).AllDevices)
            if (device.Name == iOSSimulatorName)
                return device.UDID;

        return string.Empty;
    }

    public static string DetectLatestInstalledIOSRuntime()
    {
        var runtimes = ListRuntimes(false).Runtimes;
        return runtimes.OrderByDescending(x => x.Version).First().Version;
    }

    
    public static void GoToBrowser(string? iosDeviceName)
    {
        iosDeviceName ??= "booted";

        OpenDeepLink(iosDeviceName, "", "https", "google.com");
    }

    public static void SwitchDeviceUIModeToDark()
    {
        ExternalProgramRunner.Run(
            "xcrun",
            "simctl ui booted appearance dark",
            message: "Toggle dark mode on device to 'Dark'");
    }

    public static void SwitchDeviceUIModeToLight()
    {
        ExternalProgramRunner.Run(
            "xcrun",
            "simctl ui booted appearance light",
            message: "Toggle dark mode on device to 'Light'");
    }

    public static ScreenInfo GetDeviceScreenInfo(string deviceName = "booted")
    {
        var result = ExternalProgramRunner.Run(
            "xcrun",
            $"simctl io {deviceName} enumerate",
            message: "Get touch simulator's params",
            log: false);

        var density = int.Parse(Regex.Match(result.Output, @"Preferred UI Scale: (\d+)").Groups[1].Value);

        var pixelSizeMatch = Regex.Match(result.Output, @"Pixel Size: \{(\d+), (\d+)\}");
        var pixelWidth = int.Parse(pixelSizeMatch.Groups[1].Value);
        var pixelHeight = int.Parse(pixelSizeMatch.Groups[2].Value);

        return new ScreenInfo(density, pixelWidth / density, pixelHeight / density);
    }

    public static string GetAppProcessId(string bundleId, string deviceName = "booted")
    {
        var processResult = ExternalProgramRunner.Run(
            "xcrun",
            $"simctl spawn {deviceName} launchctl list",
            message: $"Getting app process id on device {deviceName}",
            log: false);

        var lines = processResult.Output.Split(Environment.NewLine);

        foreach (var line in lines)
        {
            if (!line.Contains(bundleId))
                continue;

            var parts = line.Split("\t");

            var processId = parts[0];
            if (processId.IsNullOrEmpty() || processId == "-")
                throw new Exception("Could not find process id for app with bundle id " + bundleId);

            return processId;
        }

        throw new Exception("Could not find process id for app with bundle id " + bundleId);
    }
}