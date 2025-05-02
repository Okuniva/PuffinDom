using System.Collections.Generic;

namespace PuffinDom.Infrastructure.Helpers.DeviceLog;

public interface ILogCollector
{
    List<string> LogQueue { get; }

    public void StartLogStream(string deviceId, params string[] args);

    public void StopLogStream();

    public void ClearLogs();
}