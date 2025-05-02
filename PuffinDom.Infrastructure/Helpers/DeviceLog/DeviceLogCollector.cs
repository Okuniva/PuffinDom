using System;
using System.Threading;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Infrastructure.Helpers.DeviceLog;

public class DeviceLogCollector : LogCollector
{
    public DeviceLogCollector(Platform platform)
        : base(platform, "Main Logs")
    {
    }

    private protected override void StartLogsThread(string deviceId, CancellationToken cancellationToken, params string[] args)
    {
        Log.Write("Thread id: " + Environment.CurrentManagedThreadId);

        switch (Platform)
        {
            default:
            case Platform.Android:
                ExternalProgramRunner.Run(
                    "adb",
                    $"-s {deviceId} logcat",
                    LogQueueAdd,
                    collectOutput: false,
                    message: "Android Device log stream",
                    log: false,
                    cancellationToken: cancellationToken);

                break;
            case Platform.iOS:
                var processName = args[0];
                var getFullLog = bool.Parse(args[1]);
                ExternalProgramRunner.Run(
                    "xcrun",
                    $"simctl spawn {deviceId} log stream " +
                    (getFullLog
                        ? ""
                        : $"--predicate \"process == '{processName}' && NOT (senderImagePath CONTAINS 'XCTAutomationSupport')\" --timeout 30m"),
                    LogQueueAdd,
                    collectOutput: false,
                    message: "iOS Device log stream",
                    log: false,
                    cancellationToken: cancellationToken);

                break;
        }
    }
}