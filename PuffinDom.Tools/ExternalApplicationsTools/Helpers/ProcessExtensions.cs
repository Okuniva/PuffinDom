using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Tools.ExternalApplicationsTools.Helpers;

public static class ProcessExtensions
{
    private const int ThreadStillRunningExitCode = 259;
    private const int ThreadStillRunningRetry = 20;
    private static readonly TimeSpan _processFinishedCheckInterval = 30.Milliseconds();

    public static ProcessResult Run(
        this ProcessStartInfo processStartInfo,
        string? input = null,
        bool collectOutput = true,
        Func<ProcessOutput, bool>? handleOutput = null,
        bool waitForExit = true,
        TimeSpan? timeout = null,
        bool log = true,
        CancellationToken? cancellationToken = null)
    {
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;

        if (input != null)
            processStartInfo.RedirectStandardInput = true;

        var process = new Process
        {
            StartInfo = processStartInfo,
            EnableRaisingEvents = true,
        };

        var tcs = new TaskCompletionSource<ProcessResult>();

        var stopwatch = new Stopwatch();
        var output = new ConcurrentQueue<ProcessOutput>();

        var outputTcs = new TaskCompletionSource<bool>();
        var errorsTcs = new TaskCompletionSource<bool>();
        process.OutputDataReceived += HandleOutputData;
        process.ErrorDataReceived += HandleErrorData;

        process.Exited += HandleExited;

        if (timeout != null)
        {
            var timeoutCanselToken = new CancellationTokenSource();
            timeoutCanselToken.Token.Register(() => Terminate(true));
            timeoutCanselToken.CancelAfter(timeout.Value);
        }

        cancellationToken?.Register(
            () =>
            {
                Terminate();
                Detach();
            });

        stopwatch.Start();

        if (process.Start())
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        else
            tcs.TrySetException(new InvalidOperationException("Failed to start process."));

        if (input == null)
            return waitForExit
                ? tcs.Task.Result
                : new ProcessResult(null, 0, 0);

        var write = process.StandardInput.WriteLineAsync(input);
        Task.WhenAll(tcs.Task, write).Wait();

        return waitForExit
            ? tcs.Task.Result
            : new ProcessResult(null, 0, 0);

        void HandleExited(object? sender, EventArgs e)
        {
            for (var retries = 0; retries < ThreadStillRunningRetry && process.ExitCode == ThreadStillRunningExitCode; retries++)
                ThreadSleep.For(_processFinishedCheckInterval, "Delay between validation if process exited");

            var exitCode = process.ExitCode;
            if (exitCode == ThreadStillRunningExitCode)
                exitCode = 0;

            FinalizeTask(exitCode);
        }

        void HandleOutputData(object? sender, DataReceivedEventArgs? e)
        {
            if (e?.Data == null)
            {
                outputTcs.TrySetResult(true);
                return;
            }

            var o = new ProcessOutput(e.Data);

            if (handleOutput != null)
                try
                {
                    if (!handleOutput.Invoke(o))
                        Detach();
                }
                catch (OperationCanceledException)
                {
                    outputTcs.TrySetCanceled();
                    Terminate();
                    return;
                }
                catch (Exception ex)
                {
                    outputTcs.TrySetException(ex);
                    Terminate();
                    return;
                }

            if (collectOutput)
                output.Enqueue(o);

            if (log)
                Log.Write(o.ToString());
        }

        void HandleErrorData(object? sender, DataReceivedEventArgs? e)
        {
            if (e?.Data == null)
            {
                errorsTcs.TrySetResult(true);
                return;
            }

            var o = new ProcessOutput(e.Data, true);

            if (handleOutput != null)
                try
                {
                    if (!handleOutput.Invoke(o))
                        Detach();
                }
                catch (OperationCanceledException)
                {
                    errorsTcs.TrySetCanceled();
                    Terminate();
                    return;
                }
                catch (Exception ex)
                {
                    errorsTcs.TrySetException(ex);
                    Terminate();
                    return;
                }

            if (collectOutput)
                output.Enqueue(o);
        }

        void Terminate(bool cancel = false)
        {
            if (cancel)
                tcs.TrySetCanceled();

            if (process == null!)
                return;

            try
            {
                if (!process.HasExited)
                    process.Kill();
            }
            catch (InvalidOperationException)
            {
            }
        }

        void Detach()
        {
            process.Exited -= HandleExited;
            process.OutputDataReceived -= HandleOutputData;
            process.ErrorDataReceived -= HandleErrorData;

            HandleErrorData(null, null);
            HandleOutputData(null, null);

            FinalizeTask(0);
        }

        void FinalizeTask(int exitCode)
        {
            if (exitCode != 0)
                Log.Write($"Failed! Exit code: {exitCode}");

            try
            {
                Task.WhenAll(outputTcs.Task, errorsTcs.Task).Wait();

                var result = new ProcessResult(output.ToArray(), exitCode, stopwatch.ElapsedMilliseconds);
                process.Dispose();

                tcs.TrySetResult(result);
            }
            catch (Exception ex)
            {
                Log.Write($"FinalizeTask - The process threw an exception: {ex.Message}");
                var result = new ProcessResult(output.ToArray(), exitCode, stopwatch.ElapsedMilliseconds);

                tcs.TrySetException(
                    new ProcessResultException(
                        result,
                        $"The process threw an exception: {ex.Message}",
                        exitCode,
                        ex));
            }
        }
    }
}