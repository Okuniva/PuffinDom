using System;
using System.Threading;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Tools;

public static class ThreadSleep
{
    public static void For(TimeSpan timeSpan, string customMessage)
    {
        var baseMessage = $"Sleeping for {timeSpan.ToDisplayString()}";
        Log.Write($"{baseMessage} | {customMessage}");

        Thread.Sleep(timeSpan);
    }
}