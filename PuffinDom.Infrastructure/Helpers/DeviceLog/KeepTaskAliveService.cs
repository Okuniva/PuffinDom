using System;
using System.Threading;
using System.Threading.Tasks;
using PuffinDom.Tools;
using PuffinDom.Tools.ExternalApplicationsTools.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.Infrastructure;

namespace PuffinDom.Infrastructure.Helpers.DeviceLog;

internal class KeepTaskAliveService
{
    public static void RunActionInSeparatedThreadAndKeepAlive(
        Action<CancellationToken> action,
        string taskName,
        CancellationToken cancellationToken)
    {
        Task.Run(
            () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                    try
                    {
                        action(cancellationToken);

                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Write($"OperationCanceledException happened in {taskName}");
                        break;
                    }
                    catch (ProcessResultException e)
                    {
                        Log.Write($"{nameof(ProcessResultException)} happened in {taskName}");
                        // ReSharper disable once InvertIf
                        if (e.ExitCode == 2)
                        {
                            Log.Write($"Invalid command, stooping collect logs for {taskName}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Write($"Exception Type: {e.GetType().FullName}");
                        Log.Write(
                            e,
                            $"Failed to run {taskName} " +
                            $"Restarting it in {PuffinConstants.TaskRerunDelay}",
                            false);

                        ThreadSleep.For(
                            PuffinConstants.TaskRerunDelay,
                            $"Wait some time before starting {taskName} once again");
                    }
            },
            cancellationToken);
    }
}