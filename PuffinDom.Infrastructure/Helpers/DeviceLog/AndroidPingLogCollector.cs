using System;
using System.Threading;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Infrastructure;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Infrastructure.Helpers.DeviceLog;

internal class AndroidPingLogCollector : LogCollector
{
    public AndroidPingLogCollector(Platform platform, string name)
        : base(platform, name)
    {
        if (platform == Platform.iOS)
            throw new PlatformNotSupportedException();
    }

    private protected override void StartLogsThread(string deviceId, CancellationToken cancellationToken, params string[] args)
    {
        var url = args[0];

        ExternalProgramRunner.Run(
            "adb",
            $"-s {deviceId} shell ping -D -i {PuffinConstants.ProxiesPingDelay.Seconds.CastTo<int>()} {url}",
            LogQueueAdd,
            collectOutput: false,
            message: $"Ping {url}",
            log: false,
            cancellationToken: cancellationToken);
    }
}