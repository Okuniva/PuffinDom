namespace PuffinDom.UI.Exceptions;

public class ViewNotDisappearedException : FailTestException
{
    public ViewNotDisappearedException(string commonMessage, string? customMessage = null)
        : base(commonMessage, customMessage)
    {
    }

    // ReSharper disable once UnusedMember.Global
    public ViewNotDisappearedException(string message, Exception exception)
        : base(message, exception)
    {
    }
}