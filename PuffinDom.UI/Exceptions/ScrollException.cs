using PuffinDom.Infrastructure.Helpers;

namespace PuffinDom.UI.Exceptions;

public class ScrollException : FailTestException
{
    public ScrollException(string commonMessage, string? customMessage = null) : base(commonMessage, customMessage)
    {
    }

    public ScrollException(string message, Exception exception) : base(message, exception)
    {
    }
}