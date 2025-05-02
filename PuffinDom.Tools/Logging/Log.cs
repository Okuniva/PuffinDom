using System;
using System.Diagnostics;
using Core.Tools.Disposables;

namespace PuffinDom.Tools.Logging;

public class Log
{
    private static int _noLogsCount;
    private static int _currentPadding;

    public static IDisposable PushContext(string message)
    {
        LogMessage(string.Empty);
        Write(message);

        var startTime = DateTime.Now;
        _currentPadding++;

        return new DisposableObject().WhenDisposed(
            () =>
            {
                var passedTime = DateTime.Now - startTime;
                Write($"Finished in {passedTime.ToDisplayString()} | " + message);
                LogMessage(string.Empty);
                _currentPadding--;
            });
    }

    public static void Write(string message, bool ignoreLogsDisabling = false)
    {
        var timeStamp = DateTime.Now.ToString("HH:mm:ss.ff");

        var logMessage = $"{GetPaddingString()}{timeStamp} {AddPaddingsToEachStringOfMessage(message)}";

        LogMessage(logMessage, ignoreLogsDisabling);
        return;

        string AddPaddingsToEachStringOfMessage(string userLogMessage)
        {
            var paddingString = GetPaddingString();
            return userLogMessage.Replace("\n", $"\n{paddingString}");
        }
    }

    [Conditional("DEBUG")]
    public static void WriteInDebug(string message) => Write(message, true);

    private static string GetPaddingString()
    {
        var paddingString = string.Empty;
        for (var i = 0; i < _currentPadding; i++)
            paddingString += "    ";

        return paddingString;
    }

    // ReSharper disable once RedundantAssignment
    private static void LogMessage(string logMessage, bool ignoreLogsDisabling = false)
    {
#if DEBUG
        ignoreLogsDisabling = true;
#endif

        if (_noLogsCount > 0 && !ignoreLogsDisabling)
            return;

        Console.WriteLine(logMessage);

        // TestContext.Progress.WriteLine(logMessage); ToDo add nunit
    }

    public static void Write(Exception exception, string message, bool attachDebugger = true)
    {
#if DEBUG
        if (attachDebugger)
            if (Debugger.IsAttached)
                Debugger.Break();
#endif

        Write(message);

        var fullName = exception.GetType().FullName;
        if (!string.IsNullOrEmpty(fullName))
            Write(fullName);

        Write(exception.Message);

        if (exception.StackTrace != null)
            Write(exception.StackTrace);
    }

    public static IDisposable GetDangerousDisposableNoLogsContext(string reason)
    {
        var startTime = DateTime.Now;

        LogMessage($"Logs Off for {reason}");

        _noLogsCount++;

        return new DisposableObject().WhenDisposed(
            () =>
            {
                var passedTime = DateTime.Now - startTime;
                _noLogsCount--;
                LogMessage($"Logs On after {reason} in {passedTime.ToDisplayString()}");
            });
    }
}