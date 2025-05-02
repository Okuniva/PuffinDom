using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PuffinDom.Tools.Droid.Enums;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.ExternalApplicationsTools.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Tools.Droid;

public static class EmulatorManager
{
    private const string ConsoleListeningRegex = "boot completed";
    private const string AdbConnectedRegex = "Hostapd main loop has stopped";
    private const string AdbConnectedAndroid21Regex = "adb connected";

    private static readonly Regex _alreadyBootedRegex =
        new(@"emulator: ERROR: Running multiple emulators with the same AVD is an experimental feature\.");

    public static void BootEmulator(
        string deviceName,
        EmulatorStartupFlags? emulatorFlags = null)
    {
        ArgumentNullException.ThrowIfNull(deviceName);

        using var logContext = Log.PushContext($"Booting virtual device '{deviceName}'");

        var args = $"-avd {deviceName} -verbose -no-metrics";
        if (emulatorFlags.HasValue && emulatorFlags.Value.HasFlag(EmulatorStartupFlags.NoWindow))
            args += " -no-boot-anim -no-window";

        if (emulatorFlags.HasValue && emulatorFlags.Value.HasFlag(EmulatorStartupFlags.NoSnapshots))
            args += " -no-snapshot";

        if (emulatorFlags.HasValue && emulatorFlags.Value.HasFlag(EmulatorStartupFlags.WipeData))
            args += " -wipe-data";

        try
        {
            ExternalProgramRunner.Run(
                "emulator",
                args,
                FindComplete,
                message: $"Booting virtual device {deviceName}");

            ThreadSleep.For(10.Seconds(), "Waiting for device to boot");
        }
        catch (ProcessResultException ex) when (IsAlreadyLaunched(ex))
        {
        }

        return;

        bool FindComplete(ProcessOutput output)
        {
            if (output is not { IsError: false, Data: { } outputLine })
                return true;

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (outputLine.ToLowerInvariant().Contains(ConsoleListeningRegex, StringComparison.InvariantCulture) ||
                outputLine.Contains(AdbConnectedRegex, StringComparison.InvariantCultureIgnoreCase) ||
                outputLine.Contains(AdbConnectedAndroid21Regex, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        static bool IsAlreadyLaunched(ProcessResultException ex)
        {
            foreach (var output in ex.ProcessResult.GetOutput())
            {
                var match = _alreadyBootedRegex.Match(output);
                if (match.Success)
                    return true;
            }

            return false;
        }
    }

    
    public static IEnumerable<string> GetVirtualDevices()
    {
        Log.Write("Retrieving all the virtual devices...");

        var result = ExternalProgramRunner.Run(
            "emulator",
            "-list-avds");

        var avd = new List<string>(result.OutputCount);
        foreach (var output in result.GetOutput())
            avd.Add(output.Trim());

        return avd;
    }
}