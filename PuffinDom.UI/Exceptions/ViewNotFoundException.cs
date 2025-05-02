using PuffinDom.Infrastructure.Helpers;

namespace PuffinDom.UI.Exceptions;

public class ViewNotFoundException(string message, string? customMessage = null)
    : FailTestException(message, customMessage);