using System;

namespace PuffinDom.Tools.ExternalApplicationsTools.Helpers;

public class ProcessResultException : Exception
{
    public ProcessResultException(ProcessResult processResult, string? message, int exitCode)
        : base(message)
    {
        ProcessResult = processResult;
        ExitCode = exitCode;
    }

    public ProcessResultException(ProcessResult processResult, string? message, int? exitCode, Exception? innerException)
        : base(message, innerException)
    {
        ProcessResult = processResult;
        ExitCode = exitCode;
    }

    public ProcessResult ProcessResult { get; }
    public int? ExitCode { get; }
}