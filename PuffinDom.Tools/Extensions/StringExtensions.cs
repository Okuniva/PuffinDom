using System.Diagnostics.CodeAnalysis;

namespace PuffinDom.Tools.Extensions;

public static class StringExtensions
{
    public static bool HasChars([NotNullWhen(true)] this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }
    
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }
}