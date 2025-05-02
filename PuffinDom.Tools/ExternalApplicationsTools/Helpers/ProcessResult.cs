using System;
using System.Collections.Generic;
using System.Text;
using PuffinDom.Tools.Logging;

namespace PuffinDom.Tools.ExternalApplicationsTools.Helpers;

public class ProcessResult
{
    private readonly ProcessOutput[] _outputItems;
    private StringBuilder? _outputBuilder;
    private string? _outputString;

    public ProcessResult(
        ProcessOutput[]? output,
        int exitCode,
        long elapsed)
    {
        _outputItems = output ?? [];
        ExitCode = exitCode;
        Elapsed = TimeSpan.FromMilliseconds(elapsed);
    }

    public string Output =>
        _outputString ??= GetOutputBuilder().ToString().Trim();

    public int ExitCode { get; }

    public bool IsSuccess => ExitCode == 0;

    public TimeSpan Elapsed { get; }

    public int OutputCount => _outputItems.Length;

    public IEnumerable<string> GetOutput()
    {
        foreach (var item in _outputItems)
            if (!item.IsError)
                yield return item.Data;
    }

    public IEnumerable<string> GetErrorOutput()
    {
        foreach (var item in _outputItems)
            if (item.IsError)
                yield return item.Data;
    }

    public override string ToString() =>
        $"Completed with exit code '{ExitCode}' in {Elapsed.ToDisplayString()}.";

    private StringBuilder GetOutputBuilder()
    {
        if (_outputBuilder != null)
            return _outputBuilder;

        var builder = new StringBuilder();
        foreach (var processOutput in _outputItems)
            if (processOutput != null!)
                builder.AppendLine(processOutput.Data);

        _outputBuilder = builder;

        return _outputBuilder;
    }
}