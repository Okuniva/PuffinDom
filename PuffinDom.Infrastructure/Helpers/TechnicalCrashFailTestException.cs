using System;
using PuffinDom.Infrastructure;

namespace PuffinDom.Infrastructure.Helpers;

public class TechnicalCrashFailTestException : Exception
{
    public TechnicalCrashFailTestException(string message, Exception? exception = null)
        : base($"{PuffinConstants.CrashOfInstrumentationMessage} | {message}", exception)
    {
    }
}