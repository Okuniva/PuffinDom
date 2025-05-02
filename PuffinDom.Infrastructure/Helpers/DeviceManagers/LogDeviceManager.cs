using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using PuffinDom.Tools.Droid;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Infrastructure.Helpers.DeviceManagers;

public partial class DeviceManager
{
    private bool _loggingStarted;

    public bool WaitLogContains(
        string message,
        DateTime? logStartTime = null,
        bool assert = true,
        TimeSpan? timeout = null,
        Expression<Func<string, bool>>? logCustomCondition = null)
    {
        return WaitLogContains(message, out _, logStartTime, assert, timeout, logCustomCondition);
    }

    public bool WaitLogContains(
        string message,
        out string? foundLog,
        DateTime? logStartTime = null,
        bool assert = true,
        TimeSpan? timeout = null,
        Expression<Func<string, bool>>? logCustomCondition = null)
    {
        timeout ??= PuffinConstants.DefaultWaitLogContainsTimeout;
        var previousLogsIndex = 0;
        foundLog = string.Empty;
        string? tempLog = null;

        var result = WaitCondition(
            () =>
            {
                var logs = GetDeviceLogs();

                for (var i = logs.Count - 1; i >= previousLogsIndex; i--)
                    if (logs[i].Contains(message))
                    {
                        Log.Write($"Is contained in: {logs[i]}");

                        if (logStartTime != null)
                        {
                            Log.Write($"Log should be after time: {logStartTime:yyyy-MM-dd HH:mm:ss.ffffff}");
                            if (!IsLater(logs[i], (DateTime)logStartTime))
                                continue;
                        }

                        if (logCustomCondition != null)
                        {
                            Log.Write("Log should be resolved by customCondition");
                            if (!logCustomCondition.Compile()(logs[i]))
                                continue;
                        }

                        tempLog = logs[i];
                        return true;
                    }

                Log.Write($"Is not contained in: {logs.Count} log lines");
                previousLogsIndex = logs.Count;
                return false;
            },
            $"Wait Log Contains: {message}",
            timeout: timeout,
            assert: assert,
            customErrorMessage: () => $"Logs do not contain: {message}, after {timeout}, " +
                                      $"searched in {previousLogsIndex} log strings." +
                                      $"With condition {logCustomCondition}"
        );

        if (tempLog != null)
            foundLog = tempLog;

        return result;
    }

    public List<string> GetDeviceLogs(
        DateTime? startTime = null,
        Func<string, bool>? logCustomCondition = null,
        bool androidGetLogByAdb = true)
    {
        List<string> logs;
        if (Platform == Platform.Android && androidGetLogByAdb)
            logs = Adb.CollectLogs(PuffinEnvironmentVariables.DroidEmulatorId)
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        else
            logs = _deviceLogs.LogQueue.DistinctExcept("[]").ToList();

        var laterLogList = startTime == null
            ? logs
            : logs.SkipWhile(l => !IsLater(l, startTime.Value)).ToList();

        if (logCustomCondition == null)
            return laterLogList;

        return laterLogList
            .Where(logCustomCondition)
            .ToList();
    }

    public bool LogContains(
        string message,
        DateTime? logStartTime = null)
    {
        Log.Write($"Checking for log contains: {message}");

        return GetDeviceLogs(logStartTime).Any(x => x.Contains(message));
    }

    public bool LogContainsRegex(
        string regexPattern,
        DateTime? logStartTime = null)
    {
        Log.Write($"Checking for log contains regex: {regexPattern}");

        return GetDeviceLogs(logStartTime).Any(x => Regex.Match(x, regexPattern).Success);
    }

    public DateTime? GetDateFromLog(string log)
    {
        var minLogLength = Platform == Platform.iOS ? 26 : 18;
        if (log.Length < minLogLength)
        {
            Log.Write($"Failed to parse date from log: {log}");
            return null;
        }

        var dateString = log.Substring(0, minLogLength);
        var format = GetLogTimeFormat();

        if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return date;

        return null;
    }

    public bool IsLater(
        string log,
        DateTime time)
    {
        var parsedDate = GetDateFromLog(log);
        if (parsedDate == null)
            return false;

        Log.Write(
            $"IsLater Compare current log time = {parsedDate:yyyy-MM-dd HH:mm:ss.ffffff} and expected after time = {time:yyyy-MM-dd HH:mm:ss.ffffff}");

        var isLater = DateTime.Compare((DateTime)parsedDate, time) > 0;
        Log.Write($"isLater = {isLater}");

        return isLater;
    }

    public string GetLogTimeFormat()
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                return "MM-dd HH:mm:ss.fff";
            case Platform.iOS:
                return "yyyy-MM-dd HH:mm:ss.ffffff";
        }
    }

    public DeviceManager ClearLog()
    {
        if (Platform == Platform.Android)
            Adb.ClearLogcat(PuffinEnvironmentVariables.DroidEmulatorId);

        _deviceLogs.ClearLogs();

        return this;
    }

    [Obsolete("Use GetDeviceLogs(DateTime startTime) instead")]
    public string CollectLogs()
    {
        switch (Platform)
        {
            default:
            case Platform.Android:
                return Adb.CollectLogs(PuffinEnvironmentVariables.DroidEmulatorId);
            case Platform.iOS:
                throw new NotSupportedException("iOS logs collection is supported, by GetDeviceLogs method");
        }
    }

    public DeviceManager StartIOSDeviceLogStream(string iOSProcessName, bool getFullTouchLog)
    {
        if (Platform == Platform.Android)
            return this;

        if (_loggingStarted)
            throw new InvalidOperationException("Cannot start device logs while logging is in progress");

        _loggingStarted = true;

        _deviceLogs.StartLogStream(BootedDeviceUdid, iOSProcessName, getFullTouchLog.ToString());

        return this;
    }

    public DeviceManager StopDeviceLogStream()
    {
        using var logContext = Log.PushContext("Stopping Log Streams");

        _deviceLogs.StopLogStream();
        _macOsPingLogCollector.StopLogStream();

        foreach (var logStream in _devicePingLogs)
            logStream.Value.StopLogStream();

        _loggingStarted = false;
        return this;
    }

    public List<string> GetMacOSPingLogs()
    {
        return _macOsPingLogCollector.LogQueue;
    }

    public DeviceManager StartMacOSPingStream()
    {
        _macOsPingLogCollector.StartLogStream("");
        return this;
    }
}