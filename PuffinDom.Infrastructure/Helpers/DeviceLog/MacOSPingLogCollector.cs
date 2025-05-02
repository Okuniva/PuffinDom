using System.Threading;
using PuffinDom.Tools.ExternalApplicationsTools;
using PuffinDom.Infrastructure;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Infrastructure.Helpers.DeviceLog;

internal class MacOSPingLogCollector : LogCollector
{
    public MacOSPingLogCollector(Platform platform, string name)
        : base(platform, name)
    {
    }

    private protected override void StartLogsThread(string deviceId, CancellationToken cancellationToken, params string[] args)
    {
        ExternalProgramRunner.Run(
            "ping",
            $"8.8.8.8 --apple-time -i {PuffinConstants.ProxiesPingDelay.Seconds.CastTo<int>()}",
            LogQueueAdd,
            collectOutput: false,
            message: "Mac os ping 8.8.8.8",
            log: false,
            cancellationToken: cancellationToken);
    }
}