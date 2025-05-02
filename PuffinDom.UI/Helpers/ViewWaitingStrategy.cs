using PuffinDom.Infrastructure;
using PuffinDom.Infrastructure.Appium;
using PuffinDom.Tools;
using PuffinDom.Tools.Extensions;
using PuffinDom.Tools.Logging;
using PuffinDom.UI.Exceptions;
using PuffinDom.UI.Extensions;
using PuffinDom.UI.Views;

namespace PuffinDom.UI.Helpers;

public class ViewWaitingStrategy
{
    private const int DefaultMinimumTries = 3;

    private static ViewData? GetViewData(View view, string? customMessage = null)
    {
        using var logContext = Log.PushContext(
            $"Getting {view}" + (customMessage == null
                ? string.Empty
                : $" | {customMessage}"));

        var viewDataCollection = view.EvaluateQuery(customMessage);

        if (viewDataCollection.Any())
        {
            var resultView = viewDataCollection[0];
            Log.Write(viewDataCollection.GetFullDescriptionForLog());
            return resultView;
        }

        Log.Write($"{view} is not on screen");
        return null;
    }

    public static ViewData? WaitViewData(
        View view,
        TimeSpan? timeOut = null)
    {
        using var logContext = Log.PushContext(
            $"Waiting {view}" +
            $"{(!timeOut.HasValue ? string.Empty : $" with specific timeout {timeOut.Value.ToDisplayString()}")}");

        timeOut ??= CoreConstants.ViewWaitingTimeout;

        var waitStartTime = DateTime.Now;
        var currentTry = 0;
        while (waitStartTime + timeOut > DateTime.Now
               || DefaultMinimumTries - currentTry > 0)
        {
            var message = currentTry == 0
                ? null
                : $"Try #{currentTry + 1}";

            var viewData = GetViewData(view, message);

            if (viewData != null)
                return viewData;

            currentTry++;

            Sleep(currentTry);

            UIContext.Device.InvalidateCachedPageSource();
        }

        return null;
    }

    private static void Sleep(int currentTry)
    {
        var timeToSleep = currentTry switch
        {
            > 30 => 3.Seconds(),
            > 20 => 2.Seconds(),
            > 10 => 1.Second(),
            > 5 => 0.5.Seconds(),
            _ => CoreConstants.DefaultDelayBetweenViewExistingRechecks,
        };

        ThreadSleep.For(timeToSleep, "Delay between view existing rechecks");
    }

    public static bool WaitCondition(
        Func<bool> waitCondition,
        string conditionName,
        bool assert = false,
        TimeSpan? maxTime = null,
        string? customMessage = null,
        bool sleepAfterTry = true)
    {
        using var logContext = Log.PushContext($"Waiting condition for {conditionName}");

        maxTime ??= CoreConstants.ViewWaitingTimeout;
        var waitStartTime = DateTime.Now;
        var currentTry = 0;

        while (waitStartTime + maxTime > DateTime.Now
               || DefaultMinimumTries - currentTry > 0)
        {
            if (currentTry > 0)
                Log.Write($"Try #{currentTry + 1}");

            if (waitCondition())
                return true;

            Log.Write($"Condition {conditionName} is not met");

            currentTry++;


            if (sleepAfterTry)
                Sleep(currentTry);

            UIContext.Device.InvalidateCachedPageSource();
        }

        if (assert)
            throw new FailTestException($"Condition {conditionName} was not met", customMessage);

        return false;
    }

    public static bool WaitDisappeared(View view, TimeSpan? timeout = null, bool assert = true, string? customMessage = null)
    {
        timeout ??= CoreConstants.ViewDisappearingTimeout;

        using var logContext = Log.PushContext($"Waiting disappearing {view} in {timeout.Value.ToDisplayString()}");

        var waitStartTime = DateTime.Now;
        while (waitStartTime + timeout > DateTime.Now)
        {
            var viewData = GetViewData(view, "Disappearing");
            if (viewData == null)
            {
                Log.Write($"{view} disappeared");
                return true;
            }

            Log.Write("Found view. Continue waiting...");

            Sleep(0);

            UIContext.Device.InvalidateCachedPageSource();
        }

        Log.Write($"{view} not disappeared");

        if (assert)
            throw new ViewNotDisappearedException($"{view} is still visible but shouldn't be", customMessage);

        return false;
    }
}