using System;

namespace PuffinDom.Tools.IOS;

[Serializable]
public class XCodeException : Exception
{
    public XCodeException()
    {
    }

    public XCodeException(string message)
        : base(message)
    {
    }

    public XCodeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}