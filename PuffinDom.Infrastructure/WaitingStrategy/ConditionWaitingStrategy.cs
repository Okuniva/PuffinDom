using System;
using PuffinDom.Infrastructure.Helpers.DeviceManagers;
using PuffinDom.Tools;
using PuffinDom.Tools.Logging;
using PuffinDom.Infrastructure;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Infrastructure.WaitingStrategy;

public class ConditionWaitingStrategy
{
    private const int DefaultMinimumTries = 3;

    private static void Sleep(int currentTry)
    {
        var timeToSleep = currentTry switch
        {
            > 30 => 3.Seconds(),
            > 20 => 2.Seconds(),
            > 10 => 1.Second(),
            > 5 => 0.5.Seconds(),
            _ => PuffinConstants.DefaultDelayBetweenViewExistingRechecks,
        };

        ThreadSleep.For(timeToSleep, "Delay between condition recheck");
    }

    public static bool WaitCondition(
        DeviceManager deviceManager,
        Func<bool> waitCondition,
        string conditionName,
        bool assert = false,
        TimeSpan? timeout = null,
        Func<string>? customErrorMessage = null)
    {
        using var logContext = Log.PushContext($"Waiting condition for {conditionName}");

        timeout ??= PuffinConstants.ViewWaitingTimeout;
        var waitStartTime = DateTime.Now;
        var currentTry = 0;

        while (waitStartTime + timeout > DateTime.Now
               || DefaultMinimumTries - currentTry > 0)
        {
            if (currentTry > 0)
                Log.Write($"Try #{currentTry + 1}");

            if (waitCondition())
                return true;

            Log.Write($"Condition {conditionName} is not met");

            currentTry++;

            Sleep(currentTry);

            deviceManager.InvalidateCachedPageSource();
        }

        if (customErrorMessage != null)
            Log.Write(customErrorMessage.Invoke());

        if (assert)
            throw new TimeoutException($"Condition {conditionName} was not met. {customErrorMessage?.Invoke()}");

        return false;
    }
}