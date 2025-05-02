using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PuffinDom.Tools.ExternalApplicationsTools.Helpers;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Infrastructure.Helpers.DeviceLog;

public abstract class LogCollector : ILogCollector
{
    private readonly ConcurrentQueue<string> _logQueue = [];
    private readonly string _name;
    private protected readonly Platform Platform;
    private CancellationTokenSource? _logStreamCancellationTokenSource;

    protected LogCollector(Platform platform, string name)
    {
        Platform = platform;
        _name = name;
    }

    public List<string> LogQueue => _logQueue.ToList();

    public void StartLogStream(string deviceId, params string[] args)
    {
        Log.Write("Thread id: " + Environment.CurrentManagedThreadId);

        if (deviceId == null)
            throw new ArgumentException("Device ID is required");

        Log.Write($"Start Log Stream {_name} → {deviceId}");
        _logStreamCancellationTokenSource = new CancellationTokenSource();

        KeepTaskAliveService.RunActionInSeparatedThreadAndKeepAlive(
            cancellationToken => StartLogsThread(deviceId, cancellationToken, args),
            "Device Logs Collector",
            _logStreamCancellationTokenSource.Token);
    }

    public void StopLogStream()
    {
        Log.Write("Thread id: " + Environment.CurrentManagedThreadId);

        Log.Write($"Stop Log Stream for {_name}");
        _logStreamCancellationTokenSource?.Cancel();
    }

    public void ClearLogs()
    {
        Log.Write("Thread id: " + Environment.CurrentManagedThreadId);

        Log.Write($"Clear Logs for {_name}");

        _logQueue.Clear();
    }

    private protected abstract void StartLogsThread(string deviceId, CancellationToken cancellationToken, params string[] args);

    protected bool LogQueueAdd(ProcessOutput arg)
    {
        _logQueue.Enqueue(arg.Data);

        return true;
    }
}