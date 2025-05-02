using NUnit.Framework;

namespace PuffinDom.UI.Exceptions;

public class FailTestException : AssertionException
{
    public FailTestException(string commonMessage, string? customMessage = null)
        : base(CombineCommonAndCustomMessages(commonMessage, customMessage))
    {
    }

    public FailTestException(string message, Exception exception)
        : base(message, exception)
    {
    }

    private static string CombineCommonAndCustomMessages(
        string commonMessage,
        string? customMessage)
    {
        return customMessage != null
            ? $"{customMessage} | {commonMessage}"
            : commonMessage;
    }
}