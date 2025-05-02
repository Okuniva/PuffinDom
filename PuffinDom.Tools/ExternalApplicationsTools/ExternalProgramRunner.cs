using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using PuffinDom.Tools.ExternalApplicationsTools.Helpers;
using PuffinDom.Tools.Logging;
using PuffinDom.Tools.Extensions;

namespace PuffinDom.Tools.ExternalApplicationsTools;

public static class ExternalProgramRunner
{
    private static TimeSpan DefaultTimeout => 5.Minutes();

    public static ProcessResult Run(
        string path,
        string? arguments = null,
        Func<ProcessOutput, bool>? handleOutput = null,
        bool log = true,
        bool waitForExit = true,
        bool assert = true,
        TimeSpan? timeout = null,
        bool collectOutput = true,
        CancellationToken? cancellationToken = null,
        string? workingDirectory = null,
        string message = "Starting process")
    {
        var result = RunProcess(
            FindCommand(path),
            message,
            arguments,
            null,
            handleOutput,
            waitForExit,
            collectOutput,
            timeout,
            log,
            cancellationToken,
            workingDirectory);

        if (result.IsSuccess)
            Log.Write($"{message} completed in {result.Elapsed.ToDisplayString()}");
        else
        {
            Log.Write($"Failed {message} in {result.Elapsed.ToDisplayString()}");

            // ReSharper disable once InvertIf
            if (assert)
            {
                Log.Write($"Failed to execute: {path} {arguments} - with output: {result.Output}");
                const int maxOutputLength = 255;
                throw new ProcessResultException(
                    result,
                    $"Failed to execute: {path} {arguments} - exit code: {result.ExitCode}, " +
                    $"with first {maxOutputLength} output symbols: {result.Output.Substring(0, Math.Min(maxOutputLength, result.Output.Length))}",
                    result.ExitCode);
            }
        }

        return result;
    }

    public static ProcessResult RunWithInput(
        string input,
        string path,
        string? arguments = null,
        bool collectOutput = true,
        Func<ProcessOutput, bool>? handleOutput = null,
        bool assert = true,
        string message = "Starting process")
    {
        var result = RunProcess(FindCommand(path), message, arguments, input, handleOutput, collectOutput: collectOutput);

        if (assert && !result.IsSuccess)
            throw new ProcessResultException(
                result,
                $"Failed to execute: {path} {arguments} - exit code: {result.ExitCode}\n{result.Output}",
                result.ExitCode);

        Log.Write(message + result);
        Log.Write(result.Output);

        return result;
    }

    private static string FindCommand(string path, bool allowOSFallback = true)
    {
        if (File.Exists(path))
            return path;

        var exePath = Path.ChangeExtension(path, "exe");
        var noExtensionPath = Path.ChangeExtension(path, null);

        if (File.Exists(exePath))
            path = exePath;
        else if (File.Exists(noExtensionPath))
            path = noExtensionPath;
        else if (!allowOSFallback)
            throw new FileNotFoundException("Unable to find command file.", path);

        return path;
    }

    private static ProcessResult RunProcess(
        string path,
        string message,
        string? arguments = null,
        string? input = null,
        Func<ProcessOutput, bool>? handleOutput = null,
        bool waitForExit = true,
        bool collectOutput = true,
        TimeSpan? timeout = null,
        bool log = true,
        CancellationToken? cancellationToken = null,
        string? workingDirectory = null)
    {
        if (RunningOSTools.IsWindows)
            path = PreparePathForWindowsOS(path);

        var psi = new ProcessStartInfo
        {
            FileName = path,
            CreateNoWindow = true,
        };

        if (arguments != null)
            psi.Arguments = arguments;

        if (workingDirectory != null)
            psi.WorkingDirectory = workingDirectory;

        Log.Write($"{message} | {path} {arguments}");

        return psi.Run(
            input,
            collectOutput,
            handleOutput,
            waitForExit,
            timeout ?? DefaultTimeout,
            log,
            cancellationToken);
    }

    private static string PreparePathForWindowsOS(string path)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "where",
            CreateNoWindow = true,
            Arguments = path,
        };

        var outputPaths = processStartInfo.Run().GetOutput();

        path = outputPaths.FirstOrDefault(Path.HasExtension) ?? path;

        return path;
    }
}