namespace PuffinDom.UI.Exceptions;

public class ViewAssertionException : FailTestException
{
    public ViewAssertionException(string commonMessage, string? customMessage = null)
        : base(commonMessage, customMessage)
    {
    }

    public ViewAssertionException(string message, Exception exception)
        : base(message, exception)
    {
    }
}