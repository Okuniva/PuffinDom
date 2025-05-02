using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.ExternalApplicationsTools.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Droid;

namespace PuffinDom.Tools.Droid;

public static class AVDManager
{
    private static readonly Regex _virtualDevicePathRegex = new(@"\s*Path\:\s*(.+)");
    private static readonly string[] _userDataFiles = ["userdata-qemu.img", "userdata-qemu.img.qcow2"];

    
    public static IEnumerable<DeviceBase> GetDevices()
    {
        var result = ExternalProgramRunner.Run(
            "avdmanager",
            "list device -c",
            message: "Retrieving devices list");

        var devices = new List<DeviceBase>();
        devices.AddRange(GetListResults(result).Select(output => new DeviceBase(output)));
        return devices;
    }

    
    public static IEnumerable<DeviceTarget> GetTargets()
    {
        Log.Write("Retrieving all the device targets");

        var result = ExternalProgramRunner.Run(
            "avdmanager",
            "list target -c",
            message: "Retrieving device targets list");

        var targets = new List<DeviceTarget>(result.OutputCount);
        foreach (var output in GetListResults(result))
            targets.Add(new DeviceTarget(output));

        return targets;
    }

    
    public static void ResetVirtualDevice(string id)
    {
        Log.Write($"Resetting virtual device '{id}'...");

        var avdPath = GetVirtualDevicePath(id);

        foreach (var file in _userDataFiles)
        {
            var filePath = Path.Combine(avdPath, file);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    
    public static IEnumerable<string> GetVirtualDeviceIds()
    {
        Log.Write("Retrieving all the virtual devices");

        var result = ExternalProgramRunner.Run(
            "avdmanager",
            "list avd",
            message: "Retrieving virtual devices list");

        var avds = new List<string>();

        foreach (var output in GetListResults(result))
        {
            var pathMatch = _virtualDevicePathRegex.Match(output);
            if (!pathMatch.Success)
                continue;

            var path = pathMatch.Groups[1].Value;
            if (!Directory.Exists(path))
                continue;

            var avd = Path.GetFileNameWithoutExtension(path);
            avds.Add(avd);
        }

        return avds;
    }

    
    public static string GetVirtualDevicePath(string id)
    {
        Log.Write("Retrieving all the virtual devices...");

        var result = ExternalProgramRunner.Run(
            "avdmanager",
            "list avd",
            message: "Retrieving virtual devices list");

        foreach (var output in GetListResults(result))
        {
            var pathMatch = _virtualDevicePathRegex.Match(output);

            if (!pathMatch.Success)
                continue;

            var path = pathMatch.Groups[1].Value;
            var avdId = Path.GetFileNameWithoutExtension(path);
            if (avdId.Equals(id, StringComparison.OrdinalIgnoreCase) && Directory.Exists(path))
                return path;
        }

        throw new Exception($"Virtual device '{id}' does not exist.");
    }

    public static IEnumerable<VirtualDevice> GetVirtualDevices()
    {
        Log.Write("Retrieving all the virtual devices...");

        var result = ExternalProgramRunner.Run(
            "avdmanager",
            "list avd",
            message: "Retrieving virtual devices list");

        var avds = new List<VirtualDevice>();

        foreach (var output in GetListResults(result))
        {
            var pathMatch = _virtualDevicePathRegex.Match(output);
            if (!pathMatch.Success)
                continue;

            var path = pathMatch.Groups[1].Value;
            var configIniPath = Path.Combine(path, "config.ini");
            if (!Directory.Exists(path) || !File.Exists(configIniPath))
                continue;

            var config = new VirtualDeviceConfig(path);
            var avd = config.CreateVirtualDevice();

            avds.Add(avd);
        }

        return avds;
    }

    public static void DeleteVirtualDevice(string deviceId)
    {
        Log.Write($"Deleting virtual device '{deviceId}'");

        try
        {
            ExternalProgramRunner.Run(
                "avdmanager",
                $"delete avd --name \"{deviceId}\"",
                message: "Deleting virtual devices");
        }
        catch (ProcessResultException ex) when (WasExisting(ex.ProcessResult))
        {
            Log.Write($"Virtual device '{deviceId}' does not exist.");
        }

        return;

        bool WasExisting(ProcessResult result)
        {
            var expected = $"Error: There is no Android Virtual Device with ID '{deviceId}'.";

            return result.GetErrorOutput()
                .Any(output => output.Contains(expected, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static void CreateVirtualDevice(string id, string package, bool overwrite = true)
    {
        Log.Write($"Creating virtual device '{id}'...");

        var args = $"create avd --name \"{id}\" --package \"{package}\"";
        if (overwrite)
            args += " --force";

        try
        {
            ExternalProgramRunner.RunWithInput("no", "avdmanager", args);
        }
        catch (ProcessResultException ex) when (WasExisting(ex.ProcessResult))
        {
            Log.Write($"Virtual device '{id}' already exists.");
        }

        return;

        bool WasExisting(ProcessResult result)
        {
            var expected = $"Error: Android Virtual Device '{id}' already exists.";

            return result.GetErrorOutput()
                .Any(output => output.Contains(expected, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static IEnumerable<string> GetListResults(ProcessResult result)
    {
        foreach (var output in result.GetOutput())
        {
            if (string.IsNullOrWhiteSpace(output) || output.StartsWith('[') || output.StartsWith("Loading "))
                continue;

            var o = output;

            const string p = "package.xml";
            if (output.StartsWith("Parsing "))
            {
                if (output.EndsWith(p))
                    continue;

                o = output.Substring(output.LastIndexOf(p, StringComparison.Ordinal) + p.Length);
                if (string.IsNullOrWhiteSpace(o))
                    continue;
            }

            yield return o;
        }
    }

    public static void ReplaceConfigWithPerformanceConfig(string configFileContent, string deviceName)
    {
        var pathToEmulatorConfig = Path.Combine(
            Environment.GetEnvironmentVariable("HOME")!,
            ".android",
            "avd",
            $"{deviceName}.avd",
            "config.ini");

        File.WriteAllText(
            pathToEmulatorConfig,
            configFileContent);
    }

    public static void CloseEmulators()
    {
        try
        {
            ExternalProgramRunner.Run(
                "adb",
                "emu kill",
                message: "Closing all Emulators");
        }
        catch (Exception e)
        {
            if (e.Message.Contains("more than one emulator detected"))
            {
                var getDevices = ExternalProgramRunner.Run(
                    "adb",
                    "devices",
                    message: "Getting devices list");

                var devicesOutput = getDevices.Output;
                var lines = devicesOutput.Split('\n');
                var emulatorList =
                    (from line in lines where line.Contains("emulator") select line.Split('\t') into words select words[0]).ToList();

                try
                {
                    foreach (var emulator in emulatorList)
                        ExternalProgramRunner.Run(
                            "adb",
                            $"-s {emulator} emu kill",
                            message: $"Killing {emulator} emulator");
                }
                catch (Exception x)
                {
                    Log.Write($"Couldn't close device \n {x.Message}");
                }
            }
            else
                Log.Write($"Couldn't close device \n {e.Message}");
        }
    }
}